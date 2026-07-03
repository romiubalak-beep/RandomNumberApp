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
using Android.Runtime;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView textView;
    private Button startButton;
    private bool isRunning = false;
    private WindowManagerLayoutParams floatingParams;
    private View floatingView;
    private WindowManager windowManager;
    private Button floatingButton;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // طلب صلاحية العرض فوق التطبيقات
        RequestOverlayPermission();
        
        // إنشاء واجهة
        LinearLayout layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetPadding(50, 50, 50, 50);
        layout.SetGravity(GravityFlags.Center);
        
        textView = new TextView(this);
        textView.Text = "اضغط على الزر لبدء الخلط السريع";
        textView.TextSize = 20;
        textView.SetTextColor(Color.Black);
        
        startButton = new Button(this);
        startButton.Text = "بدء الخلط السريع";
        startButton.SetTextColor(Color.White);
        startButton.SetBackgroundColor(Color.Blue);
        startButton.Click += StartShuffling;
        
        layout.AddView(textView);
        layout.AddView(startButton);
        
        SetContentView(layout);
        
        // إنشاء الزر العائم
        CreateFloatingButton();
    }

    private void RequestOverlayPermission()
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

    private void CreateFloatingButton()
    {
        // إنشاء الزر العائم
        floatingButton = new Button(this);
        floatingButton.Text = "⚡";
        floatingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 30);
        floatingButton.SetBackgroundColor(Color.Blue);
        floatingButton.SetTextColor(Color.White);
        floatingButton.Click += FloatingButton_Click;
        
        // إنشاء Layout للزر
        floatingView = new LinearLayout(this);
        var layoutParams = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.WrapContent,
            LinearLayout.LayoutParams.WrapContent);
        floatingView.LayoutParameters = layoutParams;
        ((LinearLayout)floatingView).AddView(floatingButton);
        
        // إعدادات النافذة العائمة
        windowManager = GetSystemService(Context.WindowService).JavaCast<WindowManager>();
        
        floatingParams = new WindowManagerLayoutParams(
            WindowManagerLayoutParams.WrapContent,
            WindowManagerLayoutParams.WrapContent,
            Build.VERSION.SdkInt >= BuildVersionCodes.O
                ? WindowManagerTypes.ApplicationOverlay
                : WindowManagerTypes.Phone,
            WindowManagerFlags.NotFocusable | WindowManagerFlags.Fullscreen,
            Android.Graphics.Format.Translucent
        );
        
        floatingParams.Gravity = GravityFlags.Top | GravityFlags.Right;
        floatingParams.X = 0;
        floatingParams.Y = 200;
        
        // إضافة الزر إلى النافذة
        try
        {
            windowManager.AddView(floatingView, floatingParams);
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "خطأ في إضافة الزر العائم: " + ex.Message, ToastLength.Long).Show();
        }
    }

    private void FloatingButton_Click(object sender, EventArgs e)
    {
        Toast.MakeText(this, "الزر العائم يعمل!", ToastLength.Short).Show();
        // هنا سنضيف لاحقاً وظيفة النقر على التطبيق الآخر
    }

    private void StartShuffling(object sender, EventArgs e)
    {
        if (!isRunning)
        {
            isRunning = true;
            startButton.Text = "إيقاف الخلط";
            StartFastShuffling();
        }
        else
        {
            isRunning = false;
            startButton.Text = "بدء الخلط السريع";
            textView.Text = "تم إيقاف الخلط";
        }
    }

    private async void StartFastShuffling()
    {
        List<int> numbers = new List<int>();
        for (int i = 1; i <= 150; i++) numbers.Add(i);
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        
        while (isRunning)
        {
            // خلط سريع باستخدام Fisher-Yates
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
            
            // عرض أول 10 أرقام
            string result = "الخلط السريع:\n";
            for (int i = 0; i < Math.Min(10, numbers.Count); i++)
            {
                result += numbers[i] + " ";
            }
            textView.Text = result;
            
            // التحقق من الأرقام 1، 2، 3
            if (numbers[0] == 1 || numbers[0] == 2 || numbers[0] == 3)
            {
                Toast.MakeText(this, "تم العثور على الرقم: " + numbers[0], ToastLength.Short).Show();
                // تحديث لون الزر العائم
                floatingButton.SetBackgroundColor(Color.Red);
                await System.Threading.Tasks.Task.Delay(500);
                floatingButton.SetBackgroundColor(Color.Blue);
                await System.Threading.Tasks.Task.Delay(1000);
            }
            
            await System.Threading.Tasks.Task.Delay(50);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // إزالة الزر العائم عند إغلاق التطبيق
        if (floatingView != null && windowManager != null)
        {
            try
            {
                windowManager.RemoveView(floatingView);
            }
            catch (Exception) { }
        }
    }
}
