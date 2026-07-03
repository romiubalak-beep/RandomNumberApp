using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Content;
using Android.Runtime;

[Service]
public class FloatingButtonService : Service
{
    private WindowManager windowManager;
    private View floatingView;
    private Button floatingButton;
    private WindowManagerLayoutParams layoutParams;
    private bool isRed = false;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        
        // إنشاء الزر العائم
        CreateFloatingButton();
        
        // استقبال الإشارات لتغيير اللون
        IntentFilter filter = new IntentFilter("CHANGE_FLOATING_BUTTON_COLOR");
        RegisterReceiver(receiver, filter);
    }

    private void CreateFloatingButton()
    {
        try
        {
            // إنشاء الزر
            floatingButton = new Button(this);
            floatingButton.Text = "⚡";
            floatingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 30);
            floatingButton.SetBackgroundColor(Color.Blue);
            floatingButton.SetTextColor(Color.White);
            
            // إنشاء Layout للزر
            LinearLayout container = new LinearLayout(this);
            container.AddView(floatingButton);
            floatingView = container;
            
            // الحصول على WindowManager
            windowManager = (WindowManager)GetSystemService(WindowService);
            
            // إعدادات النافذة العائمة
            int type;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                type = (int)WindowManagerTypes.ApplicationOverlay;
            }
            else
            {
                type = (int)WindowManagerTypes.Phone;
            }
            
            layoutParams = new WindowManagerLayoutParams(
                WindowManagerLayoutParams.WrapContent,
                WindowManagerLayoutParams.WrapContent,
                type,
                (int)WindowManagerFlags.NotFocusable,
                Format.Translucent);
            
            layoutParams.Gravity = (int)GravityFlags.Top | (int)GravityFlags.Right;
            layoutParams.X = 0;
            layoutParams.Y = 200;
            
            // إضافة الزر
            windowManager.AddView(floatingView, layoutParams);
        }
        catch (Exception ex)
        {
            // تسجيل الخطأ
            Android.Util.Log.Error("FloatingService", ex.Message);
        }
    }

    private BroadcastReceiver receiver = new FloatingButtonReceiver();

    private class FloatingButtonReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == "CHANGE_FLOATING_BUTTON_COLOR")
            {
                var service = (FloatingButtonService)context;
                service.ChangeButtonColor();
            }
        }
    }

    private void ChangeButtonColor()
    {
        if (floatingButton != null)
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
        UnregisterReceiver(receiver);
    }
}
