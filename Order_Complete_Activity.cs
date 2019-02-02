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
using Java.Util;
using Android.Support.V4.App;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Load.Resource.Drawable;
using Refractored.Controls;
using Android.Webkit;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/ThemeGreen")]
    public class Order_Complete_Activity : AppCompatActivity
    {
        private string _lang;
        private ProgressBar mProgressbar;

        public string Thumbnail { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            _lang = app.GetString("Language", "en");
            Thumbnail = app.GetString("Payment", string.Empty);
            ChangeLanguage(_lang);

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.order_done);

            Button mFAQ = FindViewById<Button>(Resource.Id.btnfaq);
            Button mInfo = FindViewById<Button>(Resource.Id.btnInfo);
            ImageView mPayInfo = FindViewById<ImageView>(Resource.Id.imageView);
            Button mMore = FindViewById<Button>(Resource.Id.btnReadMore);
            mMore.Click += MMore_Click;
            Glide.With(this).Load(Thumbnail)
            .Apply(RequestOptions.OverrideOf(400, 400).FitCenter())
            .Apply(RequestOptions.PlaceholderOf(Resource.Drawable.placeholder).FitCenter())
            .Transition(DrawableTransitionOptions.WithCrossFade())
            .Into(mPayInfo);

            mFAQ.Click += (o, s) =>
            {
                FAQ();
            };
            mInfo.Click += (o, s) =>
            {
                Information();
            };

            CircleImageView mPesa = FindViewById<CircleImageView>(Resource.Id.mpesa);
            CircleImageView mTigoPesa = FindViewById<CircleImageView>(Resource.Id.tigopesa);
            CircleImageView mHalopesa = FindViewById<CircleImageView>(Resource.Id.halopesa);
            CircleImageView mAirtel = FindViewById<CircleImageView>(Resource.Id.airtelmoney);
            CircleImageView mTpesa = FindViewById<CircleImageView>(Resource.Id.tpesa);
            CircleImageView mZantel = FindViewById<CircleImageView>(Resource.Id.zantel);

            Glide.With(this).Load(Resource.Drawable.mpesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mPesa);
            Glide.With(this).Load(Resource.Drawable.tigopesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mTigoPesa);
            Glide.With(this).Load(Resource.Drawable.halopesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mHalopesa);
            Glide.With(this).Load(Resource.Drawable.airtelmoney).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mAirtel);
            Glide.With(this).Load(Resource.Drawable.tpesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mTpesa);
            Glide.With(this).Load(Resource.Drawable.easymoney).Apply(RequestOptions.OverrideOf(90, 150).CenterInside()).Into(mZantel);

            mPesa.Click += (o, s) =>
            {
                string telephone = "*150*00#";
                var phone = new Intent(Intent.ActionView,
                Android.Net.Uri.Parse(string.Format("tel:{0}", telephone)));
                StartActivity(phone);

            };
            mTigoPesa.Click += (o, s) =>
            {
                string telephone = "*150*00#";
                var phone = new Intent(Intent.ActionView,
                Android.Net.Uri.Parse(string.Format("tel:{0}", telephone)));
                StartActivity(phone);

            };
            mHalopesa.Click += (o, s) =>
            {
                string telephone = "*150*00#";
                var phone = new Intent(Intent.ActionView,
                Android.Net.Uri.Parse(string.Format("tel:{0}", telephone)));
                StartActivity(phone);

            };
            mAirtel.Click += (o, s) =>
            {
                string telephone = "*150*00#";
                var phone = new Intent(Intent.ActionView,
                Android.Net.Uri.Parse(string.Format("tel:{0}", telephone)));
                StartActivity(phone);
            };
            mTpesa.Click += (o, s) =>
            {
                string telephone = "*150*00#";
                var phone = new Intent(Intent.ActionView,
                Android.Net.Uri.Parse(string.Format("tel:{0}", telephone)));
                StartActivity(phone);
            };
            mZantel.Click += (o, s) =>
            {
                string telephone = "*150*00#";
                var phone = new Intent(Intent.ActionView,
                Android.Net.Uri.Parse(string.Format("tel:{0}", telephone)));
                StartActivity(phone);
            };

        }

        private void MMore_Click(object sender, EventArgs e)
        {
            FAQ();
        }

        private void StartIntentForBuy()
        {
            string telephone = "*150*00#";
            // set the voice call action & destinationnumber
            Intent phone = new Intent(Intent.ActionView,
             Android.Net.Uri.Parse(string.Format("tel:{0}", telephone)));
            StartActivity(phone);

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

        private void FAQ()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(Faq_Activity));
            intent.PutExtra("Back", true);
            this.StartActivity(intent, option.ToBundle());
        }

        private void Information()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(Information_Activity));
            intent.PutExtra("Back", true);

            this.StartActivity(intent, option.ToBundle());
        }

        public override void OnBackPressed()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(MainActivity));
            this.StartActivity(intent, option.ToBundle());

        }
    }
}