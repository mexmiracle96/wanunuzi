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
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Webkit;
using Android.Support.V7.Widget;
using Android.Support.V4.App;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class Information_Activity : AppCompatActivity, IResize
    {
        private WebView mWebViewer;
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private ProgressBar mProgressbar;
        private int overallProgress = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.information);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            mWebViewer = FindViewById<Android.Webkit.WebView>(Resource.Id.webView);
            mProgressbar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.title_info);

            var webclient = new MyClient(this);
            mWebViewer.SetWebViewClient(webclient);
            mWebViewer.ClearCache(true);
            mWebViewer.Settings.JavaScriptEnabled = true;
            mWebViewer.LoadUrl("https://safari-shoppers.firebaseapp.com/info.html");

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (Intent.GetBooleanExtra("Back", false))
                    {
                        ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                        Intent intent = new Intent(this, typeof(Order_Complete_Activity));
                        this.StartActivity(intent, option.ToBundle());
                        Finish();
                    }
                    else
                    {
                        Finish();
                    }
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void OnPageStartLoading()
        {
        }

        public override void OnBackPressed()
        {
            if (Intent.GetBooleanExtra("Back", false))
            {
                ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                Intent intent = new Intent(this, typeof(Order_Complete_Activity));
                this.StartActivity(intent, option.ToBundle());

            }
            else
	        {
                base.OnBackPressed();
            }
        }

        public void OnResize()
        {
            mProgressbar.Visibility = ViewStates.Gone;
        }
    }
}