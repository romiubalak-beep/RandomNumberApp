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
    
    // ✅ متغيرات السحب
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
            // ✅ التحقق من صلاحية العرض فوق التطبيقات
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

            // ✅ إنشاء الزر العائم
            floatingButton = new Button(this);
            floatingButton.Text = "▶";
            floatingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
            floatingButton.SetBackgroundColor(Color.ParseColor("#2196F3"));
            floatingButton.SetTextColor(Color.White);
            
            // ✅ جعل الزر دائرياً
            floatingButton.SetPadding(20, 20, 20, 20);
            
            // ✅ تعيين مستمع اللمس (للسحب)
            floatingButton.SetOnTouchListener(this);
            
            // ✅ حدث الضغط
            floatingButton.Click += (s, e) => {
                if (!isDragging)
                {
                    // ✅ تبديل حالة الخلط
                    ToggleShuffling();
                }
                isDragging = false;
            };
            
            // ✅ وضع الزر في Layout
            LinearLayout container = new LinearLayout(this);
            container.AddView(floatingButton);
            floatingView = container;
            
            // ✅ الحصول على WindowManager
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            
            if (windowManager == null)
            {
                Toast.MakeText(this, "فشل في الحصول على WindowManager", ToastLength.Long).Show();
                return;
            }

            // ✅ تحديد نوع النافذة حسب إصدار Android
            WindowManagerTypes windowType;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                windowType = WindowManagerTypes.ApplicationOverlay;
            }
            else
            {
                windowType = WindowManagerTypes.Phone;
            }
            
            // ✅ إعدادات النافذة العائمة
            var layoutParams = new WindowManagerLayoutParams(
                180, 180, // حجم الزر
                windowType,
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

    // ✅ دالة تبديل حالة الخلط
    private void ToggleShuffling()
    {
        try
        {
            // ✅ التحقق من حالة الزر
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

    // ✅ تنفيذ واجهة IOnTouchListener (للسحب)
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
