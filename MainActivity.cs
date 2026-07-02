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
        
        var tv = new TextView(this);
        tv.Text = "اضغط على الزر";
        tv.TextSize = 30;
        
        var btn = new Button(this);
        btn.Text = "توليد رقم عشوائي";
        
        btn.Click += (s, e) => {
            int num = RandomNumberGenerator.GetInt32(1, 101);
            tv.Text = "الرقم: " + num;
        };
        
        var layout = new LinearLayout(this);
        layout.AddView(tv);
        layout.AddView(btn);
        
        SetContentView(layout);
    }
}
