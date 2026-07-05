using Android.App;
using Android.AccessibilityServices;
using Android.OS;
using Android.Views;

[Service(Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
public class TapAccessibilityService : AccessibilityService
{
    // ✅ استخدام AccessibilityEvent? مع علامة الاستفهام
    public override void OnAccessibilityEvent(AccessibilityEvent? e)
    {
        // معالجة الأحداث
    }

    public override void OnInterrupt()
    {
        // مقاطعة
    }

    public void PerformTap(int x, int y)
    {
        try
        {
            var path = new Path();
            path.MoveTo(x, y);
            
            var builder = new GestureDescription.Builder();
            builder.AddStroke(new GestureDescription.StrokeDescription(path, 0, 1));
            
            DispatchGesture(builder.Build(), null, null);
        }
        catch (System.Exception ex)
        {
            Android.Util.Log.Error("TapService", "Tap Error: " + ex.Message);
        }
    }
}
