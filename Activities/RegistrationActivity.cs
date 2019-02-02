using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Safari_Shopping_Mall.Fragments;
using Java.Util;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", MainLauncher = false, Theme = "@style/StartTheme", NoHistory = true)]
    public class RegistrationActivity : AppCompatActivity
    {
        private string _lang;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Registration_Activity);
            var trans = SupportFragmentManager.BeginTransaction();
            trans.Replace(Resource.Id.fragmentContainer, new Registration_Page1(), "Registration_Page1");
            trans.DisallowAddToBackStack();
            trans.Commit();
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

    }
}