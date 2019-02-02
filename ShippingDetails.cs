using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.App;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Java.Util;
using Safari_Shopping_Mall.Accessors;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Safari_Shopping_Mall.Fragments;
using Safari_Shopping_Mall.Adapters;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", NoHistory = true, Theme = "@style/AppTheme")]
    public class ShippingDetails : AppCompatActivity,View.IOnTouchListener
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private string _lang;
        private ViewPager viewPager;

        public User LastPurchases { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.shipping);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            SetSupportActionBar(mToolbar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = " ";
            TabLayout tabs = FindViewById<TabLayout>(Resource.Id.tabs);
            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);

            viewPager.SetOnTouchListener(this);
            SetUpViewPager(viewPager);

            tabs.SetupWithViewPager(viewPager);

            try
            {
                viewPager.BeginFakeDrag();

                var tabstrip = ((ViewGroup)tabs.GetChildAt(0));
                for (int i = 0; i < tabstrip.ChildCount; i++)
                {
                    ViewGroup vgTab = (ViewGroup)tabstrip.GetChildAt(i);
                    vgTab.Enabled = false;
                }

            }
            catch (Exception)
            {
            }
        }

        private void SetUpViewPager(ViewPager viewPager)
        {
            TabAdapter adapter = new TabAdapter(SupportFragmentManager);
            adapter.AddFragment(new CustomerDetailsFrag(), Resources.GetString(Resource.String.title_customer));
            adapter.AddFragment(new PaymentsFrag(), Resources.GetString(Resource.String.title_make_payment));
            viewPager.Adapter = adapter;
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

        protected override void OnResume()
        {
            base.OnResume();

            var isBack = Intent.GetBooleanExtra("Back", false);
            if (isBack)
            {
                viewPager.SetCurrentItem(1, false);
            }
        }

        public void OnPageScrollStateChanged(int state)
        {
            //throw new NotImplementedException();
        }

        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            //throw new NotImplementedException();
        }

        public void OnPageSelected(int position)
        {
            //throw new NotImplementedException();
            viewPager.SetCurrentItem(viewPager.CurrentItem,false);
        }

        public override void OnBackPressed()
        {
            //ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            //Intent intent = new Intent(this, typeof(MainActivity));
            //this.StartActivity(intent, option.ToBundle());
            Finish();
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            return false;
        }
    }
}