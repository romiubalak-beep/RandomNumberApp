using Android.App;
using Android.Widget;
using Android.OS;
using System.Security.Cryptography;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // إنشاء واجهة بسيطة جداً
        LinearLayout layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetPadding(50, 50, 50, 50);
        
        TextView textView = new TextView(this);
        textView.Text = "اضغط على الزر";
        textView.TextSize = 30;
        textView.SetTextColor(Android.Graphics.Color.Black);
        
        Button button = new Button(this);
        button.Text = "توليد رقم عشوائي";
        button.SetTextColor(Android.Graphics.Color.White);
        
        button.Click += delegate
        {
            int num = RandomNumberGenerator.GetInt32(1, 101);
            textView.Text = "الرقم: " + num;
        };
        
        layout.AddView(textView);
        layout.AddView(button);
        
        SetContentView(layout);
    }
}
