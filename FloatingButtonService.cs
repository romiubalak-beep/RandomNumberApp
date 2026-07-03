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
        CreateFloatingButton();
        
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
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            
            // إعدادات النافذة العائمة
            var parameters = new WindowManagerLayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent,
                WindowManagerTypes.ApplicationOverlay,
                WindowManagerFlags.NotFocusable,
                Format.Translucent);
            
            parameters.Gravity = GravityFlags.Top | GravityFlags.Right;
            parameters.X = 0;
            parameters.Y = 200;
            
            // إضافة الزر
            windowManager.AddView(floatingView, parameters);
        }
        catch (Exception ex)
        {
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
