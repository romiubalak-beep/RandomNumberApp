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

    private void StartShuffling(object sender, EventArgs e)
    {
        if (!isRunning)
        {
            isRunning = true;
            startButton.Text = "جاري الخلط...";
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
                await System.Threading.Tasks.Task.Delay(1000);
            }
            
            // تأخير بسيط للسرعة
            await System.Threading.Tasks.Task.Delay(50);
        }
    }
}
