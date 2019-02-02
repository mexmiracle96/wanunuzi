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
using Android.Support.Design.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Load.Resource.Drawable;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class CompanyInfo : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar mToolBar;
        private TextView mCompany;
        private TextView mLocation;
        private TextView mItems;
        private TextView mPhoneNo1;
        private TextView mPhoneNo2;
        private TextView mEmail;
        private ImageView mImageThumb;
        private FloatingActionButton mFab;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.company_info);
            mToolBar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            mFab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            mCompany = FindViewById<TextView>(Resource.Id.txtSeller);
            mPhoneNo1 = FindViewById<TextView>(Resource.Id.txtPhone1);
            mPhoneNo2 = FindViewById<TextView>(Resource.Id.txtPhone2);
            mEmail = FindViewById<TextView>(Resource.Id.txtEmail);
            mLocation = FindViewById<TextView>(Resource.Id.txtLocation);
            mImageThumb = FindViewById<ImageView>(Resource.Id.backdrop);
            SetSupportActionBar(mToolBar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);

            GetCompanyInfo();
        }

        private void GetCompanyInfo()
        {
            ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            var company = app.GetString("Company", string.Empty);
            var location = app.GetString("Location", string.Empty);
            var phone1 = app.GetString("Phone1", string.Empty);
            var phone2 = app.GetString("Phone2", string.Empty);
            var email = app.GetString("Email", string.Empty);
            var thumbnail = app.GetString("Thumbnail", string.Empty);

            mCompany.Text = company;
            mLocation.Text = location;
            mPhoneNo1.Text = phone1;
            mPhoneNo2.Text = phone2;
            mEmail.Text = email;

            Glide.With(this).Load(thumbnail)
            .Apply(RequestOptions.OverrideOf(600, 600).FitCenter())
            .Apply(RequestOptions.PlaceholderOf(Resource.Drawable.placeholder).FitCenter())
            .Transition(DrawableTransitionOptions.WithCrossFade())
            .Into(mImageThumb);

            mFab.Click += (o, s) =>
            {
                CallUs(phone1, phone2);
            };
        }

        private void CallUs(string p1, string p2)
        {
            string[] nos = new string[]
            {
                p1,
                p2
            };
            var phone = "";
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.menu_contact_us);
            builder.SetSingleChoiceItems(nos, 0, delegate (object sender, DialogClickEventArgs e)
            {
                phone = nos[e.Which];
            });
            builder.SetPositiveButton(Resource.String.btn_call, delegate
            {
                builder.Dispose();
                if (!string.IsNullOrEmpty(phone))
                    phone = nos[0];
                Intent _phone = new Intent(Intent.ActionCall,
                Android.Net.Uri.Parse(string.Format("tel:{0}", phone)));
                StartActivity(_phone);
            });
            builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
            { builder.Dispose(); });
            builder.Show();
        }

        private void MakeACall(string p,string p2)
        {
            string[] nos = new string[]
            {
               p,
               p2
            };
            var phone = "";
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle("Wasiliana nasi");
            builder.SetSingleChoiceItems(nos, 0, delegate (object sender, DialogClickEventArgs e)
            {
                phone = nos[e.Which];
            });
            builder.SetPositiveButton("Piga", delegate
            {
                builder.Dispose();
                if (!string.IsNullOrEmpty(phone))
                    phone = nos[0];
                Intent _phone = new Intent(Intent.ActionCall,
                Android.Net.Uri.Parse(string.Format("tel:{0}", phone)));
                StartActivity(_phone);
            });
            builder.SetNegativeButton("Tengua", delegate
            { builder.Dispose(); });
            builder.Show();

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}