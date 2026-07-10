using Android.AccessibilityServices;
using Android.App;
using Android.Views.Accessibility;

namespace RandomNumberApp;

[Service(
    Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
    Exported = true,
    Label = "RandomApp Accessibility")]
[IntentFilter(
    new[] {
        "android.accessibilityservice.AccessibilityService"
    })]
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

        Android.Util.Log.Error(
            "ACCESSIBILITY",
            "Service Connected");
    }

    public override void OnAccessibilityEvent(AccessibilityEvent e)
    {
    }

    public override void OnInterrupt()
    {
    }
}
