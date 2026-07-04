using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Content;
using Android.Runtime;
using Android.Provider;

[Service]
public class FloatingButtonService : Service
{
    private IWindowManager windowManager;
    private View floatingView;
    private Button floatingButton;
    private bool isRed = false;
    private bool isCreated = false;
    private BroadcastReceiver receiver;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        
        // تأخير إنشاء الزر العائم
        Handler handler = new Handler(Looper.MainLooper);
        handler.PostDelayed(() => {
            CreateFloatingButton();
        }, 2000);
        
        // إنشاء الـ BroadcastReceiver مع تحديد EXPORTED
        receiver = new FloatingButtonReceiver();
        
        IntentFilter filter = new IntentFilter("CHANGE_FLOATING_BUTTON_COLOR");
        
        // ✅ الحل: استخدام RegisterReceiver مع RECEIVER_NOT_EXPORTED
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+
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
            // التحقق من صلاحية العرض فوق التطبيقات
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

            // إنشاء الزر
            floatingButton = new Button(this);
            floatingButton.Text = "⚡";
            floatingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 30);
            floatingButton.SetBackgroundColor(Color.Blue);
            floatingButton.SetTextColor(Color.White);
            
            // إضافة حدث للزر
            floatingButton.Click += (s, e) => {
                Toast.MakeText(this, "الزر العائم يعمل!", ToastLength.Short).Show();
            };
            
            // إنشاء Layout للزر
            LinearLayout container = new LinearLayout(this);
            container.AddView(floatingButton);
            floatingView = container;
            
            // الحصول على WindowManager
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            
            if (windowManager == null)
            {
                Toast.MakeText(this, "فشل في الحصول على WindowManager", ToastLength.Long).Show();
                return;
            }

            // تحديد نوع النافذة حسب إصدار Android
            WindowManagerTypes windowType;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                windowType = WindowManagerTypes.ApplicationOverlay;
            }
            else
            {
                windowType = WindowManagerTypes.Phone;
            }
            
            // إعدادات النافذة العائمة
            var layoutParams = new WindowManagerLayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent,
                windowType,
                WindowManagerFlags.NotFocusable | WindowManagerFlags.Fullscreen,
                Format.Translucent);
            
            layoutParams.Gravity = GravityFlags.Top | GravityFlags.Right;
            layoutParams.X = 0;
            layoutParams.Y = 100;
            
            // إضافة الزر
            windowManager.AddView(floatingView, layoutParams);
            isCreated = true;
            Toast.MakeText(this, "✅ تم إنشاء الزر العائم", ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "❌ خطأ: " + ex.Message, ToastLength.Long).Show();
            Android.Util.Log.Error("FloatingService", "Create Error: " + ex.Message);
            Android.Util.Log.Error("FloatingService", ex.StackTrace);
        }
    }

    private class FloatingButtonReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == "CHANGE_FLOATING_BUTTON_COLOR")
            {
                try
                {
                    var service = (FloatingButtonService)context;
                    service.ChangeButtonColor();
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error("FloatingService", "Receiver Error: " + ex.Message);
                }
            }
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
