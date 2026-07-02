using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System.Security.Cryptography;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView textView;
    private Button button;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // إنشاء الواجهة برمجياً بدلاً من Resource
        textView = new TextView(this);
        textView.Text = "اضغط على الزر";
        textView.TextSize = 30;
        textView.Gravity = GravityFlags.Center;

        button = new Button(this);
        button.Text = "توليد رقم عشوائي";

        button.Click += (s, e) =>
        {
            int num = RandomNumberGenerator.GetInt32(1, 101);
            textView.Text = "الرقم: " + num;
        };

        var layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.Gravity = GravityFlags.Center;
        layout.SetPadding(50, 50, 50, 50);
        layout.AddView(textView);
        layout.AddView(button);

        SetContentView(layout);
    }
}
