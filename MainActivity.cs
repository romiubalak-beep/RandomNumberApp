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
        textView.TextSize = 24;
        textView.SetTextColor(Android.Graphics.Color.Black);
        
        button = new Button(this);
        button.Text = "توليد وخلط الأرقام";
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
            // 1. توليد 150 رقماً عشوائياً باستخدام RandomNumberGenerator.GetInt32()
            int count = 150;
            int[] numbers = new int[count];
            
            for (int i = 0; i < count; i++)
            {
                numbers[i] = RandomNumberGenerator.GetInt32(1, 1001); // أرقام من 1 إلى 1000
            }
            
            // 2. خلط الأرقام باستخدام خوارزمية Fisher-Yates
            ShuffleFisherYates(numbers);
            
            // 3. عرض النتيجة
            string result = "الأرقام بعد الخلط:\n";
            for (int i = 0; i < Math.Min(count, 50); i++) // عرض أول 50 رقم فقط
            {
                result += numbers[i] + " ";
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

    // خوارزمية Fisher-Yates لخلط الأرقام
    private void ShuffleFisherYates(int[] array)
    {
        int n = array.Length;
        for (int i = n - 1; i > 0; i--)
        {
            // توليد رقم عشوائي من 0 إلى i باستخدام RandomNumberGenerator
            int j = RandomNumberGenerator.GetInt32(0, i + 1);
            
            // تبديل العناصر
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
