using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Views.Accessibility;

namespace RandomNumberApp;

[Service(
    Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
    Exported = true,
    Label = "RandomApp")]
[IntentFilter(
    new[] { "android.accessibilityservice.AccessibilityService" })]
[MetaData(
    "android.accessibilityservice",
    Resource = "@xml/accessibility_service_config")]
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
