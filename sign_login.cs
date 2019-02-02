using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Safari_Shopping_Mall.Activities;
using Java.Util;

namespace Safari_Shopping_Mall.Fragments
{
    public class sign_login : Android.Support.V4.App.Fragment
    {
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private Button mLogin;
        private Button mRegister;
        private ImageButton mLanguage;
        private TextView mTxtLang;
        private string selected;
        private string _lang;

        public override void OnCreate(Bundle savedInstanceState)
        {
            _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);
            base.OnCreate(savedInstanceState);

            // Create your fragment here

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

            res.Configuration.UpdateFrom(Config);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.sign_account, container, false);
            mLogin = view.FindViewById<Button>(Resource.Id.btnSignin);
            mRegister = view.FindViewById<Button>(Resource.Id.btnCreateAccount);
            mLanguage = view.FindViewById<ImageButton>(Resource.Id.btnLanguage);
            mTxtLang = view.FindViewById<TextView>(Resource.Id.txtLanguage);

            mLogin.Click += (o, s) =>
            {
                var trans = FragmentManager.BeginTransaction();
                trans.Replace(Resource.Id.fragmentContainer, new Login(), "login");
                trans.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_up);
                trans.DisallowAddToBackStack();
                trans.Commit();
            };
            mRegister.Click += (o, s) =>
            {
                Intent i = new Intent(Context, typeof(RegistrationActivity));
                StartActivity(i);
            };
            mLanguage.Click += (o, s) =>
            {
                settings();
            };

            if (_lang == "en")
            {
                mTxtLang.Text = "En";
            }
            else
            {
                mTxtLang.Text = "Sw";
            }
            return view;
        }

        private void settings()
        {
            var check = 0;
            if (_lang == "sw")
            {
                check = 1;
            }

            var builder = new Android.App.AlertDialog.Builder(Activity);
            builder.SetTitle(Resource.String.dlg_choose_lang);
            builder.SetSingleChoiceItems(Resource.Array.language_choice, check, delegate (object sender, DialogClickEventArgs e)
            {
                builder.Dispose();
                var language = Resources.GetStringArray(Resource.Array.language_choice);
                selected = language[e.Which];

            });
            builder.SetPositiveButton(Resource.String.dialog_ok, delegate
            {
                builder.Dispose();
                if (selected == "Kiswahili")
                {
                    ISharedPreferencesEditor editor = app.Edit();
                    editor.PutString("Language", "sw");
                    editor.Apply();
                    Activity.Recreate();
                }
                else
                {
                    ISharedPreferencesEditor editor = app.Edit();
                    editor.PutString("Language", "en");
                    editor.Apply();
                    Activity.Recreate();

                }
            });
            builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
            {
                builder.Dispose();
            });
            builder.Create().Show();
        }

    }
}