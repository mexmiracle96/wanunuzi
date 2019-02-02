using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.App;
using Android.Transitions;
using Safari_Shopping_Mall.Fragments;
using Java.Util;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Firebase.Messaging;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", MainLauncher = true, Theme = "@style/MyTheme.Splash", NoHistory = true)]
    public class LoginActivity : AppCompatActivity
    {
        ISharedPreferences User = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private string _lang;

        public string UserID { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var editor = User.Edit();
            editor.PutString("Language", "sw");
            editor.Apply();

            _lang = User.GetString("Language", "en");
            ChangeLanguage(_lang);

            UserID = User.GetString("USERID", String.Empty);


            Thread.Sleep(TimeSpan.FromSeconds(10).Seconds);
            SetTheme(Resource.Style.StartTheme);
            base.OnCreate(savedInstanceState);
        }

        private void ChangeLanguage(string language)
        {
            var res = this.Resources;
            var Dm = res.DisplayMetrics;
            var Config = res.Configuration;
            if (language != null)
            {
                Config.SetLocale(new Locale(language));
            }
            else
            {
                Config.SetLocale(new Locale("en"));
            }

            res.UpdateConfiguration(Config, Dm);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!string.IsNullOrEmpty(UserID))
            {
                Intent i = new Intent(this, typeof(MainActivity));
                this.StartActivity(i);
            }
            else
            {
                this.SetTheme(Resource.Style.AppTheme);

                SetContentView(Resource.Layout.login_sign_activity);
                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(Resource.Id.fragmentContainer, new sign_login(), "sign_login");
                trans.DisallowAddToBackStack();
                trans.Commit();

            }
        }

    }
}