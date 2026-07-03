using Android.App;
using Android.AccessibilityServices;
using Android.Views;
using Android.Content;
using Android.OS;

[Service(Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
public class MyAccessibilityService : AccessibilityService
{
    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
        // معالجة الأحداث
    }

    public override void OnInterrupt()
    {
        // مقاطعة
    }
}
