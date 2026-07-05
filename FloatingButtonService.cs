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
    private Android.Views.WindowManager windowManager; // ✅ تغيير إلى WindowManager
    private View floatingView;
    private Button floatingButton;
    private bool isCreated = false;
    
    private int initialX, initialY;
    private float initialTouchX, initialTouchY;
    private bool isDragging = false;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        
        Handler handler = new Handler(Looper.MainLooper);
        handler.PostDelayed(() => {
            CreateFloatingButton();
        }, 1500);
    }

    private void CreateFloatingButton()
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (!Settings.CanDrawOverlays(this))
                {
                    Intent intent = new Intent(Settings.ActionManageOverlayPermission,
                        Android.Net.Uri.Parse("package:" + PackageName));
                    intent.AddFlags(ActivityFlags.NewTask);
                    StartActivity(intent);
                    return;
                }
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
                    ToggleShuffling();
                }
                isDragging = false;
            };
            
            LinearLayout container = new LinearLayout(this);
            container.AddView(floatingButton);
            floatingView = container;
            
            // ✅ استخدام WindowManager بدلاً من IWindowManager
            windowManager = (Android.Views.WindowManager)GetSystemService(WindowService);
            
            if (windowManager == null)
            {
                Toast.MakeText(this, "فشل في الحصول على WindowManager", ToastLength.Long).Show();
                return;
            }

            // ✅ تحديد نوع النافذة حسب إصدار Android
            int windowType;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                windowType = (int)WindowManagerTypes.ApplicationOverlay;
            }
            else
            {
                windowType = (int)WindowManagerTypes.Phone;
            }
            
            var layoutParams = new WindowManagerLayoutParams(
                180, 180,
                windowType,
                (int)WindowManagerFlags.NotFocusable | (int)WindowManagerFlags.Fullscreen,
                Format.Translucent);
            
            layoutParams.Gravity = (int)GravityFlags.Top | (int)GravityFlags.Right;
            layoutParams.X = 50;
            layoutParams.Y = 200;
            
            // ✅ إضافة الزر باستخدام WindowManager.AddView
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

    private void ToggleShuffling()
    {
        try
        {
            if (floatingButton.Text == "▶")
            {
                floatingButton.Text = "⏹";
                floatingButton.SetBackgroundColor(Color.ParseColor("#4CAF50"));
                Intent startIntent = new Intent("START_SHUFFLING");
                SendBroadcast(startIntent);
                Toast.MakeText(this, "▶ بدء الخلط", ToastLength.Short).Show();
            }
            else
            {
                floatingButton.Text = "▶";
                floatingButton.SetBackgroundColor(Color.ParseColor("#2196F3"));
                Intent stopIntent = new Intent("STOP_SHUFFLING");
                SendBroadcast(stopIntent);
                Toast.MakeText(this, "⏹ إيقاف الخلط", ToastLength.Short).Show();
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("FloatingService", "ToggleShuffling Error: " + ex.Message);
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
