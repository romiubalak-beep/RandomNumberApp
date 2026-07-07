using Android.AccessibilityServices;
using Android.Graphics;
using Android.OS;

namespace RandomNumberApp;

public static class TouchHelper
{
    public static void TapCenter()
    {
        var service = MyAccessibilityService.Instance;

        if (service == null)
            return;

        var path = new Path();

        path.MoveTo(540, 1200);

        var stroke = new GestureDescription.StrokeDescription(
            path,
            0,
            50);

        var builder = new GestureDescription.Builder();
        builder.AddStroke(stroke);

        service.DispatchGesture(
            builder.Build(),
            null,
            null);
    }
}
