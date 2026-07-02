using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System.Security.Cryptography;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // إنشاء TextView
        var textView = new TextView(this);
        textView.Text = "اضغط على الزر";
        textView.TextSize = 30;
        textView.SetGravity(GravityFlags.Center);
        
        // إنشاء Button
        var button = new Button(this);
        button.Text = "توليد رقم عشوائي";
        
        // حدث الضغط
        button.Click += (s, e) =>
        {
            int num = RandomNumberGenerator.GetInt32(1, 101);
            textView.Text = "الرقم: " + num;
        };
        
        // إنشاء Layout
        var layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetGravity(GravityFlags.Center);
        layout.SetPadding(50, 50, 50, 50);
        layout.AddView(textView);
        layout.AddView(button);
        
        SetContentView(layout);
    }
}
