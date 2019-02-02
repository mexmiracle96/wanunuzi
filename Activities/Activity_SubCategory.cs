using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Safari_Shopping_Mall.Adapters;
using Android.Support.V4.App;
using Safari_Shopping_Mall.Accessors;
using Android.Support.V4.View;
using Java.Util;
using Safari_Shopping_Mall.Helpers;
using Firebase.Database;
using System;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class Activity_SubCategory : AppCompatActivity,Transaction.IHandler
    {
        #region Variables Declaration
        private string Selected;
        private string CurrentView;
        RecyclerView recyclerView;
        private TextView[] mDots;
        private ViewPager mSlideViewpager;
        private LinearLayout mDotsLayout;
        private TopProducts_Adapter sliderAdapter;
        private int CurrentPage;
        private System.Timers.Timer t;

        private Android.Support.V7.Widget.Toolbar mToolbar;
        private List<Products> img;
        private int cart_count;
        private string _lang;
        private TopProducts_Adapter mTopProductsAdapter;
        private SubCategory_Adapters mCatAdapter;
        private TextView textCartItemCount;
        private int mCartItemCount;
        private SqlLiteSession Broker = new SqlLiteSession();
        public string title { get; private set; }

        #endregion
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private ProgressBar mProgress;
        private ImageView _CartItemCount;
        private Android.Support.V7.Widget.SearchView _searchView;
        private List<SubCategories> objCategories;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);
            Selected = Intent.GetStringExtra("CategoryID");

            CountWatched();

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.subcategory_activity);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            mProgress = FindViewById<ProgressBar>(Resource.Id.loading_spinner);
            setUpToolbar();


            if(_lang == "sw")
            {
                SupportActionBar.Title = Intent.GetStringExtra("Translation");
            }
            else
            {
                SupportActionBar.Title = Intent.GetStringExtra("Category");
            }

            InitCategoriesRecyclerview(recyclerView);
            //GetTopProducts();
            GetCategories();
        }

        private void setUpToolbar()
        {
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            mToolbar.InflateMenu(Resource.Menu.menu_shopping);
            SetSupportActionBar(mToolbar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            //sSupportActionBar.Title = title;

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_shopping_cart, menu);
            var item = menu.FindItem(Resource.Id.action_cart);

            View actionView = MenuItemCompat.GetActionView(item);
            _CartItemCount = actionView.FindViewById<ImageView>(Resource.Id.cart_badge);

            mCartItemCount = Broker.GetCart().Rows.Count;
            if (mCartItemCount == 0)
            {
                _CartItemCount.Visibility = ViewStates.Gone;
            }
            else
            {
                _CartItemCount.Visibility = ViewStates.Visible;
            }
            actionView.Click += (o, e) =>
            {
                ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                Intent i = new Intent(this, typeof(MyCart_Activity));
                StartActivity(i, option.ToBundle());
            };

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.action_cart:
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    Intent i = new Intent(this, typeof(MyCart_Activity));
                    StartActivity(i, option.ToBundle());
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
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

        private void GetCategories()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;

            FirebaseCallback p = new FirebaseCallback();
            p.GetSubCategories(Selected, database);
            p.SubCategories += (sender, obj) =>
            {
                objCategories = obj;
                mProgress.Visibility = ViewStates.Gone;
                mCatAdapter = new SubCategory_Adapters(ApplicationContext, obj);
                recyclerView.SetAdapter(mCatAdapter);
                mCatAdapter.ItemClick += (s, o) =>
                {
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    Intent intent = new Intent(this, typeof(ShoppingActivity));
                    intent.PutExtra("CategoryID", mCatAdapter.category[o].CategoryID);
                    intent.PutExtra("Category", mCatAdapter.category[o].SubCategory);
                    intent.PutExtra("Translation", mCatAdapter.category[o].Translation);

                    intent.PutExtra("SubCategory", mCatAdapter.category[o].SubCategoryID);
                    StartActivity(intent, option.ToBundle());
                };
            };
        }

        private void InitCategoriesRecyclerview(RecyclerView _recyclerView)
        {
            //Create our layout manager
            SimpleItemDecorator itemDecor = new SimpleItemDecorator(this);
            _recyclerView.AddItemDecoration(itemDecor);

            var layoutManager = new GridLayoutManager(this, 2, 1, false);
            _recyclerView.SetLayoutManager(layoutManager);

        }

        protected override void OnResume()
        {
            base.OnResume();
            InvalidateOptionsMenu();
        }

        private void SearchCategory(string query)
        {
            var result = objCategories.Where(o => o.SubCategory.Contains(query)).ToList();

             mCatAdapter= new SubCategory_Adapters(this, result);
            recyclerView.SetAdapter(mCatAdapter);
        }

        public void CountWatched()
        {
            FirebaseDatabase db = FirebaseDatabase.Instance;
            DatabaseReference myRef = db.GetReference("Categories/" + Selected);
            myRef.RunTransaction(this);
        }

        public Transaction.Result DoTransaction(MutableData currentData)
        {
            if (currentData.Value != null)
            {
                var counts = currentData.Child("Watched").Value.ToString();

                var newWatched = Int32.Parse(counts);
                currentData.Child("Watched").Value = newWatched + 1;

                return Transaction.Success(currentData);
            }
            else
            {
                currentData.Value = null;
                return Transaction.Success(currentData);
            }

        }

        public void OnComplete(DatabaseError error, bool committed, DataSnapshot currentData)
        {
            ///throw new NotImplementedException();
        }

    }
}