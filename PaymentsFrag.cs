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
using Android.Support.V4.App;
using Safari_Shopping_Mall.Activities;
using Java.Util;
using Refractored.Controls;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Load.Resource.Drawable;

namespace Safari_Shopping_Mall.Fragments
{
    public class PaymentsFrag : Android.Support.V4.App.Fragment
    {
        private string _lang;
        public string Thumbnail { get; private set; }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            _lang = app.GetString("Language", "en");
            Thumbnail = app.GetString("Payment", string.Empty);
            ChangeLanguage(_lang);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view  = inflater.Inflate(Resource.Layout.order_done, container, false);

            Button mFAQ = view.FindViewById<Button>(Resource.Id.btnfaq);
            Button mInfo = view.FindViewById<Button>(Resource.Id.btnInfo);
            ImageView mPayInfo = view.FindViewById<ImageView>(Resource.Id.imageView);
            Button mMore = view.FindViewById<Button>(Resource.Id.btnReadMore);
            mMore.Click += MMore_Click;

            mPayInfo.SetScaleType(ImageView.ScaleType.FitXy);

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

            CircleImageView mPesa = view.FindViewById<CircleImageView>(Resource.Id.mpesa);
            CircleImageView mTigoPesa = view.FindViewById<CircleImageView>(Resource.Id.tigopesa);
            CircleImageView mHalopesa = view.FindViewById<CircleImageView>(Resource.Id.halopesa);
            CircleImageView mAirtel = view.FindViewById<CircleImageView>(Resource.Id.airtelmoney);
            CircleImageView mTpesa = view.FindViewById<CircleImageView>(Resource.Id.tpesa);
            CircleImageView mZantel = view.FindViewById<CircleImageView>(Resource.Id.zantel);

            Glide.With(this).Load(Resource.Drawable.mpesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mPesa);
            Glide.With(this).Load(Resource.Drawable.tigopesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mTigoPesa);
            Glide.With(this).Load(Resource.Drawable.halopesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mHalopesa);
            Glide.With(this).Load(Resource.Drawable.airtelmoney).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mAirtel);
            Glide.With(this).Load(Resource.Drawable.tpesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mTpesa);
            Glide.With(this).Load(Resource.Drawable.easymoney).Apply(RequestOptions.OverrideOf(90, 150).CenterInside()).Into(mZantel);

            mPesa.Click += (o, s) =>
            {
                string telephone = "*150*00#";
                var phone = new Intent(Intent.ActionCall,
                Android.Net.Uri.Parse(string.Format("tel:{0}", Android.Net.Uri.Encode(telephone))));
                StartActivity(phone);

            };
            mTigoPesa.Click += (o, s) =>
            {
                string telephone = "*150*011#";
                var phone = new Intent(Intent.ActionCall,
                Android.Net.Uri.Parse(string.Format("tel:{0}", Android.Net.Uri.Encode(telephone))));
                StartActivity(phone);

            };
            mHalopesa.Click += (o, s) =>
            {
                string telephone = "*150*88#";
                var phone = new Intent(Intent.ActionCall,
                Android.Net.Uri.Parse(string.Format("tel:{0}", Android.Net.Uri.Encode(telephone))));
                StartActivity(phone);

            };
            mAirtel.Click += (o, s) =>
            {
                string telephone = "*150*60#";
                var phone = new Intent(Intent.ActionCall,
                Android.Net.Uri.Parse(string.Format("tel:{0}", Android.Net.Uri.Encode(telephone))));
                StartActivity(phone);
            };
            mTpesa.Click += (o, s) =>
            {
                string telephone = "*150*71#";
                var phone = new Intent(Intent.ActionCall,
                Android.Net.Uri.Parse(string.Format("tel:{0}", Android.Net.Uri.Encode(telephone))));
                StartActivity(phone);
            };
            mZantel.Click += (o, s) =>
            {
                string telephone = "*150*02#";
                var phone = new Intent(Intent.ActionCall,
                Android.Net.Uri.Parse(string.Format("tel:{0}", Android.Net.Uri.Encode(telephone))));
                StartActivity(phone);
            };

            return view;
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
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(Activity);
            Intent intent = new Intent(Activity, typeof(Faq_Activity));
            intent.PutExtra("Back", true);
            this.StartActivity(intent, option.ToBundle());
        }

        private void Information()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(Activity);
            Intent intent = new Intent(Activity, typeof(Information_Activity));
            intent.PutExtra("Back", true);

            this.StartActivity(intent, option.ToBundle());
        }

    }
}