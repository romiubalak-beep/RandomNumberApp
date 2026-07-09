using Android.AccessibilityServices;
using Android.OS;
using Android.Util;

namespace RandomNumberApp;

public static class TouchHelper
{
    public static void TapCenter()
    {
        var service = MyAccessibilityService.Instance;

        if (service == null)
        {
            Log.Error(
                "ACCESSIBILITY",
                "Service is NULL");

            return;
        }

        Log.Debug(
            "ACCESSIBILITY",
            "TapCenter Called");

        var path = new Android.Graphics.Path();

        // ✅ الحصول على إحداثيات منتصف الشاشة ديناميكياً
        var wm = service.GetSystemService(
            Android.Content.Context.WindowService)
            as Android.Views.IWindowManager;

        var metrics = new Android.Util.DisplayMetrics();

        service.Display?.GetRealMetrics(metrics);

        float x = metrics.WidthPixels / 2f;
        float y = metrics.HeightPixels / 2f;

        path.MoveTo(x, y);

        var stroke =
            new GestureDescription.StrokeDescription(
                path,
                0,
                50);

        var builder =
            new GestureDescription.Builder();

        builder.AddStroke(stroke);

        var gesture = builder.Build();

        bool result = service.DispatchGesture(
            gesture,
            new TapCallback(),
            null);

        Log.Debug("ACCESSIBILITY",
            $"DispatchGesture={result}");
    }

    private class TapCallback
        : AccessibilityService.GestureResultCallback
    {
        public override void OnCompleted(
            GestureDescription? gestureDescription)
        {
            Log.Debug(
                "ACCESSIBILITY",
                "Gesture Completed");
        }

        public override void OnCancelled(
            GestureDescription? gestureDescription)
        {
            Log.Error(
                "ACCESSIBILITY",
                "Gesture Cancelled");
        }
    }
}
