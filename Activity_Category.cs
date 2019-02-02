using System;
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
using Android.Support.V4.View;
using Safari_Shopping_Mall.Accessors;
using Android.Transitions;
using Java.Util;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class Activit_Category : AppCompatActivity
    {
        #region Variable Declaration
        private RecyclerView recyclerView;
        private Category_Adapter Adapter;
        private int choice;
        private string[] items;
        private List<Int32> images;
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private TextView[] mDots;
        private ViewPager mSlideViewpager;
        private LinearLayout mDotsLayout;
        private TopProducts_Adapter sliderAdapter;
        private int CurrentPage;
        private System.Timers.Timer t;
        private List<Products> img;
        private string _lang;
        private ImageView _CartItemCount;

        #endregion
        private SqlLiteSession Broker = new SqlLiteSession();
        private int mCartItemCount;
        private ProgressBar progressBar;
        private List<Categories> objCategories;
        private Android.Support.V7.Widget.SearchView _searchView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
            _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);

            base.OnCreate(savedInstanceState);

            Slide slide = new Slide(GravityFlags.Right);
            slide.ExcludeTarget(Resource.Id.toolBar, true);
            Window.EnterTransition = slide;

            // Create your application here
            SetContentView(Resource.Layout.subcategory_activity);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            progressBar = FindViewById<ProgressBar>(Resource.Id.loading_spinner);
            setUpToolbar();

            RecylerUpdateView(recyclerView);

            GetCategories();
        }

        private void setUpToolbar()
        {
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            mToolbar.InflateMenu(Resource.Menu.menu_shopping);
            SetSupportActionBar(mToolbar);

            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.menu_categories);

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

        private void RecylerUpdateView(RecyclerView _recyclerView)
        {
            //Create our layout manager
            SimpleItemDecorator itemDecor = new SimpleItemDecorator(this);
            _recyclerView.AddItemDecoration(itemDecor);

            _recyclerView.SetLayoutManager(new GridLayoutManager(this, 2, 1, false));
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
            p.GetCategoriesAll(database);
            p.Categories += (sender, obj) =>
            {
                progressBar.Visibility = ViewStates.Invisible;

                objCategories = obj; 
                Adapter = new Category_Adapter(this, obj);
                recyclerView.SetAdapter(Adapter);
                Adapter.ItemClick += (s, o) =>
                {
                    p.Dispose();
                    database.Dispose();

                    Intent intent = new Intent(this, typeof(Activity_SubCategory));
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    intent.PutExtra("CategoryID", Adapter.category[o].CategoryID);
                    intent.PutExtra("Category", Adapter.category[o].Category);
                    intent.PutExtra("Translation", Adapter.category[o].Translation);

                    StartActivity(intent, option.ToBundle());

                };
            };
        }

        private void SearchCategory(string query)
        {
            var result = objCategories.Where(o => o.Category.Contains(query)).ToList();

            Adapter = new Category_Adapter(this, result);
            recyclerView.SetAdapter(Adapter);
        }

    }
}