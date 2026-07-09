using Android.AccessibilityServices;
using Android.Runtime;
using Android.Views.Accessibility;

namespace RandomNumberApp;

[Register("com.example.randomapp.MyAccessibilityService")]
[Service(
    Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
    Exported = true)]
[IntentFilter(new[]
{
    "android.accessibilityservice.AccessibilityService"
})]
public class MyAccessibilityService : AccessibilityService
{
    public static MyAccessibilityService? Instance { get; private set; }

    protected override void OnServiceConnected()
    {
        base.OnServiceConnected();
        Instance = this;
    }

    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
    }

    public override void OnInterrupt()
    {
    }
}
