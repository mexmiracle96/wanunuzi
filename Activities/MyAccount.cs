
using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Safari_Shopping_Mall.Accessors;
using System.Net.Http;
using Android.Support.Design.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Transitions;
using Java.Util;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Firebase.Messaging;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class MyAccount : AppCompatActivity
    {
        private User _user;
        private EditText mFullname;
        private EditText mLastname;
        private EditText mPhone;
        private EditText mPhone2;
        private EditText mPhoneNo;
        private EditText mConfirPhoneNo;
        private EditText mLocation;
        private Button mSave;
        private ProgressbarSetup Progress;
        private List<User> U;
        private List<Results> R;
        private Android.Support.V7.Widget.Toolbar mToolbar;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private TextView mCircle;
        private TextView mUser;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);

            base.OnCreate(savedInstanceState);
            Explode fade = new Explode();
            Slide fade2 = new Slide(GravityFlags.Right);

            fade.ExcludeTarget(Resource.Id.toolBar, true);
            Window.EnterTransition = fade;
            Window.ExitTransition = fade2;

            // Create your application here
            SetContentView(Resource.Layout.my_account);
            mFullname= FindViewById<EditText>(Resource.Id.edtFullname);
            mPhoneNo = FindViewById<EditText>(Resource.Id.edtPhoneNumber);
            mConfirPhoneNo = FindViewById<EditText>(Resource.Id.edtConfPhoneNumber);
            mLocation = FindViewById<EditText>(Resource.Id.edtLocation);

            //Set Current Ifo
            mFullname.Text = app.GetString("Fullname", string.Empty);
            mPhoneNo.Text = app.GetString("PhoneNumber", string.Empty);

            mLocation.Text = app.GetString("MyLocation", string.Empty);

            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);

            Progress = new ProgressbarSetup();
        }

        private void UserSettingsValidation()
        {
            _user = new User();
            bool first = false, last = false, phone1 = false;

            if(!string.IsNullOrEmpty(mFullname.Text))
            {
                first = true;
            }

            if (!string.IsNullOrEmpty(mPhoneNo.Text))
            {
                last = true;
            }

            if (mPhoneNo.Text == mConfirPhoneNo.Text)
            {
                phone1 = true;
            }

            if(first && last && phone1)
            {
                _user.Fullname = mFullname.Text;
                _user.PhoneNumber = mPhoneNo.Text;
                _user.Location = mLocation.Text;
                _user.UserID = app.GetString("USERID", string.Empty);
                if (string.IsNullOrEmpty(mLocation.Text))
                {
                    _user.Location = "not set";
                }
                else
                {
                    _user.Location = mLocation.Text;
                }

                var builder = new Android.App.AlertDialog.Builder(this);
                builder.SetTitle(Resource.String.dlg_info);
                builder.SetMessage(Resource.String.dlg_save);
                builder.SetPositiveButton(Resource.String.dlg_btn_yes, delegate
                 {
                     builder.Dispose();
                     Update(_user);
                 });
                builder.SetNegativeButton(Resource.String.dlg_btn_no, delegate
                 { builder.Dispose(); });
                builder.Show();
            }
        }

        private void Update(User user)
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.UpdateUserAccount(database, user);
            SaveUserData(user);

            Snackbar.Make(FindViewById(Android.Resource.Id.Content), "Saved Completed! ",
            Snackbar.LengthShort).Show();
        }

        private void SaveUserData(User U)
        {
            ISharedPreferences pref = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();
            edit.PutString("Fullname", U.Fullname);
            edit.PutString("PhoneNumber", U.PhoneNumber);
            edit.PutString("MyLocation", U.Location);

            edit.PutString("USERID", U.UserID);
            edit.Apply();
            edit.Commit();

            FirebaseMessaging.Instance.UnsubscribeFromTopic(U.PhoneNumber);

        }

        private void InfoDialog()
        {
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetIcon(Android.Resource.Drawable.IcDialogInfo);
            builder.SetTitle(Resource.String.dlg_info);
            builder.SetMessage(Resource.String.dlg_update_success);
            builder.SetPositiveButton(Resource.String.dialog_ok, delegate
            {
                builder.Dispose();
            });
            builder.Create().Show();
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

        private class Results
        {
            public string Result;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.save_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.action_save:
                    UserSettingsValidation();
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}