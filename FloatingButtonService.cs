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
        }, 1000);
        
        IntentFilter filter = new IntentFilter("CHANGE_FLOATING_BUTTON_COLOR");
        RegisterReceiver(receiver, filter);
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
            
            Toast.MakeText(this, "تم إنشاء الزر العائم", ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "خطأ: " + ex.Message, ToastLength.Long).Show();
            Android.Util.Log.Error("FloatingService", ex.Message);
            Android.Util.Log.Error("FloatingService", ex.StackTrace);
        }
    }

    private BroadcastReceiver receiver = new FloatingButtonReceiver();

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
                    Android.Util.Log.Error("FloatingService", ex.Message);
                }
            }
        }
    }

    private void ChangeButtonColor()
    {
        if (floatingButton != null)
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
                Android.Util.Log.Error("FloatingService", ex.Message);
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (floatingView != null && windowManager != null)
        {
            try
            {
                windowManager.RemoveView(floatingView);
            }
            catch (Exception) { }
        }
        try
        {
            UnregisterReceiver(receiver);
        }
        catch (Exception) { }
    }
}
