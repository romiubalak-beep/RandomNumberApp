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

    // ✅ مخزن واحد كبير (Buffer) بسعة 15 مليون
    private readonly List<int> allNumbers = new List<int>(15000000);
    private int position = 0;

    // ✅ مدة الخلط 5 ثوان
    private const int ShuffleDurationMs = 5000;

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

    // ✅ دالة الخلط الجديدة مع 10 تبديلات عشوائية
    private void Shuffle()
    {
        for (int i = 0; i < 10; i++)
        {
            int a = RandomNumberGenerator.GetInt32(150);
            int b = RandomNumberGenerator.GetInt32(150);

            (currentNumbers[a],
             currentNumbers[b]) =
            (currentNumbers[b],
             currentNumbers[a]);
        }
    }

    // ✅ النسخة مع مدة خلط 5 ثوان
    private async void StartShuffle()
    {
        try
        {
            TouchHelper.TapCenter();

            var startTime = DateTime.UtcNow;

            // ✅ إعادة تعيين المخزن
            allNumbers.Clear();
            position = 0;

            // ✅ الخلط لمدة 5 ثوان
            while ((DateTime.UtcNow - startTime).TotalMilliseconds < ShuffleDurationMs)
            {
                Shuffle();

                // ✅ إضافة الأرقام إلى المخزن مباشرة
                for (int i = 0; i < 150; i++)
                {
                    allNumbers.Add(currentNumbers[i]);
                    position++;
                }
            }

            SaveLog();
        }
        finally
        {
            isShuffling = false;
        }
    }

    // ✅ دالة الحفظ المحسنة مع StringBuilder من المخزن الواحد
    private void SaveLog()
    {
        try
        {
            var sb = new StringBuilder();

            int totalArrays = position / 150;

            sb.AppendLine($"===== SHUFFLE LOG =====");
            sb.AppendLine($"Duration: {ShuffleDurationMs}ms");
            sb.AppendLine($"Total arrays: {totalArrays}");
            sb.AppendLine("");

            // ✅ قراءة من المخزن الواحد
            for (int p = 0; p < position; p += 150)
            {
                for (int i = 0; i < 150; i++)
                {
                    sb.Append(allNumbers[p + i]);
                    sb.Append(' ');
                }

                sb.AppendLine();
            }

            string text = sb.ToString();

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

            if (uri == null)
                return;

            using var stream =
                ContentResolver.OpenOutputStream(uri);

            using var writer =
                new StreamWriter(stream!);

            writer.Write(text);

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

                    activity.cancellationToken =
                        new System.Threading.CancellationTokenSource();

                    activity.StartShuffle();
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
