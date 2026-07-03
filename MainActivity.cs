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
        
        // إنشاء واجهة بسيطة
        LinearLayout layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetPadding(50, 50, 50, 50);
        
        TextView textView = new TextView(this);
        textView.Text = "اضغط على الزر";
        textView.TextSize = 30;
        
        Button button = new Button(this);
        button.Text = "توليد رقم عشوائي";
        
        // إنشاء كائن RandomNumberGenerator مرة واحدة
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        
        button.Click += (object sender, System.EventArgs e) =>
        {
            // استخدام RandomNumberGenerator
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int num = Math.Abs(BitConverter.ToInt32(bytes, 0) % 100) + 1;
            textView.Text = "الرقم: " + num;
        };
        
        layout.AddView(textView);
        layout.AddView(button);
        
        SetContentView(layout);
    }
}
