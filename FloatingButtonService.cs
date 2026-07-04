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
    private bool isShuffling = false;
    private bool isPaused = false;

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
        IntentFilter filter = new IntentFilter("CHANGE_FLOATING_BUTTON_COLOR");
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            RegisterReceiver(receiver, filter, ReceiverFlags.NotExported);
        }
        else
        {
            RegisterReceiver(receiver, filter);
        }
    }

    // باقي الكود كما هو...
    // (احتفظ بنفس الكود السابق للـ FloatingButtonService)
}
