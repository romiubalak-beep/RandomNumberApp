namespace RandomNumberApp;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

[Activity(Label = "RandomApp", MainLauncher = true)]
public class MainActivity : Activity
{
    private TextView? numbersTextView;
    private Button? resetButton;

    private RandomNumberGenerator? rng;

    private List<int> originalNumbers = new();
    private List<int> currentNumbers = new();

    private bool isShuffling;
    private System.Threading.CancellationTokenSource? cancellationToken;

    private ShuffleReceiver? receiver;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        rng = RandomNumberGenerator.Create();

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

        RegisterReceiver(receiver, filter);
    }

    // بقية الملف كما هو بدون تغيير
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
            {
                result += "\n";
            }
        }

        return result;
    }

    private void UpdateNumbers()
    {
        if (numbersTextView != null)
        {
            numbersTextView.Text =
                FormatNumbers(currentNumbers);
        }
    }

    private async void StartShuffle(
        System.Threading.CancellationToken token)
    {
        try
        {
            while (isShuffling &&
                   !token.IsCancellationRequested)
            {
                int n = currentNumbers.Count;

                for (int i = n - 1; i > 0; i--)
                {
                    byte[] bytes = new byte[4];

                    rng!.GetBytes(bytes);

                    int j =
                        Math.Abs(
                            BitConverter.ToInt32(
                                bytes,
                                0)
                            % (i + 1));

                    (currentNumbers[i],
                     currentNumbers[j]) =
                     (currentNumbers[j],
                      currentNumbers[i]);
                }

                RunOnUiThread(UpdateNumbers);

                if (currentNumbers[0] == 1 ||
                    currentNumbers[0] == 2 ||
                    currentNumbers[0] == 3)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(
                            this,
                            "تم العثور على الرقم " +
                            currentNumbers[0],
                            ToastLength.Long).Show();
                    });

                    isShuffling = false;

                    currentNumbers =
                        new List<int>(originalNumbers);

                    RunOnUiThread(UpdateNumbers);

                    break;
                }

                await System.Threading.Tasks.Task.Delay(
                    100,
                    token);
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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        try
        {
            if (receiver != null)
            {
                UnregisterReceiver(receiver);
            }
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
                        new System.Threading
                        .CancellationTokenSource();

                    activity.StartShuffle(
                        activity.cancellationToken.Token);
                }
            }

            if (intent.Action == "STOP_SHUFFLING")
            {
                activity.isShuffling = false;

                activity.cancellationToken?.Cancel();

                activity.currentNumbers =
                    new List<int>(
                        activity.originalNumbers);

                activity.RunOnUiThread(
                    activity.UpdateNumbers);
            }
        }
    }
}
