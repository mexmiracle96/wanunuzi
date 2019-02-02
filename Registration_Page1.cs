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
using Safari_Shopping_Mall.Accessors;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using System.Threading.Tasks;
using System.Threading;
using Firebase.Messaging;

namespace Safari_Shopping_Mall.Fragments
{
    public class Registration_Page1 : Android.Support.V4.App.Fragment
    {
        private EditText fullname;
        private EditText phone;
        private EditText confirm_phone;
        private Button Register;
        private ProgressDialog Pg;

        public string UserID { get; private set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.registration, container, false);
            Register = view.FindViewById<Button>(Resource.Id.btnRegister);
            fullname = view.FindViewById<EditText>(Resource.Id.edtFirstName);
            phone = view.FindViewById<EditText>(Resource.Id.editPhone);
            confirm_phone = view.FindViewById<EditText>(Resource.Id.editConfirm);

            Register.Click += (obj, sender) =>
            {
                if(!string.IsNullOrEmpty(fullname.Text) && phone.Text == confirm_phone.Text)
                {
                    Pg = new ProgressDialog(Activity);
                    Pg.SetMessage(Resources.GetString(Resource.String.prgdlg));
                    Pg.Show();
                    User U = new User
                    {
                        UserID = Guid.NewGuid().ToString(),
                        Fullname = fullname.Text,
                        PhoneNumber = phone.Text
                    };
                    CreateAccount(U);
                }  
            };
            return view;
        }

        private void CreateAccount(User S)
        {
            var progress = new ProgressDialog(Activity);
            progress.SetMessage(Resources.GetString(Resource.String.tilte_connecting));
            progress.SetCanceledOnTouchOutside(false);
            progress.Show();

            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            FirebaseCallback u = new FirebaseCallback();
            u.LoginUserAsync(database, S.PhoneNumber);
            u._Auth += (o, data) =>
            {
                if(!string.IsNullOrEmpty(data.PhoneNumber))
                {
                    progress.Hide();
                    RegFailAlertDialog();
                }
                else
                {
                    p.CreateUserAsync(database, S);
                    p._Auth += (sender, obj) =>
                    {
                        progress.Hide();
                        if (!string.IsNullOrEmpty(obj.UserID))
                        {
                            SaveUserData(obj);
                            Intent I = new Intent(Context, typeof(MainActivity));
                            StartActivity(I);
                        }
                    };

                }
            };
        }

        private void SaveUserData(User U)
        {
            ISharedPreferences pref = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();
            edit.PutString("Fullname", U.Fullname);
            edit.PutString("PhoneNumber", U.PhoneNumber);
            edit.PutString("USERID", U.UserID);
            edit.PutString("MyLocation", string.Empty);
            edit.Apply();

            FirebaseMessaging.Instance.SubscribeToTopic(U.PhoneNumber);

        }

        private void RegFailAlertDialog()
        {
            var alert = new AlertDialog.Builder(Context);
            alert.SetTitle(Resource.String.title_info);
            alert.SetMessage(Resource.String.dlg_regstration_fail);
            alert.SetPositiveButton(Resource.String.dialog_ok, delegate
            {
                alert.Dispose();
            });
            alert.Show();
        }
    }
}