using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Safari_Shopping_Mall.Fragments;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Safari_Shopping_Mall.Adapters;
using Safari_Shopping_Mall.Accessors;
using Safari_Shopping_Mall.Activities;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Views.Animations;
using Safari_Shopping_Mall.Helpers;
using Android.Transitions;
using Android.Support.V4.App;
using System.Data;
using Java.Util;
using Android.Support.V4.View;
using Android.Runtime;
using Firebase.Database;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using System.Linq;
using CoolTechWorks.Views.Shimmer;
using Android.Content.PM;

namespace Safari_Shopping_Mall
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class ShoppingActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener,
    IOnSorting, View.IOnClickListener, onAdapterViewClicked, Transaction.IHandler
    {
        #region Variables Declaration
        private FirebaseDatabase db = FirebaseDatabase.Instance;
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private DrawerLayout mDrawerLayout;
        private BottomNavigationView mBottomNav;
        private string CategoryTitle { get; set; }
        private int filters { get; set; }
        private RecyclerView recyclerView;
        private Product_Adapter Adapter;
        private ShimmerRecyclerView mLoaderEffect;
        private List<Products> products;
        private XamarinRecyclerViewOnScrollListener onScrollListener;
        private ProgressBar progressBar;
        private CardView mBottomCardView;
        private MyEnum mCurrentLayoutManagerType;
        private int[] cartLocation = new int[2];
        private string _lang;
        HttpClient client = new HttpClient();
        private string Category { get; set; }
        private string SubCategory { get; set; }
        private int mPageOffset { get; set; }
        private int LastLoad { get; set; }
        private string UserID { get; set; }
        private string searchQuery { get; set; }
        internal APIMode CurrentMode { get; private set; }
        public string SearchOffset { get; private set; }

        private Android.Support.V7.Widget.SearchView _searchView;
        private DividerItemDecoration verticalDecoration;
        private DividerItemDecoration horizontalDecoration;
        private ImageView _CartItemCount;
        #endregion
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private int mCartItemCount;
        private SqlLiteSession Broker = new SqlLiteSession();
        private bool sortByAsc = false;
        private BottomSheetDialog mBottomSheetDialog;
        private bool sorted = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            UserID = app.GetString("USERID", string.Empty);
            _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);
            filters = -1;
            SubCategory = Intent.GetStringExtra("SubCategory");

            CountWatched();

            base.OnCreate(savedInstanceState);

            CategoryTitle = Intent.GetStringExtra("Title");

            // Create your application here
            SetContentView(Resource.Layout.shopping_main);
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            mBottomNav = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation_view);
            mBottomCardView = FindViewById<CardView>(Resource.Id.bottom_cardview);
            mLoaderEffect = FindViewById<ShimmerRecyclerView>(Resource.Id.shimmer_recycler_view);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            mLoaderEffect.SetLayoutManager(new LinearLayoutManager(this));
            mLoaderEffect.ShowShimmerAdapter();

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);

            mBottomNav.SetOnNavigationItemSelectedListener(this);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            InitDecoration();

            mPageOffset = 0;
            mCurrentLayoutManagerType = MyEnum.ListView;
            InitDecoration();
            SetUpRecyclerView(recyclerView);

            products = new List<Products>();

            Category = Intent.GetStringExtra("CategoryID");
            CategoryTitle = Intent.GetStringExtra("Category");


            SupportActionBar.Title = CategoryTitle;

            GetProducts();
            CurrentMode = APIMode.Direct;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_shopping, menu);

            var item = menu.FindItem(Resource.Id.search);
            var searchView = MenuItemCompat.GetActionView(item);

            _searchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();
            _searchView.QueryTextSubmit += SearchView_QueryTextSubmitAsync;

            var hint = Resources.GetString(Resource.String.hint_search_in);

            _searchView.QueryHint = hint + " " + CategoryTitle;

            var item2 = menu.FindItem(Resource.Id.action_cart);


            View actionView = MenuItemCompat.GetActionView(item2);
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

        private void SearchView_QueryTextSubmitAsync(object sender, Android.Support.V7.Widget.SearchView.QueryTextSubmitEventArgs e)
        {
            _searchView.ClearFocus();
            _searchView.OnActionViewCollapsed();

            SearchOffset = string.Empty;
            Adapter.Clear();

            CurrentMode = APIMode.Search;
            onScrollListener.IsLoading = true;
            HideShowProgress();
            searchQuery = e.Query;

            SupportActionBar.Title = e.Query;
            SeachProducts(searchQuery, SearchOffset);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.search:
                    Adapter.Clear();
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.sorting:
                    SortingDialog();
                    return true;
                case Resource.Id.view:
                    ChangeLayout(recyclerView);
                    return true;
                case Resource.Id.home:
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    Intent i = new Intent(this, typeof(MainActivity));
                    StartActivity(i, option.ToBundle());
                    return true;
                default:
                    break;
            }
            return OnNavigationItemSelected(item);
        }

        private void SortingDialog()
        {
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.dlg_title_sorting);
            builder.SetSingleChoiceItems(Resource.Array.sorting_price, filters, delegate (object s, DialogClickEventArgs e)
            {
                filters = e.Which;
            });
            builder.SetPositiveButton(Resource.String.dialog_ok, delegate
            {
                builder.Dispose();
                if (filters == 0)
                {
                    //Sort by high price
                    sortByAsc = false;
                    SortListed(sortByAsc);
                }
                else
                {
                    //Sort by lower price
                    sortByAsc = true;
                    SortListed(sortByAsc);
                }
                sorted = true;

            });
            builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
            { builder.Dispose(); });
            builder.Show();
        }

        private void OnScrollListener_LoadMoreEventAsync(object sender, EventArgs e)
        {
            //Load more stuff here
            onScrollListener.IsLoading = true;
            HideShowProgress();

            if (CurrentMode == APIMode.Direct)
            {
                if (LastLoad == 10)
                {
                    GetProducts();
                }
                else
                {
                    onScrollListener.IsLoading = false;
                    HideShowProgress();
                }
            }
            else
            {
                if (LastLoad == 6)
                {
                    SeachProducts(searchQuery, SearchOffset);
                }
                else
                {
                    onScrollListener.IsLoading = false;
                    HideShowProgress();
                }
            }
        }

        private void SetUpRecyclerView(RecyclerView _recyclerView)
        {
            //Create our layout manager
            _recyclerView.AddItemDecoration(horizontalDecoration);

            recyclerView.HasFixedSize = true;
            var layoutManager = new LinearLayoutManager(this);

            onScrollListener = new XamarinRecyclerViewOnScrollListener(layoutManager);
            onScrollListener.LoadMoreEvent += OnScrollListener_LoadMoreEventAsync;
            onScrollListener.OnHide += OnScrollListener_OnHide;
            onScrollListener.OnShow += OnScrollListener_OnShow;
            recyclerView.AddOnScrollListener(onScrollListener);
            recyclerView.SetLayoutManager(layoutManager);
        }

        private void OnScrollListener_OnShow(object sender, EventArgs e)
        {
            showViews();
        }

        private void OnScrollListener_OnHide(object sender, EventArgs e)
        {
            hideViews();
        }

        private void HideShowProgress()
        {
            if (onScrollListener.IsLoading)
            {
                progressBar.Visibility = ViewStates.Visible;
            }
            else
            {
                progressBar.Visibility = ViewStates.Invisible;
            }
        }

        private void hideViews()
        {
            mToolbar.Animate().TranslationY(-mToolbar.Height).SetInterpolator(new AccelerateInterpolator(2));

            FrameLayout.LayoutParams lp = (FrameLayout.LayoutParams)mBottomCardView.LayoutParameters;
            int fabBottomMargin = lp.BottomMargin;
            mBottomCardView.Animate().TranslationY(mBottomCardView.Height + fabBottomMargin).SetInterpolator(new AccelerateInterpolator(2)).Start();
        }

        private void showViews()
        {
            mToolbar.Animate().TranslationY(0).SetInterpolator(new DecelerateInterpolator(2));
            mBottomCardView.Animate().TranslationY(0).SetInterpolator(new DecelerateInterpolator(2)).Start();
        }

        public void sortingHigh_Price()
        {
            Adapter.mSortHigh_Price = true;
            Adapter.Sort();
        }

        public void sortingLow_Price()
        {
            Adapter.mSortLow_Price = true;
            Adapter.Sort();
        }

        public void sorting_New()
        {
            //Adapter.SortNew_Condition();
        }

        public void sortingUsed()
        {
            //Adapter.SortUsed_Condition();
        }

        private void ChangeLayout(RecyclerView _recyclerView)
        {
            if (Adapter != null && Adapter.ItemCount > 0)
            {
                int scrollPosition = 0;
                LinearLayoutManager mLayoutManager;
                // If a layout manager has already been set, get current scroll position.
                if (_recyclerView.GetLayoutManager() != null)
                {
                    scrollPosition = ((LinearLayoutManager)_recyclerView.GetLayoutManager())
                            .FindFirstCompletelyVisibleItemPosition();
                }

                switch (mCurrentLayoutManagerType)
                {
                    case MyEnum.ListView:
                        mLayoutManager = new LinearLayoutManager(this);
                        mCurrentLayoutManagerType = MyEnum.Thumbnail;
                        ChangeMenuIconToThumbnail();
                        _recyclerView.SetLayoutManager(mLayoutManager);
                        onScrollListener = new XamarinRecyclerViewOnScrollListener(mLayoutManager);
                        break;
                    case MyEnum.Thumbnail:
                        var mGridLayoutManager = new GridLayoutManager(this, 2, GridLayoutManager.Vertical, false);
                        mCurrentLayoutManagerType = MyEnum.GridView;
                        ChangeMenuIconToList();
                        _recyclerView.SetLayoutManager(mGridLayoutManager);
                        onScrollListener = new XamarinRecyclerViewOnScrollListener(mGridLayoutManager);
                        break;
                    case MyEnum.GridView:
                        mLayoutManager = new LinearLayoutManager(this);
                        mCurrentLayoutManagerType = MyEnum.ListView;
                        ChangeMenuIconToList();
                        _recyclerView.SetLayoutManager(mLayoutManager);
                        onScrollListener = new XamarinRecyclerViewOnScrollListener(mLayoutManager);
                        break;
                    default:
                        mLayoutManager = new LinearLayoutManager(this);
                        mCurrentLayoutManagerType = MyEnum.ListView;
                        ChangeMenuIconToList();
                        _recyclerView.SetLayoutManager(mLayoutManager);
                        onScrollListener = new XamarinRecyclerViewOnScrollListener(mLayoutManager);
                        break;
                }

                Adapter = new Product_Adapter(this, Adapter.Product, this);
                Adapter.SetLayoutManager(mCurrentLayoutManagerType);

                _recyclerView.SetAdapter(Adapter);
                _recyclerView.ScrollToPosition(scrollPosition);
                recyclerView.RemoveOnScrollListener(onScrollListener);


                onScrollListener.LoadMoreEvent += OnScrollListener_LoadMoreEventAsync;
                onScrollListener.OnHide += OnScrollListener_OnHide;
                onScrollListener.OnShow += OnScrollListener_OnShow;
                recyclerView.AddOnScrollListener(onScrollListener);

                //Decorate Recyclerview
                if (mCurrentLayoutManagerType == MyEnum.GridView)
                {
                    _recyclerView.AddItemDecoration(verticalDecoration);
                }
                else
                {
                    _recyclerView.AddItemDecoration(verticalDecoration);
                }

            }

        }

        private void ChangeMenuIconToThumbnail()
        {
            var item = mBottomNav.Menu.FindItem(Resource.Id.view);
            item.SetIcon(Resource.Drawable.ic_if_grid_01_186402);
        }

        private void ChangeMenuIconToList()
        {
            var item = mBottomNav.Menu.FindItem(Resource.Id.view);
            item.SetIcon(Resource.Drawable.ic_list_black_24dp);
        }

        public void OnClick(View v)
        {
            //throw new NotImplementedException();
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

        private void GetProducts()
        {
            var mPageLimit = 10;
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            if (sorted)
                p.GetProducts(Category, SubCategory, mPageOffset, mPageLimit, sortByAsc, database);
            else
                p.GetProducts(Category, SubCategory, mPageOffset, mPageLimit, database);

            p.GetProduct += (sender, obj) =>
            {
                mPageOffset = mPageOffset + mPageLimit;
                mLoaderEffect.Visibility = ViewStates.Gone;
                mLoaderEffect.HideShimmerAdapter();
                if (Adapter != null && Adapter.Product.Count > 0)
                {
                    Adapter.AddList(obj);
                }
                else
                {
                    Adapter = new Product_Adapter(this, obj, this);
                    recyclerView.SetAdapter(Adapter);
                }
                LastLoad = obj.Count;
                onScrollListener.IsLoading = false;
                HideShowProgress();
            };
        }

        private void SeachProducts(string query, string offset)
        {
            //Clear adapter
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.SearchProducts(query.ToLower(), Category, SubCategory, offset, database);
            p.GetProduct += (sender, obj) =>
            {
                if (obj.Count > 0)
                {
                    if (Adapter != null && Adapter.Product.Count > 0)
                    {
                        Adapter.AddList(obj);
                    }
                    else
                    {
                        Adapter = new Product_Adapter(this, obj, this);
                        recyclerView.SetAdapter(Adapter);
                    }
                    onScrollListener.IsLoading = false;
                    HideShowProgress();
                    SearchOffset = obj[obj.Count - 1].sort_name;

                }
                else if (Adapter.Product.Count == 0)
                {
                    onScrollListener.IsLoading = false;
                    HideShowProgress();
                    var builder = new Android.App.AlertDialog.Builder(this);
                    builder.SetTitle(Resource.String.dlg_info);
                    builder.SetMessage(Resource.String.info_nomatch_result);
                    builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                    {
                        builder.Dispose();
                    });
                    builder.Show();

                }
                else
                {
                    onScrollListener.IsLoading = false;
                    HideShowProgress();
                }
                LastLoad = obj.Count;

            };
        }

        public override void OnBackPressed()
        {

            if (CurrentMode == APIMode.Search)
            {
                Adapter.Clear();
                Adapter = null;
                GetProducts();
                SupportActionBar.Title = CategoryTitle;
                CurrentMode = APIMode.Direct;
            }
            else
            {
                base.OnBackPressed();
            }
        }

        private void InitDecoration()
        {
            verticalDecoration = new DividerItemDecoration(this, DividerItemDecoration.Horizontal);
            Drawable verticalDivider = ContextCompat.GetDrawable(this, Resource.Drawable.line_divider);
            verticalDecoration.SetDrawable(verticalDivider);

            horizontalDecoration = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            Drawable horizontalDivider = ContextCompat.GetDrawable(this, Resource.Drawable.horizontal_line);
            horizontalDecoration.SetDrawable(horizontalDivider);

        }

        public void OnItemClicked(int p, ImageView view)
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, view, Adapter.Product[p].ProductID);
            Intent i = new Intent(this, typeof(ActivityDetails));

            i.PutExtra("SellerID", Adapter.Product[p].SellerID);
            i.PutExtra("ProductID", Adapter.Product[p].ProductID);
            i.PutExtra("Product", Adapter.Product[p].Product);
            i.PutExtra("Price", Adapter.Product[p].Price);
            i.PutExtra("Buy_Price", Adapter.Product[p].Buy_Price);
            i.PutExtra("Offer", Adapter.Product[p].Offer_Price);
            i.PutExtra("EndDate", Adapter.Product[p].OfferEnds);
            i.PutExtra("Condition", Adapter.Product[p].Condition);

            i.PutExtra("Description", Adapter.Product[p].Description);
            i.PutExtra("Sizes", Adapter.Product[p].Sizes);
            i.PutExtra("Thumbnail_1", Adapter.Product[p].Thumbnail_1);
            i.PutExtra("Thumbnail_2", Adapter.Product[p].Thumbnail_2);
            i.PutExtra("Thumbnail_3", Adapter.Product[p].Thumbnail_3);
            i.PutExtra("Category", Adapter.Product[p].CategoryID);
            i.PutExtra("SubCategory", Adapter.Product[p].SubCategoryID);

            StartActivity(i, option.ToBundle());
        }

        public void OnItemOptionClicked(int p)
        {
            OptionMenuDialog(p);
        }

        protected override void OnResume()
        {
            base.OnResume();
            InvalidateOptionsMenu();
        }

        public void CountWatched()
        {
            FirebaseDatabase db = FirebaseDatabase.Instance;
            DatabaseReference myRef = db.GetReference("SubCategories/" + SubCategory);
            myRef.RunTransaction(this);
        }

        private void SortListed(bool asc)
        {
            if (Adapter != null && Adapter.ItemCount > 0)
            {
                onScrollListener.IsLoading = true;
                HideShowProgress();

                if (asc)
                {
                    //sort by low price
                    var Sorted = Adapter.Product.OrderBy(i => i.Product).ToList();
                    Adapter.Product.Clear();
                    Adapter.AddList(Sorted);


                    int scrollPosition = 0;
                    if (recyclerView.GetLayoutManager() != null)
                    {
                        scrollPosition = ((LinearLayoutManager)recyclerView.GetLayoutManager())
                                .FindFirstCompletelyVisibleItemPosition();
                    }
                    recyclerView.ScrollToPosition(scrollPosition);


                }
                else
                {
                    //sort by high price
                    var Sorted = Adapter.Product.OrderByDescending(i => i.Price).ToList();
                    Adapter.Product.Clear();
                    Adapter.AddList(Sorted);


                    int scrollPosition = 0;
                    if (recyclerView.GetLayoutManager() != null)
                    {
                        scrollPosition = ((LinearLayoutManager)recyclerView.GetLayoutManager())
                                .FindFirstCompletelyVisibleItemPosition();
                    }
                    recyclerView.ScrollToPosition(scrollPosition);

                }

            }
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
            //throw new NotImplementedException();
        }

        private void OptionMenuDialog(int pos)
        {
            var optionMenu = LayoutInflater.Inflate(Resource.Layout.menu_dialog, null, false);
            var btnShare = optionMenu.FindViewById<Button>(Resource.Id.btnShare);
            var btnWish = optionMenu.FindViewById<Button>(Resource.Id.btnWishlist);

            var alert = new Android.App.AlertDialog.Builder(this).Create() ;
            alert.SetView(optionMenu);
            alert.Window.EnterTransition = new Slide(GravityFlags.Bottom);
            alert.Window.RequestFeature(WindowFeatures.NoTitle);
            alert.Show();

            btnShare.Click += (o, s) =>
            {
                alert.Dismiss();
                ShareBottom();
            };
            btnWish.Click += (o, s) =>
            {
                alert.Dismiss();
                var product = Adapter.Product[pos].Product;
                var id = Adapter.Product[pos].ProductID;
                var categoryID = Adapter.Product[pos].CategoryID;
                var subcategoryID = Adapter.Product[pos].SubCategoryID;

                HashMap map = new HashMap();
                map.Put("ProductID", id);
                map.Put("CategoryID", categoryID);
                map.Put("SubCategoryID", subcategoryID);

                db = FirebaseDatabase.Instance;
                var _ref = db.GetReference("Wishlists");
                _ref.Child(UserID).Child(id).SetValue(map);

                var text = Resources.GetString(Resource.String.snkbr_added_wishlist);
                Snackbar.Make(FindViewById(Android.Resource.Id.Content), product + " " + text, Snackbar.LengthShort).Show();
            };

        }

        private void ShareBottom()
        {
            View bottomSheetLayout = LayoutInflater.Inflate(Resource.Layout.btm_share_dlg, null);
            (bottomSheetLayout.FindViewById(Resource.Id.button_close)).Click += (o, s) =>
            {
                //Close
                mBottomSheetDialog.Dismiss();
            };
            (bottomSheetLayout.FindViewById(Resource.Id.fb)).Click += (o, s) =>
            {
                //Close
                mBottomSheetDialog.Dismiss();
                ShareFacebook();
            };
            (bottomSheetLayout.FindViewById(Resource.Id.insta)).Click += (o, s) =>
            {
                //Close
                mBottomSheetDialog.Dismiss();
                ShareInstagram();
            };
            (bottomSheetLayout.FindViewById(Resource.Id.whatsapp)).Click += (o, s) =>
            {
                //Close
                mBottomSheetDialog.Dismiss();
                ShareWhatsUp();
            };
            (bottomSheetLayout.FindViewById(Resource.Id.twitter)).Click += (o, s) =>
            {
                //Close
                mBottomSheetDialog.Dismiss();
                ShareTwitter();
                
            };

            mBottomSheetDialog = new BottomSheetDialog(this);
            mBottomSheetDialog.SetContentView(bottomSheetLayout);
            mBottomSheetDialog.Show();
        }

        private void ShareWhatsUp()
        {
            try
            {
                Intent whatsappIntent = new Intent(Intent.ActionSend);
                whatsappIntent.SetType("text/plain");
                whatsappIntent.PutExtra(Intent.ExtraSubject, "Wanunuzi.com");
                string sAux = "\nLet me recommend you this application\n\n";
                sAux = sAux + "https://play.google.com/store/apps/details?id=wanunuzi.com \n\n";

                whatsappIntent.PutExtra(Intent.ExtraText, sAux);

                whatsappIntent.SetPackage("com.whatsapp");
                whatsappIntent.PutExtra(Intent.ExtraText, sAux);
                this.StartActivity(whatsappIntent);

            }
            catch
            {
                var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                builder.SetTitle(Resource.String.dlg_info);
                builder.SetMessage("Whatsapp Not Installed");
                builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                {

                    builder.Dispose();
                });
                builder.Show();
            }
        }

        private void ShareInstagram()
        {
            try
            {
                var info = PackageManager.GetApplicationInfo("com.instagram.android", 0);
                Intent i = new Intent(Intent.ActionSend);
                i.SetType("text/plain");
                i.PutExtra(Intent.ExtraSubject, "Wanunuzi.com");
                string sAux = "\nLet me recommend you this application\n\n";
                sAux = sAux + "https://play.google.com/store/apps/details?id=wanunuzi.com \n\n";

                i.PutExtra(Intent.ExtraText, sAux);

                i.PutExtra(Intent.ExtraText, sAux);
                i.SetPackage("com.instagram.android");
                StartActivity(i);

            }
            catch
            {
                var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                builder.SetTitle(Resource.String.dlg_info);
                builder.SetMessage("Instagram Not Installed");
                builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                {
                    builder.Dispose();
                });
                builder.Show();
            }
        }

        private void ShareFacebook()
        {
            var urlToShare = string.Empty;
            Intent intent = new Intent(Intent.ActionSend);
            intent.SetType("text/plain");
            intent.PutExtra(Intent.ExtraSubject, "Wanunuzi.com");
            string sAux = "\nLet me recommend you this application\n\n";
            sAux = sAux + "https://play.google.com/store/apps/details?id=wanunuzi.com \n\n";

            intent.PutExtra(Intent.ExtraText, sAux);
            // See if official Facebook app is found
            bool facebookAppFound = false;
            var matches = PackageManager.QueryIntentActivities(intent, 0);
            foreach (ResolveInfo info in matches)
            {
                if (info.ActivityInfo.PackageName.ToLower().StartsWith("com.facebook"))
                {
                    intent.SetPackage(info.ActivityInfo.PackageName);
                    facebookAppFound = true;
                    break;
                }
            }
            // As fallback, launch sharer.php in a browser
            if (!facebookAppFound)
            {
                var sharerUrl = "https://www.facebook.com/sharer/sharer.php?u=" + urlToShare;
                intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(sharerUrl));
            }
            StartActivity(intent);
        }

        private void ShareTwitter()
        {
            var urlToShare = string.Empty;
            Intent intent = new Intent(Intent.ActionSend);
            intent.SetType("text/plain");
            intent.PutExtra(Intent.ExtraSubject, "Wanunuzi.com");
            string sAux = "\nLet me recommend you this application\n\n";
            sAux = sAux + "https://play.google.com/store/apps/details?id=wanunuzi.com \n\n";

            intent.PutExtra(Intent.ExtraText, sAux);
            // See if official Facebook app is found
            bool facebookAppFound = false;
            var matches = PackageManager.QueryIntentActivities(intent, 0);
            foreach (ResolveInfo info in matches)
            {
                if (info.ActivityInfo.PackageName.ToLower().StartsWith("com.twitter.android"))
                {
                    intent.SetPackage(info.ActivityInfo.PackageName);
                    facebookAppFound = true;
                    break;
                }
            }
            // As fallback, launch sharer.php in a browser
            if (!facebookAppFound)
            {
                var sharerUrl = "https://www.twitter.com/intent/tweet?url=" + sAux;
                intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(sharerUrl));
            }
            StartActivity(intent);
        }

    }
    enum APIMode
    {
        Search,
        Direct
    }

    public enum MyEnum
    {
        Thumbnail,
        ListView,
        GridView
    }
}