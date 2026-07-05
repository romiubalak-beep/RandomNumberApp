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
    private IWindowManager windowManager;
    private View floatingView;
    private Button floatingButton;
    private bool isRed = false;
    private bool isCreated = false;
    private BroadcastReceiver receiver;
    private bool isShuffling = false;

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
        }, 2000);
        
        receiver = new FloatingButtonReceiver();
        IntentFilter filter = new IntentFilter();
        filter.AddAction("CHANGE_FLOATING_BUTTON_COLOR");
        filter.AddAction("FOUND_TARGET");
        filter.AddAction("SYNC_BUTTON_STATE");
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            RegisterReceiver(receiver, filter, ReceiverFlags.NotExported);
        }
        else
        {
            RegisterReceiver(receiver, filter);
        }
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
            floatingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 18);
            floatingButton.SetBackgroundColor(Color.Blue);
            floatingButton.SetTextColor(Color.White);
            
            // ✅ إضافة حدث الضغط مع التمييز بين الضغط والسحب
            floatingButton.Click += (s, e) => {
                if (!isDragging)
                {
                    isShuffling = !isShuffling;
                    Console.WriteLine("🔄 تم الضغط على الزر العائم - isShuffling: " + isShuffling);
                    
                    if (isShuffling)
                    {
                        floatingButton.Text = "⏹";
                        floatingButton.SetBackgroundColor(Color.Green);
                        Toast.MakeText(this, "▶ بدء الخلط", ToastLength.Short).Show();
                        Intent startIntent = new Intent("START_SHUFFLING");
                        SendBroadcast(startIntent);
                    }
                    else
                    {
                        floatingButton.Text = "▶";
                        floatingButton.SetBackgroundColor(Color.Blue);
                        Toast.MakeText(this, "⏹ إيقاف الخلط", ToastLength.Short).Show();
                        Intent stopIntent = new Intent("STOP_SHUFFLING");
                        SendBroadcast(stopIntent);
                    }
                }
                isDragging = false;
            };
            
            // ✅ إضافة حدث السحب (OnTouch)
            floatingButton.SetOnTouchListener(new FloatingButtonTouchListener(this));
            
            LinearLayout container = new LinearLayout(this);
            container.AddView(floatingButton);
            floatingView = container;
            
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            
            if (windowManager == null)
            {
                Toast.MakeText(this, "فشل في الحصول على WindowManager", ToastLength.Long).Show();
                return;
            }

            WindowManagerTypes windowType;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                windowType = WindowManagerTypes.ApplicationOverlay;
            }
            else
            {
                windowType = WindowManagerTypes.Phone;
            }
            
            var layoutParams = new WindowManagerLayoutParams(
                140, 140, windowType,
                WindowManagerFlags.NotFocusable | WindowManagerFlags.Fullscreen,
                Format.Translucent);
            
            layoutParams.Gravity = GravityFlags.Top | GravityFlags.Right;
            layoutParams.X = 0;
            layoutParams.Y = 100;
            
            windowManager.AddView(floatingView, layoutParams);
            isCreated = true;
            Toast.MakeText(this, "✅ الزر العائم جاهز (اسحب لتحريكه)", ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "❌ خطأ: " + ex.Message, ToastLength.Long).Show();
            Android.Util.Log.Error("FloatingService", "Create Error: " + ex.Message);
        }
    }

    // ✅ كلاس مخصص لمعالجة اللمس والسحب
    private class FloatingButtonTouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        private FloatingButtonService service;
        private WindowManagerLayoutParams layoutParams;

        public FloatingButtonTouchListener(FloatingButtonService service)
        {
            this.service = service;
        }

        public bool OnTouch(View? v, MotionEvent? e)
        {
            if (e == null || service.windowManager == null || service.floatingView == null)
                return false;

            if (layoutParams == null)
            {
                try
                {
                    layoutParams = (WindowManagerLayoutParams)service.floatingView.LayoutParameters;
                }
                catch { return false; }
            }

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

    private class FloatingButtonReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var service = (FloatingButtonService)context;
            
            if (intent.Action == "CHANGE_FLOATING_BUTTON_COLOR")
            {
                service.ChangeButtonColor();
            }
            else if (intent.Action == "FOUND_TARGET")
            {
                service.OnTargetFound();
            }
            else if (intent.Action == "SYNC_BUTTON_STATE")
            {
                bool isRunning = intent.GetBooleanExtra("isRunning", false);
                service.SyncButtonState(isRunning);
            }
        }
    }

    private void SyncButtonState(bool isRunning)
    {
        if (floatingButton != null && isCreated)
        {
            if (isRunning)
            {
                floatingButton.Text = "⏹";
                floatingButton.SetBackgroundColor(Color.Green);
                isShuffling = true;
            }
            else
            {
                floatingButton.Text = "▶";
                floatingButton.SetBackgroundColor(Color.Blue);
                isShuffling = false;
            }
        }
    }

    private void OnTargetFound()
    {
        if (floatingButton != null && isCreated)
        {
            floatingButton.Text = "▶";
            floatingButton.SetBackgroundColor(Color.Blue);
            isShuffling = false;
            Toast.MakeText(this, "✅ تم العثور على الرقم - الخلط متوقف", ToastLength.Short).Show();
            
            Intent stopIntent = new Intent("STOP_SHUFFLING");
            SendBroadcast(stopIntent);
        }
    }

    private void ChangeButtonColor()
    {
        if (floatingButton != null && isCreated)
        {
            try
            {
                if (isRed)
                {
                    floatingButton.SetBackgroundColor(Color.Blue);
                    isRed = false;
                }
                else
                {
                    floatingButton.SetBackgroundColor(Color.Red);
                    isRed = true;
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("FloatingService", "ChangeColor Error: " + ex.Message);
            }
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
        try
        {
            UnregisterReceiver(receiver);
        }
        catch (Exception) { }
    }
}
