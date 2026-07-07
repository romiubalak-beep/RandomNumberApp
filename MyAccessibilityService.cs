using Android.AccessibilityServices;
using Android.Graphics;
using Android.Views.Accessibility;

namespace RandomNumberApp;

[Service(
    Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
    Exported = true)]
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
