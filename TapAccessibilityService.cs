using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using Android.Content.PM;

namespace RandomNumberApp
{
    [Service(Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
    public class FloatBarService : AccessibilityService
    {
        private static readonly string TAG = "FloatBarService";
        private WindowManagerLayoutParams wmParams;
        private WindowManager windowManager;
        private LinearLayout floatLayout;
        private ImageButton floatButton;
        private Prefs prefs;
        private GestureDetector gestureDetector;

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info(TAG, "OnCreate");
            prefs = new Prefs(this);
            CreateFloatView();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Info(TAG, "OnStartCommand");
            UpdateFloatService();
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Info(TAG, "OnDestroy");
            if (floatLayout != null)
            {
                windowManager?.RemoveView(floatLayout);
            }
        }

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            // معالجة الأحداث
        }

        public override void OnInterrupt()
        {
            // مقاطعة
        }

        protected override void OnServiceConnected()
        {
            Log.Info(TAG, "OnServiceConnected");
            UpdateFloatService();
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        private void CreateFloatView()
        {
            // إعدادات النافذة العائمة
            wmParams = new WindowManagerLayoutParams(
                WindowManagerLayoutParams.WrapContent,
                WindowManagerLayoutParams.WrapContent,
                Build.VERSION.SdkInt >= BuildVersionCodes.O
                    ? WindowManagerTypes.ApplicationOverlay
                    : WindowManagerTypes.Phone,
                WindowManagerFlags.NotFocusable,
                Android.Graphics.Format.Translucent);

            // تحديد موقع الزر (يمين أو يسار)
            wmParams.Gravity = prefs.IsRightMode ? GravityFlags.Right | GravityFlags.Top : GravityFlags.Left | GravityFlags.Top;
            wmParams.X = 0;
            wmParams.Y = 0;

            windowManager = GetSystemService(Context.WindowService).JavaCast<WindowManager>();

            // إنشاء Layout للزر
            floatLayout = new LinearLayout(this);
            floatLayout.Orientation = Orientation.Vertical;

            // إنشاء الزر
            floatButton = new ImageButton(this);
            floatButton.SetImageResource(Android.Resource.Drawable.IcMenuCamera); // أيقونة مؤقتة
            floatButton.SetBackgroundColor(Color.Argb(150, 0, 0, 255));
            floatButton.Click += (s, e) =>
            {
                prefs.DoTouch(this);
            };

            // إضافة الزر إلى Layout
            floatLayout.AddView(floatButton);

            // إضافة النافذة العائمة
            windowManager.AddView(floatLayout, wmParams);

            // إعدادات اللمس
            var listener = new MyOnGestureListener(this, prefs);
            gestureDetector = new GestureDetector(listener);

            floatButton.Touch += (s, e) =>
            {
                if (prefs.IsFeedback)
                {
                    if (e.Event.Action == MotionEventActions.Down)
                    {
                        floatButton.SetBackgroundColor(Color.ParseColor("#ffd060"));
                    }
                    else if (e.Event.Action == MotionEventActions.Up)
                    {
                        floatButton.SetBackgroundColor(prefs.GetColor());
                        floatButton.Background.SetAlpha(prefs.Alpha);
                    }
                }
                gestureDetector.OnTouchEvent(e.Event);
                e.Handled = true;
            };
        }

        private void UpdateFloatService()
        {
            // تحديث الموقع
            wmParams.Gravity = prefs.IsRightMode ? GravityFlags.Right | GravityFlags.Top : GravityFlags.Left | GravityFlags.Top;
            wmParams.Y = prefs.Distance;
            windowManager?.UpdateViewLayout(floatLayout, wmParams);

            // إظهار أو إخفاء الزر
            floatButton.Visibility = prefs.IsEnabled ? ViewStates.Visible : ViewStates.Gone;

            // تحديث الحجم
            floatButton.SetMinimumWidth(prefs.Width);
            floatButton.SetMinimumHeight(prefs.Height);

            // تحديث اللون والشفافية
            floatButton.SetBackgroundColor(prefs.GetColor());
            floatButton.Background.SetAlpha(prefs.Alpha);
        }

        // تنفيذ إجراءات عالمية (مثل النقر على زر الرجوع)
        public void PerformGlobalAction(int action)
        {
            PerformGlobalAction((GlobalAction)action);
        }

        // تنفيذ نقرة على إحداثيات محددة
        public void PerformTap(int x, int y)
        {
            var path = new Path();
            path.MoveTo(x, y);

            var builder = new GestureDescription.Builder();
            builder.AddStroke(new GestureDescription.StrokeDescription(path, 0, 1));

            DispatchGesture(builder.Build(), null, null);
        }

        // **********************************************
        //  مستمعات اللمس والإيماءات
        // **********************************************

        private class MyOnGestureListener : GestureDetector.SimpleOnGestureListener
        {
            private FloatBarService service;
            private Prefs prefs;

            public MyOnGestureListener(FloatBarService service, Prefs prefs)
            {
                this.service = service;
                this.prefs = prefs;
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                Log.Info(TAG, "DoubleTap");
                prefs.DoDoubleClick(service);
                return false;
            }

            public override void OnLongPress(MotionEvent e)
            {
                base.OnLongPress(e);
                Log.Info(TAG, "LongPress");
                prefs.DoLongClick(service);
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                int dy = (int)(e2.GetY() - e1.GetY());
                int dx = (int)(e2.GetX() - e1.GetX());

                if (dy < -20 && Math.Abs(velocityY) > Math.Abs(velocityX))
                {
                    Log.Info(TAG, "向上");
                    prefs.DoSwipUp(service);
                }
                else if (dy > 20 && Math.Abs(velocityY) > Math.Abs(velocityX))
                {
                    Log.Info(TAG, "向下");
                    prefs.DoSwipDown(service);
                }
                else if (dx > 20 && Math.Abs(velocityX) > Math.Abs(velocityY))
                {
                    Log.Info(TAG, "向右");
                    prefs.DoSwipRight(service);
                }
                else if (dx < -20 && Math.Abs(velocityX) > Math.Abs(velocityY))
                {
                    Log.Info(TAG, "向左");
                    prefs.DoSwipLeft(service);
                }

                return false;
            }
        }
    }

    // **********************************************
    //  فئة الإعدادات
    // **********************************************

    public class Prefs
    {
        private ISharedPreferences prefs;
        private Context context;

        public Prefs(Context context)
        {
            this.context = context;
            prefs = context.GetSharedPreferences("floatbar_prefs", FileCreationMode.Private);
        }

        public bool IsRightMode
        {
            get => prefs.GetBoolean("right_mode", false);
            set => prefs.Edit().PutBoolean("right_mode", value).Apply();
        }

        public int Distance
        {
            get => prefs.GetInt("distance", 0);
            set => prefs.Edit().PutInt("distance", value).Apply();
        }

        public bool IsEnabled
        {
            get => prefs.GetBoolean("enabled", true);
            set => prefs.Edit().PutBoolean("enabled", value).Apply();
        }

        public int Width
        {
            get => prefs.GetInt("width", 100);
            set => prefs.Edit().PutInt("width", value).Apply();
        }

        public int Height
        {
            get => prefs.GetInt("height", 100);
            set => prefs.Edit().PutInt("height", value).Apply();
        }

        public int Alpha
        {
            get => prefs.GetInt("alpha", 150);
            set => prefs.Edit().PutInt("alpha", value).Apply();
        }

        public int Color
        {
            get => prefs.GetInt("color", Color.Blue);
            set => prefs.Edit().PutInt("color", value).Apply();
        }

        public bool IsFeedback
        {
            get => prefs.GetBoolean("feedback", true);
            set => prefs.Edit().PutBoolean("feedback", value).Apply();
        }

        public Color GetColor()
        {
            return new Color(Color);
        }

        public void DoTouch(Context context)
        {
            // تنفيذ إجراء عند النقر على الزر العائم
            Toast.MakeText(context, "تم النقر على الزر العائم!", ToastLength.Short).Show();
            // هنا يمكنك تنفيذ الخلط أو أي إجراء آخر
        }

        public void DoDoubleClick(Context context)
        {
            Toast.MakeText(context, "نقر مزدوج!", ToastLength.Short).Show();
        }

        public void DoLongClick(Context context)
        {
            Toast.MakeText(context, "ضغطة طويلة!", ToastLength.Short).Show();
        }

        public void DoSwipUp(Context context)
        {
            Toast.MakeText(context, "سحب لأعلى!", ToastLength.Short).Show();
        }

        public void DoSwipDown(Context context)
        {
            Toast.MakeText(context, "سحب لأسفل!", ToastLength.Short).Show();
        }

        public void DoSwipRight(Context context)
        {
            Toast.MakeText(context, "سحب لليمين!", ToastLength.Short).Show();
        }

        public void DoSwipLeft(Context context)
        {
            Toast.MakeText(context, "سحب لليسار!", ToastLength.Short).Show();
        }

        public void ClearPrefs()
        {
            prefs.Edit().Clear().Apply();
        }
    }
}
