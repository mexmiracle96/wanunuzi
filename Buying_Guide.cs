
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Support.V4.View;
using Safari_Shopping_Mall.Adapters;
using Android.Transitions;
using Java.Util;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.Widget;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Buying_Guide" , Theme = "@style/AppTheme1")]
    public class Buying_Guide : AppCompatActivity
    {
        private ViewPager mSlideViewpager;
        private LinearLayout mDotsLayout;
        private TextView[] mDots;
        private Button mNextBtn;
        private Button mPrevBtn;
        private int CurrentPage;

        private ProgressbarSetup progressbar;
        SqlLiteSession S;
        private string _lang;
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private RecyclerView mRecyclerview;

        public string UserID { get; private set; }
        public string Qty { get; private set; }
        public string ProductID { get; private set; }
        public string Sizes { get; private set; }
        public string PayNo { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            UserID = app.GetString("USERID", string.Empty);
            PayNo = app.GetString("PhoneNumber", string.Empty);

            _lang = "sw";
            ChangeLanguage(_lang);

            base.OnCreate(savedInstanceState);
            Slide slide = new Slide(GravityFlags.Right);
            Slide slideout = new Slide(GravityFlags.Left);
            UserID = app.GetString("USERID", string.Empty);

            // Create your application here
            SetContentView(Resource.Layout.slide_layout);
            mNextBtn = FindViewById<Button>(Resource.Id.btnNext);

            mNextBtn.Click += (o, sender) =>
            {

            };
 
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
    }
}