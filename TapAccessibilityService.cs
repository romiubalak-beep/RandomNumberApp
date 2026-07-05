using Android.App;
using Android.AccessibilityServices;
using Android.OS;
using Android.Views;
using Android.Views.Accessibility;

[Service(Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
public class TapAccessibilityService : AccessibilityService
{
    private bool isStarted = false;

    public override void OnAccessibilityEvent(AccessibilityEvent? e)
    {
        // معالجة الأحداث
    }

    public override void OnInterrupt()
    {
        // مقاطعة
    }

    public override void OnServiceConnected()
    {
        base.OnServiceConnected();
        Android.Util.Log.Info("TapService", "✅ Service connected");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Android.Util.Log.Info("TapService", "❌ Service destroyed");
    }

    public void StartService()
    {
        if (!isStarted)
        {
            isStarted = true;
            Android.Util.Log.Info("TapService", "▶ Service started");
        }
    }

    public void StopService()
    {
        if (isStarted)
        {
            isStarted = false;
            Android.Util.Log.Info("TapService", "⏹ Service stopped");
        }
    }

    public void PerformTap(int x, int y)
    {
        if (!isStarted)
        {
            Android.Util.Log.Warn("TapService", "⚠️ Service not started");
            return;
        }

        try
        {
            var path = new Android.Graphics.Path();
            path.MoveTo(x, y);
            
            var builder = new GestureDescription.Builder();
            builder.AddStroke(new GestureDescription.StrokeDescription(path, 0, 1));
            
            DispatchGesture(builder.Build(), new TapGestureResultCallback(), null);
        }
        catch (System.Exception ex)
        {
            Android.Util.Log.Error("TapService", "Tap Error: " + ex.Message);
        }
    }

    private class TapGestureResultCallback : GestureResultCallback
    {
        public override void OnCompleted(GestureDescription gestureDescription)
        {
            Android.Util.Log.Info("TapService", "✅ Tap completed");
        }

        public override void OnCancelled(GestureDescription gestureDescription)
        {
            Android.Util.Log.Warn("TapService", "⚠️ Tap cancelled");
        }
    }

    public bool IsStarted() => isStarted;
}
