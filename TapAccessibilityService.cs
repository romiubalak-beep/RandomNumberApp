using Android.App;
using Android.AccessibilityServices;
using Android.OS;
using Android.Views;
using Android.Views.Accessibility;
using Android.Content;
using System;

[Service(Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE")]
public class TapAccessibilityService : AccessibilityService
{
    // ✅ مثل Klick'r: متغيرات للتحكم بحالة الخدمة
    private bool isStarted = false;
    private Action<int, int>? onTapAction;

    public override void OnAccessibilityEvent(AccessibilityEvent? e)
    {
        // مثل Klick'r: معالجة الأحداث (غير مستخدمة حالياً)
    }

    public override void OnInterrupt()
    {
        // مثل Klick'r: مقاطعة الخدمة
    }

    public override void OnServiceConnected()
    {
        base.OnServiceConnected();
        // ✅ مثل Klick'r: تهيئة الخدمة عند الاتصال
        Android.Util.Log.Info("TapService", "✅ Service connected");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // ✅ مثل Klick'r: تنظيف الموارد عند التدمير
        Android.Util.Log.Info("TapService", "❌ Service destroyed");
    }

    // ✅ مثل Klick'r: دالة بدء الخدمة
    public void StartService()
    {
        if (!isStarted)
        {
            isStarted = true;
            Android.Util.Log.Info("TapService", "▶ Service started");
        }
    }

    // ✅ مثل Klick'r: دالة إيقاف الخدمة
    public void StopService()
    {
        if (isStarted)
        {
            isStarted = false;
            Android.Util.Log.Info("TapService", "⏹ Service stopped");
        }
    }

    // ✅ مثل Klick'r: تنفيذ النقرة (dispatchGesture)
    public void PerformTap(int x, int y)
    {
        if (!isStarted)
        {
            Android.Util.Log.Warn("TapService", "⚠️ Service not started, cannot perform tap");
            return;
        }

        try
        {
            var path = new Android.Graphics.Path();
            path.MoveTo(x, y);
            
            var builder = new GestureDescription.Builder();
            builder.AddStroke(new GestureDescription.StrokeDescription(path, 0, 1));
            
            DispatchGesture(builder.Build(), new TapGestureResultCallback(), null);
            Android.Util.Log.Info("TapService", $"👆 Tap performed at ({x}, {y})");
        }
        catch (System.Exception ex)
        {
            Android.Util.Log.Error("TapService", "Tap Error: " + ex.Message);
        }
    }

    // ✅ مثل Klick'r: معالج نتيجة النقرة
    private class TapGestureResultCallback : GestureResultCallback
    {
        public override void OnCompleted(GestureDescription gestureDescription)
        {
            Android.Util.Log.Info("TapService", "✅ Tap completed successfully");
        }

        public override void OnCancelled(GestureDescription gestureDescription)
        {
            Android.Util.Log.Warn("TapService", "⚠️ Tap was cancelled");
        }
    }

    // ✅ مثل Klick'r: دالة الحصول على حالة الخدمة
    public bool IsStarted() => isStarted;
}
