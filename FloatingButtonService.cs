using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Content;
using Android.Graphics;

[Service]
public class FloatingButtonService : Service
{
    private Android.Views.WindowManager windowManager;
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
        windowManager = GetSystemService(Context.WindowService) as Android.Views.WindowManager;
        
        // إنشاء الزر برمجياً بدلاً من XML
        floatingButton = new Button(this);
        floatingButton.Text = "⚡";
        floatingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 30);
        floatingButton.SetBackgroundColor(Android.Graphics.Color.Blue);
        floatingButton.SetTextColor(Android.Graphics.Color.White);
        
        // إنشاء Layout لاحتواء الزر
        floatingView = new LinearLayout(this);
        var layoutParams = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.WrapContent,
            LinearLayout.LayoutParams.WrapContent);
        floatingView.LayoutParameters = layoutParams;
        ((LinearLayout)floatingView).AddView(floatingButton);
        
        // إعدادات النافذة العائمة
        var parameters = new WindowManagerLayoutParams(
            WindowManagerLayoutParams.WrapContent,
            WindowManagerLayoutParams.WrapContent,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotFocusable,
            Android.Graphics.Format.Translucent
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
