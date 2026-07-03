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

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // طلب صلاحية Accessibility
        RequestAccessibilityPermission();
        
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
    }

    private void RequestAccessibilityPermission()
    {
        Intent intent = new Intent(Settings.ActionAccessibilitySettings);
        StartActivity(intent);
        Toast.MakeText(this, "الرجاء تفعيل التطبيق في إعدادات إمكانية الوصول", ToastLength.Long).Show();
    }

    private void StartShuffling(object sender, EventArgs e)
    {
        if (!isRunning)
        {
            isRunning = true;
            startButton.Text = "جاري الخلط...";
            
            // بدء خدمة الزر العائم
            Intent serviceIntent = new Intent(this, typeof(FloatingButtonService));
            StartService(serviceIntent);
            
            // بدء الخلط السريع
            StartFastShuffling();
        }
    }

    private async void StartFastShuffling()
    {
        List<int> numbers = new List<int>();
        for (int i = 1; i <= 150; i++) numbers.Add(i);
        
        while (isRunning)
        {
            // خلط سريع باستخدام Fisher-Yates
            ShuffleFisherYates(numbers);
            
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
                // تنفيذ نقرة على تطبيق آخر
                PerformClickOnOtherApp(numbers[0]);
                
                // إيقاف الخلط مؤقتاً
                await System.Threading.Tasks.Task.Delay(1000);
            }
            
            // تأخير بسيط للسرعة
            await System.Threading.Tasks.Task.Delay(50);
        }
    }

    private void ShuffleFisherYates(List<int> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void PerformClickOnOtherApp(int number)
    {
        // إرسال إشارة إلى Accessibility Service
        Intent intent = new Intent("CLICK_ACTION");
        intent.PutExtra("number", number);
        SendBroadcast(intent);
        
        Toast.MakeText(this, "تم العثور على الرقم: " + number, ToastLength.Short).Show();
    }
}
