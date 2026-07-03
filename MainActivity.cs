using Android.App;
using Android.Widget;
using Android.OS;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView textView;
    private Button button;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // إنشاء واجهة
        LinearLayout layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetPadding(50, 50, 50, 50);
        
        textView = new TextView(this);
        textView.Text = "اضغط على الزر لتوليد 150 رقماً";
        textView.TextSize = 20;
        textView.SetTextColor(Android.Graphics.Color.Black);
        
        button = new Button(this);
        button.Text = "توليد وخلط الأرقام (1-150)";
        button.SetTextColor(Android.Graphics.Color.White);
        button.SetBackgroundColor(Android.Graphics.Color.Blue);
        
        button.Click += GenerateAndShuffleNumbers;
        
        layout.AddView(textView);
        layout.AddView(button);
        
        SetContentView(layout);
    }

    private void GenerateAndShuffleNumbers(object sender, EventArgs e)
    {
        try
        {
            // 1. إنشاء قائمة أرقام من 1 إلى 150
            int count = 150;
            List<int> numbers = new List<int>();
            for (int i = 1; i <= count; i++)
            {
                numbers.Add(i);
            }
            
            // 2. خلط الأرقام باستخدام خوارزمية Fisher-Yates مع RandomNumberGenerator
            ShuffleFisherYates(numbers);
            
            // 3. عرض النتيجة
            string result = "الأرقام المخلوطة (1-150):\n";
            for (int i = 0; i < numbers.Count; i++)
            {
                result += numbers[i].ToString().PadLeft(3) + " ";
                if ((i + 1) % 10 == 0) result += "\n";
            }
            
            textView.Text = result;
            Toast.MakeText(this, "تم توليد وخلط " + count + " رقم!", ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "خطأ: " + ex.Message, ToastLength.Long).Show();
        }
    }

    // خوارزمية Fisher-Yates لخلط الأرقام باستخدام RandomNumberGenerator
    private void ShuffleFisherYates(List<int> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            // توليد رقم عشوائي من 0 إلى i باستخدام RandomNumberGenerator
            int j = RandomNumberGenerator.GetInt32(0, i + 1);
            
            // تبديل العناصر
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
