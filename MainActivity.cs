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
    private TextView numbersTextView;
    private TextView titleTextView;
    private Button resetButton;
    private RandomNumberGenerator rng;
    private System.Threading.CancellationTokenSource cancellationToken;
    private BroadcastReceiver shuffleReceiver;
    private bool accessibilityChecked = false;
    private List<int> originalNumbers;
    private List<int> currentNumbers;
    private bool isShuffling = false;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // ✅ استخدام ملف XML مع معالجة الأخطاء
        try
        {
            SetContentView(Resource.Layout.activity_main);
        }
        catch (Exception ex)
        {
            // ✅ إذا فشل تحميل XML، أنشئ الواجهة برمجياً
            Android.Util.Log.Error("MainActivity", "XML Error: " + ex.Message);
            CreateLayoutProgrammatically();
        }
        
        try
        {
            rng = RandomNumberGenerator.Create();
            cancellationToken = new System.Threading.CancellationTokenSource();
            
            originalNumbers = new List<int>();
            for (int i = 1; i <= 150; i++)
            {
                originalNumbers.Add(i);
            }
            currentNumbers = new List<int>(originalNumbers);
            
            // ✅ ربط العناصر من XML (إذا كانت موجودة)
            try
            {
                numbersTextView = FindViewById<TextView>(Resource.Id.numbersTextView);
                titleTextView = FindViewById<TextView>(Resource.Id.textView);
                resetButton = FindViewById<Button>(Resource.Id.resetButton);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("MainActivity", "FindView Error: " + ex.Message);
                CreateLayoutProgrammatically();
            }
            
            // ✅ إذا كانت العناصر null، أنشئها برمجياً
            if (numbersTextView == null || resetButton == null)
            {
                CreateLayoutProgrammatically();
            }
            
            // ✅ عرض الأرقام
            if (numbersTextView != null)
            {
                numbersTextView.Text = FormatNumbers(currentNumbers);
            }
            
            CheckOverlayPermission();
            
            if (!accessibilityChecked)
            {
                accessibilityChecked = true;
                CheckAccessibilityPermission();
            }
            
            if (resetButton != null)
            {
                resetButton.Click += (s, e) => {
                    currentNumbers = new List<int>(originalNumbers);
                    if (numbersTextView != null)
                        numbersTextView.Text = FormatNumbers(currentNumbers);
                    Toast.MakeText(this, "تم إعادة الأرقام الأصلية", ToastLength.Short).Show();
                };
            }
            
            StartFloatingButtonService();
            
            shuffleReceiver = new ShuffleBroadcastReceiver();
            IntentFilter filter = new IntentFilter();
            filter.AddAction("START_SHUFFLING");
            filter.AddAction("STOP_SHUFFLING");
            filter.AddAction("PERFORM_TAP");
            filter.AddAction("FOUND_TARGET");
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                RegisterReceiver(shuffleReceiver, filter, ReceiverFlags.NotExported);
            }
            else
            {
                RegisterReceiver(shuffleReceiver, filter);
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "خطأ في التهيئة: " + ex.Message, ToastLength.Long).Show();
            Android.Util.Log.Error("MainActivity", "OnCreate Error: " + ex.Message);
        }
    }

    // ✅ إنشاء الواجهة برمجياً (في حال فشل XML)
    private void CreateLayoutProgrammatically()
    {
        LinearLayout mainLayout = new LinearLayout(this);
        mainLayout.Orientation = Orientation.Vertical;
        mainLayout.SetPadding(50, 50, 50, 50);
        mainLayout.SetGravity(GravityFlags.Center);
        
        titleTextView = new TextView(this);
        titleTextView.Text = "📊 الأرقام (1-150):";
        titleTextView.TextSize = 18;
        titleTextView.SetTextColor(Color.Black);
        titleTextView.SetTextSize(Android.Util.ComplexUnitType.Sp, 18);
        
        numbersTextView = new TextView(this);
        numbersTextView.Text = "";
        numbersTextView.TextSize = 12;
        numbersTextView.SetTextColor(Color.Black);
        numbersTextView.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
        numbersTextView.SetGravity(GravityFlags.Center);
        
        resetButton = new Button(this);
        resetButton.Text = "🔄 إظهار الأرقام الأصلية";
        resetButton.SetTextColor(Color.White);
        resetButton.SetBackgroundColor(Color.Gray);
        
        LinearLayout.LayoutParams params1 = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.MatchParent,
            LinearLayout.LayoutParams.WrapContent);
        params1.SetMargins(0, 0, 0, 20);
        numbersTextView.LayoutParameters = params1;
        
        LinearLayout.LayoutParams params2 = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.MatchParent,
            LinearLayout.LayoutParams.WrapContent);
        params2.SetMargins(0, 0, 0, 20);
        titleTextView.LayoutParameters = params2;
        
        mainLayout.AddView(titleTextView);
        mainLayout.AddView(numbersTextView);
        mainLayout.AddView(resetButton);
        
        SetContentView(mainLayout);
    }

    private void StartFloatingButtonService()
    {
        try
        {
            Intent serviceIntent = new Intent(this, typeof(FloatingButtonService));
            StartService(serviceIntent);
            Toast.MakeText(this, "✅ الزر العائم يعمل فوق التطبيقات", ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MainActivity", "StartFloatingButtonService Error: " + ex.Message);
            Toast.MakeText(this, "خطأ في بدء الخدمة: " + ex.Message, ToastLength.Long).Show();
        }
    }

    private string FormatNumbers(List<int> numbers)
    {
        string result = "";
        int count = 0;
        foreach (int num in numbers)
        {
            result += num.ToString().PadLeft(3) + " ";
            count++;
            if (count % 15 == 0) result += "\n";
        }
        return result;
    }

    private void CheckAccessibilityPermission()
    {
        try
        {
            if (IsAccessibilityServiceEnabled())
            {
                return;
            }
            
            Intent intent = new Intent(Android.Provider.Settings.ActionAccessibilitySettings);
            StartActivity(intent);
            Toast.MakeText(this, "الرجاء تفعيل خدمة إمكانية الوصول للتطبيق", ToastLength.Long).Show();
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MainActivity", "Accessibility Error: " + ex.Message);
        }
    }

    private bool IsAccessibilityServiceEnabled()
    {
        try
        {
            string serviceName = "com.example.randomapp/com.example.randomapp.TapAccessibilityService";
            string enabledServices = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.EnabledAccessibilityServices);
            
            if (!string.IsNullOrEmpty(enabledServices))
            {
                return enabledServices.Contains(serviceName);
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private class ShuffleBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var activity = (MainActivity)context;
            if (intent.Action == "START_SHUFFLING")
            {
                if (!activity.isShuffling)
                {
                    activity.isShuffling = true;
                    activity.cancellationToken = new System.Threading.CancellationTokenSource();
                    activity.StartFastShuffling(activity.cancellationToken.Token);
                    Toast.MakeText(activity, "✅ بدء الخلط", ToastLength.Short).Show();
                }
            }
            else if (intent.Action == "STOP_SHUFFLING")
            {
                if (activity.isShuffling)
                {
                    activity.isShuffling = false;
                    activity.cancellationToken.Cancel();
                    activity.currentNumbers = new List<int>(activity.originalNumbers);
                    activity.RunOnUiThread(() => {
                        if (activity.numbersTextView != null)
                            activity.numbersTextView.Text = activity.FormatNumbers(activity.currentNumbers);
                    });
                    Toast.MakeText(activity, "⏹ إيقاف الخلط", ToastLength.Short).Show();
                }
            }
            else if (intent.Action == "PERFORM_TAP")
            {
                activity.PerformTapOnCenter();
            }
            else if (intent.Action == "FOUND_TARGET")
            {
                activity.OnTargetFound();
            }
        }
    }

    private void OnTargetFound()
    {
        try
        {
            isShuffling = false;
            cancellationToken.Cancel();
            currentNumbers = new List<int>(originalNumbers);
            RunOnUiThread(() => {
                if (numbersTextView != null)
                    numbersTextView.Text = FormatNumbers(currentNumbers);
                Toast.MakeText(this, "✅ تم العثور على الرقم المستهدف", ToastLength.Short).Show();
            });
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MainActivity", "OnTargetFound Error: " + ex.Message);
        }
    }

    private void CheckOverlayPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            if (!Android.Provider.Settings.CanDrawOverlays(this))
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission,
                    Android.Net.Uri.Parse("package:" + PackageName));
                StartActivity(intent);
                Toast.MakeText(this, "الرجاء تفعيل صلاحية العرض فوق التطبيقات", ToastLength.Long).Show();
            }
        }
    }

    private async void StartFastShuffling(System.Threading.CancellationToken token)
    {
        try
        {
            while (isShuffling && !token.IsCancellationRequested)
            {
                int n = currentNumbers.Count;
                for (int i = n - 1; i > 0; i--)
                {
                    byte[] bytes = new byte[4];
                    rng.GetBytes(bytes);
                    int j = Math.Abs(BitConverter.ToInt32(bytes, 0) % (i + 1));
                    int temp = currentNumbers[i];
                    currentNumbers[i] = currentNumbers[j];
                    currentNumbers[j] = temp;
                }
                
                RunOnUiThread(() => {
                    if (numbersTextView != null)
                        numbersTextView.Text = FormatNumbers(currentNumbers);
                });
                
                if (currentNumbers[0] == 1 || currentNumbers[0] == 2 || currentNumbers[0] == 3)
                {
                    RunOnUiThread(() => {
                        Toast.MakeText(this, "🎯 تم العثور على الرقم: " + currentNumbers[0], ToastLength.Short).Show();
                    });
                    
                    isShuffling = false;
                    currentNumbers = new List<int>(originalNumbers);
                    
                    RunOnUiThread(() => {
                        if (numbersTextView != null)
                            numbersTextView.Text = FormatNumbers(currentNumbers);
                        Toast.MakeText(this, "✅ تم إعادة الأرقام الأصلية", ToastLength.Short).Show();
                    });
                    
                    PerformTapOnCenter();
                    
                    await System.Threading.Tasks.Task.Delay(1000);
                }
                
                await System.Threading.Tasks.Task.Delay(100, token);
            }
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            currentNumbers = new List<int>(originalNumbers);
            RunOnUiThread(() => {
                if (numbersTextView != null)
                    numbersTextView.Text = FormatNumbers(currentNumbers);
                Toast.MakeText(this, "⏹ تم إيقاف الخلط", ToastLength.Short).Show();
            });
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
            isShuffling = false;
        }
    }

    private void PerformTapOnCenter()
    {
        try
        {
            var display = WindowManager.DefaultDisplay;
            var size = new Point();
            display.GetSize(size);
            int centerX = size.X / 2;
            int centerY = size.Y / 2;
            
            try
            {
                Intent tapIntent = new Intent("PERFORM_TAP");
                tapIntent.PutExtra("x", centerX);
                tapIntent.PutExtra("y", centerY);
                SendBroadcast(tapIntent);
                Toast.MakeText(this, "👆 تم النقر في منتصف الشاشة", ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("MainActivity", "Tap Error: " + ex.Message);
                Toast.MakeText(this, "⚠️ لا يمكن النقر: " + ex.Message, ToastLength.Long).Show();
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
