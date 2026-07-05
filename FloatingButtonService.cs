using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Content;
using Android.Runtime;
using Android.Provider;
using System;

[Service]
public class FloatingButtonService : Service, View.IOnTouchListener
{
    private IWindowManager windowManager;
    private View floatingView;
    private Button floatingButton;
    private bool isCreated = false;
    
    private int initialX, initialY;
    private float initialTouchX, initialTouchY;
    private bool isDragging = false;
    private bool isServiceStarted = false;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        Handler handler = new Handler(Looper.MainLooper);
        handler.PostDelayed(CreateFloatingButton, 1500);
    }

    private void CreateFloatingButton()
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && !Settings.CanDrawOverlays(this))
            {
                Intent intent = new Intent(Settings.ActionManageOverlayPermission,
                    Android.Net.Uri.Parse("package:" + PackageName));
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
                return;
            }

            floatingButton = new Button(this);
            floatingButton.Text = "▶";
            floatingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
            floatingButton.SetBackgroundColor(Color.ParseColor("#2196F3"));
            floatingButton.SetTextColor(Color.White);
            floatingButton.SetPadding(20, 20, 20, 20);
            floatingButton.SetOnTouchListener(this);
            
            floatingButton.Click += (s, e) => {
                if (!isDragging)
                {
                    ToggleService();
                }
                isDragging = false;
            };
            
            LinearLayout container = new LinearLayout(this);
            container.AddView(floatingButton);
            floatingView = container;
            
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            if (windowManager == null)
            {
                Toast.MakeText(this, "فشل في الحصول على WindowManager", ToastLength.Long).Show();
                return;
            }

            var layoutParams = new WindowManagerLayoutParams(
                180, 180,
                WindowManagerTypes.ApplicationOverlay,
                WindowManagerFlags.NotFocusable | WindowManagerFlags.Fullscreen,
                Format.Translucent);
            
            layoutParams.Gravity = GravityFlags.Top | GravityFlags.Right;
            layoutParams.X = 50;
            layoutParams.Y = 200;
            
            windowManager.AddView(floatingView, layoutParams);
            isCreated = true;
            Toast.MakeText(this, "✅ الزر العائم يعمل فوق التطبيقات", ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "❌ خطأ: " + ex.Message, ToastLength.Long).Show();
            Android.Util.Log.Error("FloatingService", "Create Error: " + ex.Message);
        }
    }

    private void ToggleService()
    {
        isServiceStarted = !isServiceStarted;
        
        if (isServiceStarted)
        {
            floatingButton.Text = "⏹";
            floatingButton.SetBackgroundColor(Color.ParseColor("#4CAF50"));
            SendBroadcast(new Intent("START_SHUFFLING"));
            Toast.MakeText(this, "▶ بدء الخلط", ToastLength.Short).Show();
            
            Intent tapServiceIntent = new Intent(this, typeof(TapAccessibilityService));
            StartService(tapServiceIntent);
        }
        else
        {
            floatingButton.Text = "▶";
            floatingButton.SetBackgroundColor(Color.ParseColor("#2196F3"));
            SendBroadcast(new Intent("STOP_SHUFFLING"));
            Toast.MakeText(this, "⏹ إيقاف الخلط", ToastLength.Short).Show();
            
            Intent tapServiceIntent = new Intent(this, typeof(TapAccessibilityService));
            StopService(tapServiceIntent);
        }
    }

    public bool OnTouch(View v, MotionEvent e)
    {
        if (windowManager == null || floatingView == null)
            return false;

        var layoutParams = (WindowManagerLayoutParams)floatingView.LayoutParameters;
        switch (e.Action)
        {
            case MotionEventActions.Down:
                initialX = layoutParams.X;
                initialY = layoutParams.Y;
                initialTouchX = e.RawX;
                initialTouchY = e.RawY;
                isDragging = false;
                return true;

            case MotionEventActions.Move:
                float deltaX = e.RawX - initialTouchX;
                float deltaY = e.RawY - initialTouchY;
                if (Math.Abs(deltaX) > 10 || Math.Abs(deltaY) > 10)
                {
                    isDragging = true;
                }
                if (isDragging)
                {
                    layoutParams.X = initialX + (int)deltaX;
                    layoutParams.Y = initialY + (int)deltaY;
                    windowManager.UpdateViewLayout(floatingView, layoutParams);
                }
                return true;

            case MotionEventActions.Up:
                return true;
        }
        return false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (floatingView != null && windowManager != null && isCreated)
        {
            try
            {
                windowManager.RemoveView(floatingView);
                isCreated = false;
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("FloatingService", "RemoveView Error: " + ex.Message);
            }
        }
    }
}
