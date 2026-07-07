using Android.AccessibilityServices;
using Android.OS;

namespace RandomNumberApp;

public static class TouchHelper
{
    public static void TapCenter()
    {
        var service = MyAccessibilityService.Instance;

        if (service == null)
            return;

        var path = new Android.Graphics.Path();

        path.MoveTo(540, 1200);

        var stroke = new GestureDescription.StrokeDescription(
            path,
            0,
            50);

        var builder = new GestureDescription.Builder();
        builder.AddStroke(stroke);

        var gesture = builder.Build();

        if (gesture != null)
        {
            service.DispatchGesture(
                gesture,
                null,
                (Handler?)null);
        }
    }
}
