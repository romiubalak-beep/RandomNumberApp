using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Views.Accessibility;
using Android.Widget;
using System.Collections.Generic;

namespace RandomNumberApp
{
    [Activity(Label = "FloatBar Settings", MainLauncher = true)]
    public class MainActivity : PreferenceActivity
    {
        private CheckBoxPreference startCheckBox;
        private Preference sendMail, returnDefault;
        private Prefs prefs;
        private bool isEnabled = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.prefs_list_content);
            AddPreferencesFromResource(Resource.Xml.float_preference);

            prefs = new Prefs(this);

            startCheckBox = (CheckBoxPreference)FindPreference("started");
            startCheckBox.PreferenceChange += (s, e) =>
            {
                Intent intent = new Intent(this, typeof(FloatBarService));
                StartService(intent);
            };

            sendMail = FindPreference("mail");
            sendMail.PreferenceClick += (s, e) =>
            {
                // إرسال بريد إلكتروني
                Intent emailIntent = new Intent(Intent.ActionSend);
                emailIntent.PutExtra(Intent.ExtraEmail, new string[] { "your-email@example.com" });
                emailIntent.PutExtra(Intent.ExtraSubject, "FloatBar Feedback");
                emailIntent.SetType("message/rfc822");
                StartActivity(Intent.CreateChooser(emailIntent, "Choose Email Client"));
            };

            returnDefault = FindPreference("returnDefaultSetting");
            returnDefault.PreferenceClick += (s, e) =>
            {
                ShowDialog(this, "استعادة الإعدادات الافتراضية", 
                          "سيتم استعادة جميع الإعدادات إلى القيم الافتراضية. هل أنت متأكد؟",
                          "نعم", "لا");
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckAccessibilityService();
        }

        private void CheckAccessibilityService()
        {
            AccessibilityManager manager = (AccessibilityManager)GetSystemService(AccessibilityService);
            var list = manager.GetEnabledAccessibilityServiceList(Android.AccessibilityServices.AccessibilityServiceInfo.FeedbackAllMask);
            
            isEnabled = false;
            foreach (var info in list)
            {
                if (info.Id == "com.example.randomapp/.FloatBarService")
                {
                    isEnabled = true;
                    break;
                }
            }

            if (!isEnabled)
            {
                ShowDialog(this, "تفعيل الخدمة", 
                          "لم يتم تفعيل خدمة إمكانية الوصول. يرجى تفعيلها من الإعدادات.",
                          "اذهب إلى الإعدادات", "إلغاء");
            }
        }

        private void ShowDialog(Context context, string title, string message, string positiveText, string negativeText)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton(positiveText, (s, e) =>
            {
                if (isEnabled)
                {
                    prefs.ClearPrefs();
                    Finish();
                    StartService(new Intent(this, typeof(FloatBarService)));
                    StartActivity(new Intent(this, typeof(MainActivity)));
                }
                else
                {
                    Intent intent = new Intent("android.settings.ACCESSIBILITY_SETTINGS");
                    StartActivity(intent);
                }
            });
            builder.SetNegativeButton(negativeText, (s, e) => { });
            builder.SetCancelable(false);
            builder.Show();
        }
    }
}
