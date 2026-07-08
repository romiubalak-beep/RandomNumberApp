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

        path.MoveTo(540, 1200);

        var stroke =
            new GestureDescription.StrokeDescription(
                path,
                0,
                50);

        var builder =
            new GestureDescription.Builder();

        builder.AddStroke(stroke);

        var gesture = builder.Build();

        service.DispatchGesture(
            gesture,
            new TapCallback(),
            null);
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
