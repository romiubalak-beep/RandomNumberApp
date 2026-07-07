namespace RandomNumberApp;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

[Service]
public class FloatingButtonService : Service, View.IOnTouchListener
{
    private IWindowManager? windowManager;
    private View? floatingView;
    private Button? floatingButton;

    private int initialX;
    private int initialY;
    private float initialTouchX;
    private float initialTouchY;
    private bool isDragging;

    public override IBinder? OnBind(Intent intent)
    {
        return null;
    }

    public override void OnCreate()
    {
        base.OnCreate();

        Handler handler = new Handler(Looper.MainLooper!);
        handler.Post(CreateFloatingButton);
    }

    private void CreateFloatingButton()
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M &&
                !Android.Provider.Settings.CanDrawOverlays(this))
            {
                return;
            }

            var inflater = LayoutInflater.From(this);

            floatingView =
                inflater.Inflate(
                    Resource.Layout.floating_layout,
                    null);

            floatingButton =
                floatingView.FindViewById<Button>(
                    Resource.Id.floatingButton);

            if (floatingButton == null)
                return;

            floatingButton.SetOnTouchListener(this);

            floatingButton.Click += (s, e) =>
            {
                if (!isDragging)
                    ToggleShuffle();

                isDragging = false;
            };

            windowManager =
                GetSystemService(WindowService)
                .JavaCast<IWindowManager>();

            WindowManagerTypes type =
                Build.VERSION.SdkInt >= BuildVersionCodes.O
                ? WindowManagerTypes.ApplicationOverlay
                : WindowManagerTypes.Phone;

            var parameters = new WindowManagerLayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent,
                type,
                WindowManagerFlags.NotFocusable,
                Format.Translucent);

            parameters.Gravity =
                GravityFlags.Top | GravityFlags.Start;

            parameters.X = 50;
            parameters.Y = 200;

            windowManager.AddView(
                floatingView,
                parameters);
        }
        catch (System.Exception ex)
        {
            Android.Util.Log.Error(
                "FloatingButtonService",
                ex.ToString());
        }
    }

    // ✅ الدالة المطلوبة مع SetPackage
    private void ToggleShuffle()
    {
        if (floatingButton == null)
            return;

        Toast.MakeText(
            this,
            "Button Pressed",
            ToastLength.Short).Show();

        if (floatingButton.Text == "▶")
        {
            floatingButton.Text = "⏹";

            var intent = new Intent("START_SHUFFLING");
            intent.SetPackage(PackageName);

            SendBroadcast(intent);

            Toast.MakeText(
                this,
                "START sent",
                ToastLength.Short).Show();
        }
        else
        {
            floatingButton.Text = "▶";

            var intent = new Intent("STOP_SHUFFLING");
            intent.SetPackage(PackageName);

            SendBroadcast(intent);

            Toast.MakeText(
                this,
                "STOP sent",
                ToastLength.Short).Show();
        }
    }

    public bool OnTouch(View? v, MotionEvent? e)
    {
        if (windowManager == null ||
            floatingView == null ||
            e == null)
            return false;

        var lp =
            (WindowManagerLayoutParams)
            floatingView.LayoutParameters!;

        switch (e.Action)
        {
            case MotionEventActions.Down:

                initialX = lp.X;
                initialY = lp.Y;

                initialTouchX = e.RawX;
                initialTouchY = e.RawY;

                isDragging = false;
                return true;

            case MotionEventActions.Move:

                float dx = e.RawX - initialTouchX;
                float dy = e.RawY - initialTouchY;

                if (Math.Abs(dx) > 10 ||
                    Math.Abs(dy) > 10)
                {
                    isDragging = true;

                    lp.X = initialX + (int)dx;
                    lp.Y = initialY + (int)dy;

                    windowManager.UpdateViewLayout(
                        floatingView,
                        lp);
                }

                return true;

            case MotionEventActions.Up:

                if (!isDragging)
                {
                    v?.PerformClick();
                }

                return true;
        }

        return false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (floatingView != null &&
            windowManager != null)
        {
            try
            {
                windowManager.RemoveView(floatingView);
            }
            catch
            {
            }
        }
    }
}
