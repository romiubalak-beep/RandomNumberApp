using Android.App;
using Android.Widget;
using Android.OS;
using System;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView textView;
    private Random random;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // إنشاء Random
        random = new Random();
        
        // إنشاء واجهة
        LinearLayout layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetPadding(50, 50, 50, 50);
        
        textView = new TextView(this);
        textView.Text = "اضغط على الزر";
        textView.TextSize = 30;
        textView.SetTextColor(Android.Graphics.Color.Black);
        
        Button button = new Button(this);
        button.Text = "توليد رقم عشوائي";
        button.SetTextColor(Android.Graphics.Color.White);
        button.SetBackgroundColor(Android.Graphics.Color.Blue);
        
        button.Click += GenerateRandomNumber;
        
        layout.AddView(textView);
        layout.AddView(button);
        
        SetContentView(layout);
    }

    private void GenerateRandomNumber(object sender, EventArgs e)
    {
        try
        {
            int num = random.Next(1, 101);
            string result = "الرقم: " + num;
            textView.Text = result;
            Toast.MakeText(this, result, ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "خطأ: " + ex.Message, ToastLength.Long).Show();
        }
    }
}
