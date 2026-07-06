namespace RandomNumberApp;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

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

        // ✅ إضافة Toast بعد RegisterReceiver
        Toast.MakeText(
            this,
            "Receiver Registered",
            ToastLength.Long).Show();
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

    private async void StartShuffle(
        System.Threading.CancellationToken token)
    {
        RunOnUiThread(() =>
        {
            Toast.MakeText(
                this,
                "StartShuffle ENTERED",
                ToastLength.Long).Show();
        });

        try
        {
            while (isShuffling &&
                   !token.IsCancellationRequested)
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(
                        this,
                        "Loop Running",
                        ToastLength.Short).Show();
                });

                int n = currentNumbers.Count;

                for (int i = n - 1; i > 0; i--)
                {
                    byte[] bytes = new byte[4];

                    rng!.GetBytes(bytes);

                    int j = Math.Abs(
                        BitConverter.ToInt32(bytes, 0)
                        % (i + 1));

                    (currentNumbers[i],
                     currentNumbers[j]) =
                    (currentNumbers[j],
                     currentNumbers[i]);
                }

                RunOnUiThread(UpdateNumbers);

                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            RunOnUiThread(() =>
            {
                Toast.MakeText(
                    this,
                    ex.ToString(),
                    ToastLength.Long).Show();
            });
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

            Toast.MakeText(
                activity,
                "Received: " + intent.Action,
                ToastLength.Long).Show();

            if (intent.Action == "START_SHUFFLING")
            {
                Toast.MakeText(
                    activity,
                    "START received",
                    ToastLength.Long).Show();

                if (!activity.isShuffling)
                {
                    activity.isShuffling = true;

                    activity.cancellationToken =
                        new System.Threading.CancellationTokenSource();

                    activity.StartShuffle(
                        activity.cancellationToken.Token);
                }
            }

            if (intent.Action == "STOP_SHUFFLING")
            {
                Toast.MakeText(
                    activity,
                    "STOP received",
                    ToastLength.Long).Show();

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
