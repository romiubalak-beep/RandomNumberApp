using Android.AccessibilityServices;
using Android.Runtime;
using Android.Content;

namespace RandomNumberApp;

[Register("com.example.randomapp.MyAccessibilityService")]
[Service(
    Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
    Exported = true)]
[IntentFilter(new[]
{
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
    }

    public override void OnAccessibilityEvent(
        Android.Views.Accessibility.AccessibilityEvent? e)
    {
    }

    public override void OnInterrupt()
    {
    }
}
