using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Safari_Shopping_Mall.Adapters;
using Safari_Shopping_Mall.Accessors;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Transitions;
using Android.Views.Animations;
using Safari_Shopping_Mall.Helpers;
using Java.Util;
using Android.Support.V4.App;
using Android.Support.V7.Widget.Helper;
using Firebase.Database;
using Android.Support.V4.View;
using Safari_Shopping_Mall.Fragments;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme2")]
    public class FavActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        public string UserID { get; private set; }
        private string _lang;
        private string mPhoneNumber;
        private TextView textCartItemCount;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private int mCartItemCount;
        private ImageView _CartItemCount;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            UserID = app.GetString("USERID", string.Empty);
            mPhoneNumber = app.GetString("PhoneNumber", string.Empty);

            _lang = app.GetString("Language", "en");

            ChangeLanguage(_lang);
            Slide slide = new Slide(GravityFlags.Top);
            slide.ExcludeTarget(Resource.Id.toolbar, true);
            Explode explode = new Explode();

            base.OnCreate(savedInstanceState);


            Window.EnterTransition = slide;
            Window.ExitTransition = explode;

            // Create your application here
            SetContentView(Resource.Layout.my_purchases);

            // Create your application here
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);


            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "";

            TabLayout tabs = FindViewById<TabLayout>(Resource.Id.tabs);

            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);

            SetUpViewPager(viewPager);

            tabs.SetupWithViewPager(viewPager);

        }

        private void SetUpViewPager(ViewPager viewPager)
        {
            TabAdapter adapter = new TabAdapter(SupportFragmentManager);
            adapter.AddFragment(new Finished_Purchases(), Resources.GetString(Resource.String.tab_finished_purchases));
            adapter.AddFragment(new Pending_Purchases(), Resources.GetString(Resource.String.tab_pending_purchases));

            viewPager.Adapter = adapter;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {

            //throw new NotImplementedException();
            return true;
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
            InvalidateOptionsMenu();
        }

    }
}