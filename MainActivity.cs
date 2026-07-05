using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Provider;
using System.Security.Cryptography;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView textView;
    private RandomNumberGenerator rng;
    private System.Threading.CancellationTokenSource cancellationToken;
    private BroadcastReceiver shuffleReceiver;
    private bool accessibilityChecked = false;
    private List<int> originalNumbers;
    private List<int> currentNumbers;
    private bool isShuffling = false;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        try
        {
            rng = RandomNumberGenerator.Create();
            cancellationToken = new System.Threading.CancellationTokenSource();
            
            originalNumbers = new List<int>();
            for (int i = 1; i <= 150; i++)
            {
                originalNumbers.Add(i);
            }
            currentNumbers = new List<int>(originalNumbers);
            
            CheckOverlayPermission();
            
            if (!accessibilityChecked)
            {
                accessibilityChecked = true;
                CheckAccessibilityPermission();
            }
            
            LinearLayout mainLayout = new LinearLayout(this);
            mainLayout.Orientation = Orientation.Vertical;
            mainLayout.SetPadding(50, 50, 50, 50);
            mainLayout.SetGravity(GravityFlags.Center);
            
            textView = new TextView(this);
            textView.Text = FormatNumbers(currentNumbers);
            textView.TextSize = 14;
            textView.SetTextColor(Color.Black);
            
            Button resetButton = new Button(this);
            resetButton.Text = "🔄 إظهار الأرقام الأصلية";
            resetButton.SetTextColor(Color.White);
            resetButton.SetBackgroundColor(Color.Gray);
            resetButton.Click += (s, e) => {
                currentNumbers = new List<int>(originalNumbers);
                textView.Text = FormatNumbers(currentNumbers);
                Toast.MakeText(this, "تم إعادة الأرقام الأصلية", ToastLength.Short).Show();
            };
            
            mainLayout.AddView(textView);
            mainLayout.AddView(resetButton);
            
            LinearLayout.LayoutParams params1 = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MatchParent,
                LinearLayout.LayoutParams.WrapContent);
            params1.SetMargins(0, 0, 0, 20);
            textView.LayoutParameters = params1;
            
            SetContentView(mainLayout);
            
            // بدء خدمة الزر العائم
            StartFloatingButtonService();
            
            shuffleReceiver = new ShuffleBroadcastReceiver();
            IntentFilter filter = new IntentFilter();
            filter.AddAction("START_SHUFFLING");
            filter.AddAction("STOP_SHUFFLING");
            filter.AddAction("PERFORM_TAP");
            filter.AddAction("FOUND_TARGET");
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                RegisterReceiver(shuffleReceiver, filter, ReceiverFlags.NotExported);
            }
            else
            {
                RegisterReceiver(shuffleReceiver, filter);
            }
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "خطأ في التهيئة: " + ex.Message, ToastLength.Long).Show();
            Android.Util.Log.Error("MainActivity", "OnCreate Error: " + ex.Message);
        }
    }

    // باقي الدوال (StartFloatingButtonService, FormatNumbers, CheckAccessibilityPermission, IsAccessibilityServiceEnabled, CheckOverlayPermission, StartFastShuffling, PerformTapOnCenter, OnDestroy) تبقى كما هي في الكود السابق
}
