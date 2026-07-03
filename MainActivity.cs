using Android.App;
using Android.Widget;
using Android.OS;
using System.Security.Cryptography;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView textView;
    private Button button;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // إنشاء واجهة بسيطة
        LinearLayout layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetPadding(50, 50, 50, 50);
        
        textView = new TextView(this);
        textView.Text = "اضغط على الزر";
        textView.TextSize = 30;
        
        button = new Button(this);
        button.Text = "توليد رقم عشوائي";
        
        button.Click += Button_Click;
        
        layout.AddView(textView);
        layout.AddView(button);
        
        SetContentView(layout);
    }

    private void Button_Click(object sender, System.EventArgs e)
    {
        try
        {
            // استخدام RandomNumberGenerator
            int randomNumber = GetRandomNumber(1, 101);
            textView.Text = "الرقم: " + randomNumber;
        }
        catch (System.Exception ex)
        {
            textView.Text = "خطأ: " + ex.Message;
        }
    }

    private int GetRandomNumber(int min, int max)
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] data = new byte[4];
            rng.GetBytes(data);
            int value = System.BitConverter.ToInt32(data, 0);
            return System.Math.Abs(value % (max - min)) + min;
        }
    }
}
