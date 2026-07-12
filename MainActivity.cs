namespace RandomNumberApp;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Java.IO;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView? numbersTextView;
    private Button? resetButton;

    private List<int> originalNumbers = new();
    private int[] currentNumbers = new int[150]; // ✅ int[] بدلاً من List<int>

    private bool isShuffling;
    private System.Threading.CancellationTokenSource? cancellationToken;

    private ShuffleReceiver? receiver;

    // ✅ قوائم التخزين باستخدام Queue للسرعة
    private readonly Queue<int[]> beforeTap = new Queue<int[]>();
    private readonly List<int[]> duringTap = new List<int[]>(5000);
    private readonly Queue<int[]> afterTap = new Queue<int[]>();

    private const int BEFORE_COUNT = 1500;
    private const int AFTER_COUNT = 1500;

    private bool targetTriggered = false;
    private int afterTapCount = 0;

    // ✅ متغيرات تتبع النقرة
    private bool tapInProgress = false;
    private bool tapFinished = false;
    private int tapShuffleCount = 0;

    // ✅ المصفوفة المستهدفة
    private int[]? targetArray = null;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        for (int i = 0; i < 150; i++)
            originalNumbers.Add(i + 1);

        // ✅ نسخ الأرقام إلى المصفوفة
        for (int i = 0; i < 150; i++)
            currentNumbers[i] = originalNumbers[i];

        numbersTextView = FindViewById<TextView>(Resource.Id.numbersTextView);
        resetButton = FindViewById<Button>(Resource.Id.resetButton);

        UpdateNumbers();

        if (resetButton != null)
        {
            resetButton.Click += (s, e) =>
            {
                // ✅ إعادة تعيين المصفوفة
                for (int i = 0; i < 150; i++)
                    currentNumbers[i] = originalNumbers[i];

                UpdateNumbers();
            };
        }

        CheckOverlayPermission();
        StartFloatingButtonService();

        receiver = new ShuffleReceiver(this);

        var filter = new IntentFilter();
        filter.AddAction("START_SHUFFLING");
        filter.AddAction("STOP_SHUFFLING");
        filter.AddAction("TAP_FINISHED");

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            RegisterReceiver(
                receiver,
                filter,
                ReceiverFlags.NotExported);
        }
        else
        {
            RegisterReceiver(receiver, filter);
        }
    }

    private void StartFloatingButtonService()
    {
        try
        {
            StartService(
                new Intent(
                    this,
                    typeof(FloatingButtonService)));
        }
        catch (Exception ex)
        {
            Toast.MakeText(
                this,
                ex.Message,
                ToastLength.Long).Show();
        }
    }

    private void CheckOverlayPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            if (!Settings.CanDrawOverlays(this))
            {
                var intent = new Intent(
                    Settings.ActionManageOverlayPermission,
                    Android.Net.Uri.Parse(
                        "package:" + PackageName));

                StartActivity(intent);
            }
        }
    }

    private string FormatNumbers(int[] numbers)
    {
        string result = "";
        int count = 0;

        foreach (var n in numbers)
        {
            result += n.ToString().PadLeft(3) + " ";
            count++;

            if (count % 15 == 0)
                result += "\n";
        }

        return result;
    }

    private void UpdateNumbers()
    {
        if (numbersTextView != null)
            numbersTextView.Text = FormatNumbers(currentNumbers);
    }

    private async void StartShuffle(
        System.Threading.CancellationToken token)
    {
        try
        {
            int refreshCounter = 0;

            while (isShuffling &&
                   !token.IsCancellationRequested)
            {
                int n = currentNumbers.Length;

                // ✅ خلط المصفوفة مباشرة
                for (int i = n - 1; i > 0; i--)
                {
                    int j =
                        RandomNumberGenerator.GetInt32(i + 1);

                    (currentNumbers[i],
                     currentNumbers[j]) =
                    (currentNumbers[j],
                     currentNumbers[i]);
                }

                refreshCounter++;

                if (refreshCounter >= 50)
                {
                    refreshCounter = 0;
                    RunOnUiThread(UpdateNumbers);
                }

                // ✅ أخذ نسخة واحدة فقط من المصفوفة في كل دورة
                int[] snapshot = (int[])currentNumbers.Clone();

                // ✅ التسجيل قبل العثور على الهدف
                if (!targetTriggered && !tapInProgress && !tapFinished)
                {
                    beforeTap.Enqueue(snapshot);

                    while (beforeTap.Count > BEFORE_COUNT)
                        beforeTap.Dequeue();
                }

                // ✅ التسجيل أثناء النقرة
                if (tapInProgress)
                {
                    tapShuffleCount++;

                    duringTap.Add(snapshot);
                }

                // ✅ التسجيل بعد النقرة مع التحقق من اكتمال العدد
                if (tapFinished)
                {
                    afterTap.Enqueue(snapshot);

                    while (afterTap.Count > AFTER_COUNT)
                        afterTap.Dequeue();

                    afterTapCount++;

                    if (afterTapCount >= AFTER_COUNT)
                    {
                        SaveLog();

                        isShuffling = false;

                        break;
                    }
                }

                // ✅ الشرط: العثور على الرقم 1 فقط
                if (!targetTriggered && !tapInProgress && !tapFinished &&
                    currentNumbers[0] == 1)
                {
                    // ✅ حفظ المصفوفة المستهدفة
                    targetArray = (int[])currentNumbers.Clone();

                    // ✅ تسجيل العثور على الهدف
                    targetTriggered = true;

                    // ✅ بدء تتبع النقرة
                    tapInProgress = true;
                    tapFinished = false;
                    tapShuffleCount = 0;
                    duringTap.Clear();

                    // ✅ تنفيذ النقرة
                    TouchHelper.TapCenter();

                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(
                            this,
                            "🎯 تم العثور على الرقم 1",
                            ToastLength.Short).Show();
                    });
                }

                // ✅ حذف await Task.Yield() للسرعة القصوى
            }
        }
        catch
        {
        }
        finally
        {
            isShuffling = false;
        }
    }

    // ✅ دالة حفظ السجل
    private void SaveLog()
    {
        try
        {
            var lines = new List<string>();

            lines.Add($"===== {BEFORE_COUNT} BEFORE =====");

            foreach (var arr in beforeTap)
            {
                lines.Add(string.Join(" ", arr));
            }

            lines.Add("");

            lines.Add($"===== TARGET =====");

            if (targetArray != null)
            {
                lines.Add(string.Join(" ", targetArray));
            }

            lines.Add("");

            lines.Add($"===== DURING TAP =====");

            foreach (var arr in duringTap)
            {
                lines.Add(string.Join(" ", arr));
            }

            lines.Add("");

            lines.Add($"===== {AFTER_COUNT} AFTER =====");

            foreach (var arr in afterTap)
            {
                lines.Add(string.Join(" ", arr));
            }

            string text =
                string.Join("\n", lines);

            ContentValues values =
                new ContentValues();

            values.Put(
                MediaStore.IMediaColumns.DisplayName,
                $"shuffle_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            values.Put(
                MediaStore.IMediaColumns.MimeType,
                "text/plain");

            values.Put(
                MediaStore.IMediaColumns.RelativePath,
                Android.OS.Environment.DirectoryDownloads);

            var uri =
                ContentResolver.Insert(
                    MediaStore.Downloads.ExternalContentUri,
                    values);

            if (uri != null)
            {
                using var stream =
                    ContentResolver.OpenOutputStream(uri);

                using var writer =
                    new StreamWriter(stream!);

                writer.Write(text);
            }

            RunOnUiThread(() =>
            {
                Toast.MakeText(
                    this,
                    "تم حفظ الملف في Download",
                    ToastLength.Long)
                    .Show();
            });
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error(
                "SAVE_FILE",
                ex.ToString());
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        try
        {
            if (receiver != null)
                UnregisterReceiver(receiver);
        }
        catch
        {
        }
    }

    private class ShuffleReceiver : BroadcastReceiver
    {
        private readonly MainActivity activity;

        public ShuffleReceiver(MainActivity activity)
        {
            this.activity = activity;
        }

        public override void OnReceive(
            Context? context,
            Intent? intent)
        {
            if (intent == null)
                return;

            if (intent.Action == "START_SHUFFLING")
            {
                if (!activity.isShuffling)
                {
                    activity.isShuffling = true;
                    activity.targetTriggered = false;
                    activity.afterTapCount = 0;
                    activity.tapInProgress = false;
                    activity.tapFinished = false;
                    activity.tapShuffleCount = 0;
                    activity.targetArray = null;
                    activity.beforeTap.Clear();
                    activity.duringTap.Clear();
                    activity.afterTap.Clear();

                    activity.cancellationToken =
                        new System.Threading.CancellationTokenSource();

                    activity.StartShuffle(
                        activity.cancellationToken.Token);
                }
            }

            if (intent.Action == "STOP_SHUFFLING")
            {
                activity.isShuffling = false;

                activity.cancellationToken?.Cancel();

                // ✅ إعادة تعيين المصفوفة
                for (int i = 0; i < 150; i++)
                    activity.currentNumbers[i] = activity.originalNumbers[i];

                activity.RunOnUiThread(
                    activity.UpdateNumbers);
            }

            if (intent.Action == "TAP_FINISHED")
            {
                activity.tapInProgress = false;
                activity.tapFinished = true;
                activity.afterTapCount = 0;
                activity.targetTriggered = true;
            }
        }
    }
}
