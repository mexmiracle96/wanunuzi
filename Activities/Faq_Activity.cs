using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.Support.V7.App;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V4.App;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class Faq_Activity : AppCompatActivity, IResize
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private WebView mWebViewer;
        private ProgressBar mProgressbar;
        private int overallProgress = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.frequently_asked_quest);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            mWebViewer = FindViewById<Android.Webkit.WebView>(Resource.Id.webView);
            mProgressbar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            
            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.menu_faq);

            // Makes Progress bar Visible

            var webclient = new MyClient(this);
            mWebViewer.SetWebViewClient(webclient);
            mWebViewer.ClearCache(true);

            mWebViewer.NestedScrollingEnabled = false;
            //mWebViewer.
            mWebViewer.Settings.JavaScriptEnabled = true;
            mWebViewer.LoadUrl("https://safari-shoppers.firebaseapp.com/faq.html");
            
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
                        base.Finish();
                    }
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void OnPageStartLoading()
        {
            //throw new NotImplementedException();
        }

        public override void OnBackPressed()
        {
            if (Intent.GetBooleanExtra("Back", false))
            {
                ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                Intent intent = new Intent(this, typeof(Order_Complete_Activity));
                this.StartActivity(intent, option.ToBundle());
                Finish();
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

    public interface IResize
    {
        void OnResize();
    }

    public class MyClient : WebViewClient
    {
        public IResize listener;
        public MyClient(IResize listener)
        {
            this.listener = listener;
        }
        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            listener.OnResize();

        }
    }

}
