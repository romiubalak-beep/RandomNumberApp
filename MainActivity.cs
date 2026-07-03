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
    private View floatingView;
    private Button floatingButton;
    private Android.Views.WindowManager windowManager;
    private WindowManagerLayoutParams layoutParams;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // طلب صلاحية العرض فوق التطبيقات
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
        
        // إنشاء واجهة التطبيق الرئيسية
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
        
        // إنشاء الزر العائم بعد التحميل
        CreateFloatingButton();
    }

    private void CreateFloatingButton()
    {
        try
        {
            // إنشاء الزر العائم
            floatingButton = new Button(this);
            floatingButton.Text = "⚡";
            floatingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 30);
            floatingButton.SetBackgroundColor(Color.Blue);
            floatingButton.SetTextColor(Color.White);
            
            // إنشاء Layout بسيط للزر
            LinearLayout container = new LinearLayout(this);
            container.AddView(floatingButton);
            floatingView = container;
            
            // إعدادات النافذة العائمة
            int type;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                type = (int)WindowManagerTypes.ApplicationOverlay;
            }
            else
            {
                type = (int)WindowManagerTypes.Phone;
            }
            
            layoutParams = new WindowManagerLayoutParams(
                WindowManagerLayoutParams.WrapContent,
                WindowManagerLayoutParams.WrapContent,
                type,
                (int)WindowManagerFlags.NotFocusable,
                Format.Translucent);
            
            layoutParams.Gravity = (int)GravityFlags.Top | (int)GravityFlags.Right;
            layoutParams.X = 0;
            layoutParams.Y = 200;
            
            // الحصول على WindowManager
            windowManager = (Android.Views.WindowManager)GetSystemService(WindowService);
            
            // إضافة الزر
            windowManager.AddView(floatingView, layoutParams);
            
            Toast.MakeText(this, "تم إنشاء الزر العائم", ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "خطأ في الزر العائم: " + ex.Message, ToastLength.Long).Show();
        }
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
        
        while (isRunning)
        {
            // خلط باستخدام Fisher-Yates مع RandomNumberGenerator
            int n = numbers.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(0, i + 1);
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
                // تغيير لون الزر العائم
                if (floatingButton != null)
                {
                    floatingButton.SetBackgroundColor(Color.Red);
                    await System.Threading.Tasks.Task.Delay(500);
                    floatingButton.SetBackgroundColor(Color.Blue);
                }
            }
            
            await System.Threading.Tasks.Task.Delay(50);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // إزالة الزر العائم
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
