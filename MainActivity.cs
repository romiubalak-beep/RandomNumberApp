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
    private List<int> currentNumbers = new();

    private bool isShuffling;
    private System.Threading.CancellationTokenSource? cancellationToken;

    private ShuffleReceiver? receiver;

    // ✅ المتغيرات الجديدة
    private DateTime shuffleStartTime;
    private bool tapExecuted;
    private List<int[]> allArrays = new();

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        for (int i = 1; i <= 150; i++)
            originalNumbers.Add(i);

        currentNumbers = new List<int>(originalNumbers);

        numbersTextView = FindViewById<TextView>(Resource.Id.numbersTextView);
        resetButton = FindViewById<Button>(Resource.Id.resetButton);

        UpdateNumbers();

        if (resetButton != null)
        {
            resetButton.Click += (s, e) =>
            {
                currentNumbers = new List<int>(originalNumbers);
                UpdateNumbers();
            };
        }

        CheckOverlayPermission();
        StartFloatingButtonService();

        receiver = new ShuffleReceiver(this);

        var filter = new IntentFilter();
        filter.AddAction("START_SHUFFLING");
        filter.AddAction("STOP_SHUFFLING");

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

    private string FormatNumbers(List<int> numbers)
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

    // ✅ النسخة المطلوبة مع التوقيت والنقرة في الثانية الثانية
    private async void StartShuffle(
        System.Threading.CancellationToken token)
    {
        try
        {
            int refreshCounter = 0;

            while (isShuffling &&
                   !token.IsCancellationRequested)
            {
                // ✅ حساب الوقت المنقضي
                double elapsed =
                    (DateTime.UtcNow -
                     shuffleStartTime)
                    .TotalSeconds;

                int n = currentNumbers.Count;

                for (int i = n - 1; i > 0; i--)
                {
                    int j =
                        RandomNumberGenerator.GetInt32(i + 1);

                    (currentNumbers[i],
                     currentNumbers[j]) =
                    (currentNumbers[j],
                     currentNumbers[i]);
                }

                // ✅ حفظ جميع المصفوفات بعد الخلط
                allArrays.Add(
                    currentNumbers.ToArray());

                refreshCounter++;

                if (refreshCounter >= 50)
                {
                    refreshCounter = 0;
                    RunOnUiThread(UpdateNumbers);
                }

                // ✅ تنفيذ النقرة فقط في الثانية الثانية (بين 1 و 2 ثانية)
                if (!tapExecuted &&
                    elapsed >= 1.0 &&
                    elapsed < 2.0 &&
                    (currentNumbers[0] == 1 ||
                     currentNumbers[0] == 2 ||
                     currentNumbers[0] == 3))
                {
                    tapExecuted = true;

                    allArrays.Add(
                        new int[] { -999 });

                    TouchHelper.TapCenter();

                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(
                            this,
                            $"🎯 تم العثور على الرقم {currentNumbers[0]}",
                            ToastLength.Short).Show();
                    });
                }

                // ✅ إيقاف الخلط بعد ثانيتين
                if (elapsed >= 2.0)
                {
                    SaveLog();

                    isShuffling = false;

                    RunOnUiThread(UpdateNumbers);

                    break;
                }

                await Task.Yield();
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

    // ✅ دالة حفظ السجل المعدلة
    private void SaveLog()
    {
        try
        {
            var lines = new List<string>();

            lines.Add("===== SHUFFLE LOG =====");
            lines.Add($"Total arrays: {allArrays.Count}");
            lines.Add("");

            foreach (var arr in allArrays)
            {
                if (arr.Length == 1 &&
                    arr[0] == -999)
                {
                    lines.Add(
                        "========== TAP ==========");
                }
                else
                {
                    lines.Add(
                        string.Join(" ", arr));
                }
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
                    // ✅ التهيئة الجديدة
                    activity.isShuffling = true;
                    activity.tapExecuted = false;
                    activity.allArrays.Clear();
                    activity.shuffleStartTime = DateTime.UtcNow;

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

                activity.currentNumbers =
                    new List<int>(activity.originalNumbers);

                activity.RunOnUiThread(
                    activity.UpdateNumbers);
            }
        }
    }
}
