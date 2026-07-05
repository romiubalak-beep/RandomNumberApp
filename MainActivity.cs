using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Provider;
using System.Security.Cryptography;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView textView;
    private Button startButton;
    private bool isRunning = false;
    private RandomNumberGenerator rng;
    private System.Threading.CancellationTokenSource cancellationToken;
    private BroadcastReceiver shuffleReceiver;
    private bool isShufflingActive = false;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        rng = RandomNumberGenerator.Create();
        cancellationToken = new System.Threading.CancellationTokenSource();
        
        CheckOverlayPermission();
        CheckAccessibilityPermission();
        
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
        
        shuffleReceiver = new ShuffleBroadcastReceiver();
        IntentFilter filter = new IntentFilter();
        filter.AddAction("START_SHUFFLING");
        filter.AddAction("STOP_SHUFFLING");
        filter.AddAction("PERFORM_TAP");
        filter.AddAction("UPDATE_STATUS");
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            RegisterReceiver(shuffleReceiver, filter, ReceiverFlags.NotExported);
        }
        else
        {
            RegisterReceiver(shuffleReceiver, filter);
        }
        
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

    private void CheckAccessibilityPermission()
    {
        try
        {
            Intent intent = new Intent(Android.Provider.Settings.ActionAccessibilitySettings);
            StartActivity(intent);
            Toast.MakeText(this, "الرجاء تفعيل خدمة إمكانية الوصول للتطبيق", ToastLength.Long).Show();
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MainActivity", "Accessibility Error: " + ex.Message);
        }
    }

    private class ShuffleBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var activity = (MainActivity)context;
            if (intent.Action == "START_SHUFFLING")
            {
                activity.StartShufflingFromFloatingButton();
            }
            else if (intent.Action == "STOP_SHUFFLING")
            {
                activity.StopShufflingFromFloatingButton();
            }
            else if (intent.Action == "PERFORM_TAP")
            {
                int x = intent.GetIntExtra("x", 0);
                int y = intent.GetIntExtra("y", 0);
                activity.PerformTapOnCenter();
            }
            else if (intent.Action == "UPDATE_STATUS")
            {
                bool status = intent.GetBooleanExtra("isRunning", false);
                activity.isShufflingActive = status;
                activity.RunOnUiThread(() => {
                    if (status)
                    {
                        activity.startButton.Text = "⏹ إيقاف الخلط";
                        activity.textView.Text = "🔄 الخلط قيد التشغيل...";
                    }
                    else
                    {
                        activity.startButton.Text = "▶ بدء الخلط";
                        activity.textView.Text = "⏹ تم إيقاف الخلط";
                    }
                });
            }
        }
    }

    private void StartShufflingFromFloatingButton()
    {
        if (!isRunning)
        {
            isRunning = true;
            isShufflingActive = true;
            RunOnUiThread(() => {
                startButton.Text = "⏹ إيقاف الخلط";
                textView.Text = "🔄 الخلط قيد التشغيل...";
            });
            cancellationToken = new System.Threading.CancellationTokenSource();
            StartFastShuffling(cancellationToken.Token);
        }
    }

    private void StopShufflingFromFloatingButton()
    {
        if (isRunning)
        {
            isRunning = false;
            isShufflingActive = false;
            cancellationToken.Cancel();
            RunOnUiThread(() => {
                startButton.Text = "▶ بدء الخلط";
                textView.Text = "⏹ تم إيقاف الخلط";
            });
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
            isShufflingActive = true;
            startButton.Text = "⏹ إيقاف الخلط";
            cancellationToken = new System.Threading.CancellationTokenSource();
            StartFastShuffling(cancellationToken.Token);
            // إبلاغ الزر العائم بحالة الخلط
            Intent statusIntent = new Intent("UPDATE_FLOATING_STATUS");
            statusIntent.PutExtra("isRunning", true);
            SendBroadcast(statusIntent);
        }
        else
        {
            isRunning = false;
            isShufflingActive = false;
            startButton.Text = "▶ بدء الخلط";
            cancellationToken.Cancel();
            textView.Text = "⏹ تم إيقاف الخلط";
            // إبلاغ الزر العائم بحالة الخلط
            Intent statusIntent = new Intent("UPDATE_FLOATING_STATUS");
            statusIntent.PutExtra("isRunning", false);
            SendBroadcast(statusIntent);
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
                RunOnUiThread(() => {
                    textView.Text = result;
                });
                
                if (numbers[0] == 1 || numbers[0] == 2 || numbers[0] == 3)
                {
                    RunOnUiThread(() => {
                        Toast.MakeText(this, "🎯 تم العثور على الرقم: " + numbers[0], ToastLength.Short).Show();
                        Intent colorIntent = new Intent("CHANGE_FLOATING_BUTTON_COLOR");
                        SendBroadcast(colorIntent);
                    });
                    
                    // إيقاف الخلط مؤقتاً
                    isRunning = false;
                    RunOnUiThread(() => {
                        startButton.Text = "▶ بدء الخلط";
                        textView.Text = "⏸ توقف مؤقت: تم العثور على " + numbers[0];
                    });
                    
                    // النقر على منتصف الشاشة
                    PerformTapOnCenter();
                    
                    await System.Threading.Tasks.Task.Delay(1000);
                    
                    // استئناف الخلط
                    isRunning = true;
                    RunOnUiThread(() => {
                        startButton.Text = "⏹ إيقاف الخلط";
                        textView.Text = "🔄 استئناف الخلط...";
                    });
                    cancellationToken = new System.Threading.CancellationTokenSource();
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

    private void PerformTapOnCenter()
    {
        try
        {
            // استخدام AccessibilityService للنقر
            try
            {
                Intent tapIntent = new Intent("PERFORM_TAP");
                // الحصول على حجم الشاشة
                var display = WindowManager.DefaultDisplay;
                var size = new Point();
                display.GetSize(size);
                int centerX = size.X / 2;
                int centerY = size.Y / 2;
                tapIntent.PutExtra("x", centerX);
                tapIntent.PutExtra("y", centerY);
                SendBroadcast(tapIntent);
                RunOnUiThread(() => {
                    Toast.MakeText(this, "👆 تم النقر في منتصف الشاشة", ToastLength.Short).Show();
                });
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("MainActivity", "Tap Error: " + ex.Message);
                RunOnUiThread(() => {
                    Toast.MakeText(this, "⚠️ لا يمكن النقر: " + ex.Message, ToastLength.Long).Show();
                });
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MainActivity", "Display Error: " + ex.Message);
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
