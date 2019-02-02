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
using System.Net.Http;
using Safari_Shopping_Mall.Accessors;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Android.Support.V4.App;
using Firebase.Messaging;
using Firebase.Iid;

namespace Safari_Shopping_Mall.Fragments
{
    public class Login : Android.Support.V4.App.Fragment
    {
        private EditText Username;
        private EditText MobileEmail;
        private Button btnLogin;
        private ProgressDialog Progress;
        private List<User> U;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.login, container, false);

            MobileEmail = view.FindViewById<EditText>(Resource.Id.edtEmailMobile);
            btnLogin = view.FindViewById<Button>(Resource.Id.btnSignin);

            Progress = new ProgressDialog(Activity);

            Progress.SetMessage(Resources.GetString(Resource.String.prgdlg));
            btnLogin.Click += (obj, sender) =>
            {
                Progress.Show();
                if(!string.IsNullOrEmpty(MobileEmail.Text))
                    LoginUser(MobileEmail.Text);
            };
            return view;
        }

        private bool ErrorDialog()
        {
            var ret = false;
            var alert = new AlertDialog.Builder(Context);
            alert.SetTitle(Resource.String.title_info);
            alert.SetMessage(Resource.String.request_timeout);
            alert.SetPositiveButton(Resource.String.dialog_retry, delegate
            {
                ret = true;
                alert.Dispose();
            });
            alert.SetPositiveButton(Resource.String.dialog_cancel, delegate
            {
                alert.Dispose();
            });
            alert.Show();
            return ret;
        }

        private void SaveUserData(User U)
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            ISharedPreferences pref = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();
            edit.PutString("Fullname", U.Fullname);
            edit.PutString("PhoneNumber", U.PhoneNumber);
            edit.PutString("USERID", U.UserID);
            edit.PutString("MyLocation", string.Empty);

            edit.Apply();
        }

        private void LoginFailAlertDialog()
        {
            var alert = new AlertDialog.Builder(Context);
            alert.SetTitle(Resource.String.title_info);
            alert.SetMessage(Resource.String.dlg_login_fail);
            alert.SetPositiveButton(Resource.String.dialog_ok, delegate
            {
                alert.Dispose();
            });
            alert.Show();
        }

        private void LoginUser(string phoneNumber)
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.LoginUserAsync(database, phoneNumber);
            p._Auth += (sender, obj) =>
            {
                Progress.Hide();
                if (!string.IsNullOrEmpty(obj.UserID))
                {

                    SaveUserData(obj);
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(Activity);
                    Intent intent = new Intent(Activity, typeof(MainActivity));
                    StartActivity(intent);
                }
                else
                {
                    LoginFailAlertDialog();
                }
            };
        }

    }
}