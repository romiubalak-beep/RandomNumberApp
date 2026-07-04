using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Provider;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView textView;
    private Button startButton;
    private bool isRunning = false;
    private bool stopOnNextFound = false;
    private RandomNumberGenerator rng;
    private System.Threading.CancellationTokenSource cancellationToken;
    private BroadcastReceiver shuffleReceiver;
    private AccessibilityService accessibilityService;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        rng = RandomNumberGenerator.Create();
        cancellationToken = new System.Threading.CancellationTokenSource();
        
        CheckOverlayPermission();
        
        LinearLayout layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetPadding(50, 50, 50, 50);
        layout.SetGravity(GravityFlags.Center);
        
        textView = new TextView(this);
        textView.Text = "استخدم الزر العائم للتحكم بالخلط";
        textView.TextSize = 20;
        textView.SetTextColor(Color.Black);
        
        startButton = new Button(this);
        startButton.Text = "بدء الخلط (يدوي)";
        startButton.SetTextColor(Color.White);
        startButton.SetBackgroundColor(Color.Blue);
        startButton.Click += StartShuffling;
        
        layout.AddView(textView);
        layout.AddView(startButton);
        
        SetContentView(layout);
        
        // استقبال إشارات من الزر العائم
        shuffleReceiver = new ShuffleBroadcastReceiver();
        IntentFilter filter = new IntentFilter();
        filter.AddAction("START_SHUFFLING");
        filter.AddAction("STOP_SHUFFLING");
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            RegisterReceiver(shuffleReceiver, filter, ReceiverFlags.NotExported);
        }
        else
        {
            RegisterReceiver(shuffleReceiver, filter);
        }
        
        // بدء خدمة الزر العائم
        Handler handler = new Handler(Looper.MainLooper);
        handler.PostDelayed(() => {
            try
            {
                Intent serviceIntent = new Intent(this, typeof(FloatingButtonService));
                StartService(serviceIntent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "خطأ في بدء الخدمة: " + ex.Message, ToastLength.Long).Show();
            }
        }, 2000);
    }

    private class ShuffleBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var activity = (MainActivity)context;
            if (intent.Action == "START_SHUFFLING")
            {
                if (!activity.isRunning)
                {
                    activity.isRunning = true;
                    activity.stopOnNextFound = false;
                    activity.startButton.Text = "⏹ إيقاف الخلط";
                    activity.cancellationToken = new System.Threading.CancellationTokenSource();
                    activity.StartFastShuffling(activity.cancellationToken.Token);
                }
            }
            else if (intent.Action == "STOP_SHUFFLING")
            {
                if (activity.isRunning)
                {
                    activity.isRunning = false;
                    activity.startButton.Text = "▶ بدء الخلط";
                    activity.cancellationToken.Cancel();
                    activity.textView.Text = "⏹ تم إيقاف الخلط";
                }
            }
        }
    }

    private void CheckOverlayPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            if (!Settings.CanDrawOverlays(this))
            {
                Intent intent = new Intent(Settings.ActionManageOverlayPermission,
                    Android.Net.Uri.Parse("package:" + PackageName));
                StartActivity(intent);
                Toast.MakeText(this, "الرجاء تفعيل صلاحية العرض فوق التطبيقات", ToastLength.Long).Show();
            }
        }
    }

    private void StartShuffling(object sender, EventArgs e)
    {
        if (!isRunning)
        {
            isRunning = true;
            stopOnNextFound = false;
            startButton.Text = "⏹ إيقاف الخلط";
            cancellationToken = new System.Threading.CancellationTokenSource();
            StartFastShuffling(cancellationToken.Token);
        }
        else
        {
            isRunning = false;
            startButton.Text = "▶ بدء الخلط";
            cancellationToken.Cancel();
            textView.Text = "⏹ تم إيقاف الخلط";
        }
    }

    private async void StartFastShuffling(System.Threading.CancellationToken token)
    {
        List<int> numbers = new List<int>();
        for (int i = 1; i <= 150; i++) numbers.Add(i);
        
        try
        {
            while (isRunning && !token.IsCancellationRequested)
            {
                int n = numbers.Count;
                for (int i = n - 1; i > 0; i--)
                {
                    byte[] bytes = new byte[4];
                    rng.GetBytes(bytes);
                    int j = Math.Abs(BitConverter.ToInt32(bytes, 0) % (i + 1));
                    int temp = numbers[i];
                    numbers[i] = numbers[j];
                    numbers[j] = temp;
                }
                
                string result = "🔄 الخلط:\n";
                for (int i = 0; i < Math.Min(10, numbers.Count); i++)
                {
                    result += numbers[i] + " ";
                }
                textView.Text = result;
                
                // ✅ التحقق من الرقم المستهدف (1، 2، 3)
                if (numbers[0] == 1 || numbers[0] == 2 || numbers[0] == 3)
                {
                    RunOnUiThread(() => {
                        Toast.MakeText(this, "🎯 تم العثور على الرقم: " + numbers[0], ToastLength.Short).Show();
                    });
                    
                    // ✅ تغيير لون الزر العائم
                    try
                    {
                        Intent colorIntent = new Intent("CHANGE_FLOATING_BUTTON_COLOR");
                        SendBroadcast(colorIntent);
                    }
                    catch (Exception ex)
                    {
                        Android.Util.Log.Error("MainActivity", "Broadcast Error: " + ex.Message);
                    }
                    
                    // ✅ النقر على منتصف الشاشة (محاكاة نقرة)
                    PerformClickInCenter();
                    
                    // ✅ إيقاف الخلط بعد 1 ثانية
                    stopOnNextFound = true;
                    await System.Threading.Tasks.Task.Delay(1000);
                    
                    if (stopOnNextFound && isRunning)
                    {
                        isRunning = false;
                        stopOnNextFound = false;
                        cancellationToken.Cancel();
                        RunOnUiThread(() => {
                            startButton.Text = "▶ بدء الخلط";
                            textView.Text = "⏹ تم إيقاف الخلط (تم العثور على الرقم)";
                            Toast.MakeText(this, "⏹ تم إيقاف الخلط", ToastLength.Short).Show();
                        });
                        
                        // ✅ تحديث الزر العائم إلى وضع الإيقاف
                        Intent stopIntent = new Intent("STOP_SHUFFLING");
                        SendBroadcast(stopIntent);
                        break; // الخروج من الحلقة
                    }
                }
                
                await System.Threading.Tasks.Task.Delay(100, token);
            }
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            // تم الإلغاء بشكل طبيعي
        }
        catch (Exception ex)
        {
            RunOnUiThread(() => {
                Toast.MakeText(this, "❌ خطأ: " + ex.Message, ToastLength.Long).Show();
            });
            Android.Util.Log.Error("MainActivity", "Shuffling Error: " + ex.Message);
        }
        finally
        {
            if (!isRunning)
            {
                RunOnUiThread(() => {
                    textView.Text = "⏹ تم إيقاف الخلط";
                });
            }
        }
    }

    // ✅ دالة لمحاكاة النقر في منتصف الشاشة
    private void PerformClickInCenter()
    {
        try
        {
            // الحصول على حجم الشاشة
            var display = WindowManager.DefaultDisplay;
            var size = new Point();
            display.GetSize(size);
            int centerX = size.X / 2;
            int centerY = size.Y / 2;
            
            // إنشاء MotionEvent للنقر
            long downTime = SystemClock.UptimeMillis();
            long eventTime = SystemClock.UptimeMillis();
            
            var downEvent = MotionEvent.Obtain(downTime, eventTime, MotionEventActions.Down, centerX, centerY, 0);
            var upEvent = MotionEvent.Obtain(downTime, eventTime + 100, MotionEventActions.Up, centerX, centerY, 0);
            
            // إرسال الحدث إلى النافذة الحالية
            WindowManager.DefaultDisplay.?.DispatchPointerEvent(downEvent);
            WindowManager.DefaultDisplay.?.DispatchPointerEvent(upEvent);
            
            downEvent.Recycle();
            upEvent.Recycle();
            
            RunOnUiThread(() => {
                Toast.MakeText(this, "👆 تم النقر في منتصف الشاشة", ToastLength.Short).Show();
            });
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MainActivity", "Click Error: " + ex.Message);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        try
        {
            UnregisterReceiver(shuffleReceiver);
        }
        catch (Exception) { }
    }
}
