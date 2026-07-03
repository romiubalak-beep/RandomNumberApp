using Android.App;
using Android.Widget;
using Android.OS;
using System;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        LinearLayout layout = new LinearLayout(this);
        layout.Orientation = Orientation.Vertical;
        layout.SetPadding(50, 50, 50, 50);
        
        TextView textView = new TextView(this);
        textView.Text = "اضغط على الزر";
        textView.TextSize = 30;
        
        Button button = new Button(this);
        button.Text = "توليد رقم عشوائي";
        
        Random random = new Random();
        
        button.Click += (s, e) =>
        {
            int num = random.Next(1, 101);
            textView.Text = "الرقم: " + num;
        };
        
        layout.AddView(textView);
        layout.AddView(button);
        
        SetContentView(layout);
    }
}
