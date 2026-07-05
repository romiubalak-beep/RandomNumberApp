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
public class FloatingButtonService : Service
{
    private IWindowManager windowManager; // ✅ العودة إلى IWindowManager
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
            floatingButton.SetOnTouchListener(new FloatingButtonTouchListener(this));
            
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
            
            // ✅ استخدام IWindowManager بدلاً من WindowManager
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            
            if (windowManager == null)
            {
                Toast.MakeText(this, "فشل في الحصول على WindowManager", ToastLength.Long).Show();
                return;
            }

            // ✅ استخدام WindowManagerTypes و WindowManagerFlags مباشرة
            var layoutParams = new WindowManagerLayoutParams(
                180, 180,
                WindowManagerTypes.ApplicationOverlay,
                WindowManagerFlags.NotFocusable | WindowManagerFlags.Fullscreen,
                Format.Translucent);
            
            layoutParams.Gravity = GravityFlags.Top | GravityFlags.Right;
            layoutParams.X = 50;
            layoutParams.Y = 200;
            
            // ✅ إضافة الزر
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

    // ✅ كلاس مخصص للتعامل مع اللمس
    private class FloatingButtonTouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        private FloatingButtonService service;

        public FloatingButtonTouchListener(FloatingButtonService service)
        {
            this.service = service;
        }

        public bool OnTouch(View? v, MotionEvent? e)
        {
            if (service.windowManager == null || service.floatingView == null || e == null)
                return false;

            var layoutParams = (WindowManagerLayoutParams)service.floatingView.LayoutParameters;
            if (layoutParams == null) return false;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    service.initialX = layoutParams.X;
                    service.initialY = layoutParams.Y;
                    service.initialTouchX = e.RawX;
                    service.initialTouchY = e.RawY;
                    service.isDragging = false;
                    return true;

                case MotionEventActions.Move:
                    float deltaX = e.RawX - service.initialTouchX;
                    float deltaY = e.RawY - service.initialTouchY;
                    
                    if (Math.Abs(deltaX) > 10 || Math.Abs(deltaY) > 10)
                    {
                        service.isDragging = true;
                    }
                    
                    if (service.isDragging)
                    {
                        layoutParams.X = service.initialX + (int)deltaX;
                        layoutParams.Y = service.initialY + (int)deltaY;
                        service.windowManager.UpdateViewLayout(service.floatingView, layoutParams);
                    }
                    return true;

                case MotionEventActions.Up:
                    return true;
            }
            return false;
        }
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
