using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Content;

[Service]
public class FloatingButtonService : Service
{
    private WindowManager windowManager;
    private View floatingView;
    private Button floatingButton;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        
        // إنشاء الزر العائم
        windowManager = GetSystemService(WindowService) as WindowManager;
        
        floatingView = LayoutInflater.From(this).Inflate(Resource.Layout.floating_button, null);
        floatingButton = floatingView.FindViewById<Button>(Resource.Id.floatingButton);
        
        WindowManagerLayoutParams parameters = new WindowManagerLayoutParams(
            WindowManagerLayoutParams.WrapContent,
            WindowManagerLayoutParams.WrapContent,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotFocusable,
            Format.Translucent
        );
        
        parameters.Gravity = GravityFlags.Top | GravityFlags.Right;
        parameters.X = 0;
        parameters.Y = 100;
        
        windowManager.AddView(floatingView, parameters);
        
        floatingButton.Click += (s, e) =>
        {
            Toast.MakeText(this, "الخلط قيد التشغيل!", ToastLength.Short).Show();
        };
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (floatingView != null)
        {
            windowManager.RemoveView(floatingView);
        }
    }
}
