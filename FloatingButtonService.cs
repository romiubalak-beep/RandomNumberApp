using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Content;
using Android.Runtime;
using Android.Provider;
using Android.AccessibilityServices;

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
            
            floatingButton.Click += (s, e) => {
                if (!isPaused)
                {
                    isShuffling = !isShuffling;
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
                else
                {
                    Toast.MakeText(this, "⏳ توقف مؤقت بسبب العثور على الرقم", ToastLength.Short).Show();
                }
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
            Toast.MakeText(this, "✅ الزر العائم جاهز", ToastLength.Short).Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, "❌ خطأ: " + ex.Message, ToastLength.Long).Show();
            Android.Util.Log.Error("FloatingService", "Create Error: " + ex.Message);
        }
    }

    public void PauseShuffling(bool pause)
    {
        isPaused = pause;
        if (pause)
        {
            floatingButton.Text = "⏸";
            floatingButton.SetBackgroundColor(Color.Orange);
        }
        else
        {
            floatingButton.Text = "▶";
            floatingButton.SetBackgroundColor(Color.Blue);
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
        if (floatingButton != null && isCreated && !isPaused)
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
