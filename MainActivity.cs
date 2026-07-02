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
        SetContentView(Resource.Layout.activity_main);

        textView = FindViewById<TextView>(Resource.Id.textView);
        button = FindViewById<Button>(Resource.Id.button);

        button.Click += (s, e) =>
        {
            int num = RandomNumberGenerator.GetInt32(1, 101);
            textView.Text = "الرقم: " + num;
        };
    }
}
