
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Transitions;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Safari_Shopping_Mall.Accessors;
using Safari_Shopping_Mall.Activities;
using Safari_Shopping_Mall.Adapters;
using Safari_Shopping_Mall.Helpers;
using System;
using System.Collections.Generic;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Firebase.Database;
using Android.Support.V4.Content;
using Android.Graphics.Drawables;
using Safari_Shopping_Mall.Fragments;
using Android.Content.PM;
using Java.Util;

namespace Safari_Shopping_Mall
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class ActivitySearch : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener,
    IOnSorting, onAdapterViewClicked
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private event EventHandler onclick;
        Android.Support.V7.Widget.SearchView searchView;
        private List<Products> products;
        private XamarinRecyclerViewOnScrollListener onScrollListener;
        private BottomNavigationView mBottomNav;
        private CardView mBottomCardView;
        private CardView mCardProgessbar;
        private RecyclerView recyclerView;
        private ProgressBar progressBar;
        public static string SearchOffset { get; private set; }
        private  int LastCount { get; set; }
        public string UserID { get; private set; }

        private List<SubCategories> SubCategory;
        private event EventHandler search;
        private Product_Adapter Adapter;
        private Android.Support.V7.Widget.SearchView _searchView;
        private string CurrectCategory;
        private string CurrentSubCategory;
        private string searchQuery;
        private int maxIndex = 0;
        private RelativeLayout mLoaderContent;
        private ProgressBar mSpinner;
        private DividerItemDecoration verticalDecoration;
        private DividerItemDecoration horizontalDecoration;
        private MyEnum mCurrentLayoutManagerType;
        private bool IsSearchCompleted = false;
        private TextView textCartItemCount;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private int mCartItemCount;
        private string _category;
        private Button btnSearch;
        private ImageView _CartItemCount;
        private TextView txtSearch;
        private ImageView imgSearch;
        private BottomSheetDialog mBottomSheetDialog;
        private FirebaseDatabase db;
        private string _lang;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            UserID = app.GetString("USERID", string.Empty);
            _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);

            Fade slide = new Fade(FadingMode.In);
            Explode explode = new Explode();

            Window.EnterTransition = slide;
            Window.ExitTransition = explode;

            // Create your application here

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.search_activity);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            mBottomNav = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation_view);
            mBottomCardView = FindViewById<CardView>(Resource.Id.bottom_cardview);
            mLoaderContent = FindViewById<RelativeLayout>(Resource.Id.relativeLayout);
            mBottomNav.SetOnNavigationItemSelectedListener(this);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            txtSearch = FindViewById<TextView>(Resource.Id.txtSearch);
            imgSearch = FindViewById<ImageView>(Resource.Id.imgSearchView);
            InitDecoration();

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);

            var hint = Resources.GetString(Resource.String.hint_search_in);

            SupportActionBar.Title = hint + " Wanunuzi";
            SetUpRecyclerView(recyclerView);
            SearchOffset = string.Empty;

            search += ActivitySearch_search;
            progressBar.Visibility = ViewStates.Invisible;
        }

        private void ActivitySearch_search(object sender, EventArgs e)
        {
            if (maxIndex < SubCategory.Count)
            {
                onScrollListener.IsLoading = true;
                HideShowProgress();

                CurrectCategory = SubCategory[maxIndex].CategoryID;
                CurrentSubCategory = SubCategory[maxIndex].SubCategoryID;
                _category = SubCategory[maxIndex].SubCategory;

                SeachProducts(searchQuery, CurrectCategory, CurrentSubCategory, SearchOffset);
            }
            else
                IsSearchCompleted = true;

            if (Adapter == null && IsSearchCompleted)
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
                txtSearch.Visibility = ViewStates.Visible;
            }
        }

        private void SearchView_QueryTextSubmitAsync(object sender, Android.Support.V7.Widget.SearchView.QueryTextSubmitEventArgs e)
        {
            txtSearch.Visibility = ViewStates.Gone;
            imgSearch.Visibility = ViewStates.Gone;
            _searchView.ClearFocus();
            _searchView.OnActionViewCollapsed();

            e.Handled = true;
            searchQuery = e.Query;
            SearchOffset = string.Empty;
            maxIndex = 0;
            if (Adapter != null && Adapter.Product.Count > 0)
            {
                Adapter.Clear();
            }
            if(SubCategory != null && SubCategory.Count > 0)
            {
                search?.Invoke(this, null);
            }
            else
            {
                GetSubCategories();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_shopping, menu);

            var item = menu.FindItem(Resource.Id.search);
            var item2 = menu.FindItem(Resource.Id.action_cart);

            var searchView = MenuItemCompat.GetActionView(item);
            _searchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();

            var hint = Resources.GetString(Resource.String.hint_search_in);

            _searchView.QueryHint = Resources.GetString(Resource.String.hint_search);
            _searchView.QueryTextSubmit += SearchView_QueryTextSubmitAsync;

            _searchView.OnActionViewExpanded();

            View actionView = MenuItemCompat.GetActionView(item2);
            _CartItemCount = actionView.FindViewById<ImageView>(Resource.Id.cart_badge);

            mCartItemCount = app.GetInt("MyCart", 0);
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
                case Resource.Id.search:
                    mLoaderContent.Visibility = ViewStates.Gone;
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

        private void SetUpRecyclerView(RecyclerView _recyclerView)
        {
            //Create our layout manager
            _recyclerView.AddItemDecoration(horizontalDecoration);
            _recyclerView.HasFixedSize = true;
            var layoutManager = new LinearLayoutManager(this);

            onScrollListener = new XamarinRecyclerViewOnScrollListener(layoutManager);
            onScrollListener.LoadMoreEvent += OnScrollListener_LoadMoreEventAsync;
            onScrollListener.OnHide += OnScrollListener_OnHide;
            onScrollListener.OnShow += OnScrollListener_OnShow;
            _recyclerView.AddOnScrollListener(onScrollListener);
            _recyclerView.SetLayoutManager(layoutManager);
        }

        private void OnScrollListener_OnShow(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void OnScrollListener_OnHide(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private  void OnScrollListener_LoadMoreEventAsync(object sender, EventArgs e)
        {
            //Load more stuff here
            if (LastCount == 10)
            {
                onScrollListener.IsLoading = true;
                HideShowProgress();

                SeachProducts(searchQuery, CurrectCategory, CurrentSubCategory, SearchOffset);
            }
            else
            {
                maxIndex++;
                search?.Invoke(this, null);
            }

            if (maxIndex == SubCategory.Count)
            {
                IsSearchCompleted = true;
                onScrollListener.IsLoading = false;
                HideShowProgress();
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

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.sorting:
                    var trans = SupportFragmentManager.BeginTransaction();
                    var sort = new Sort_Menu_Dialogy();
                    sort.Show(trans, "Sorting");
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

        private void SeachProducts(string query,string category, string subcategory,string offset)
        {
       
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.SearchProducts(query.ToLower(), category,subcategory, offset,database);
            p.GetProduct += (sender, obj) =>
            {
                LastCount = obj.Count;
                if(obj.Count > 0)
                {
                    SearchOffset = obj[obj.Count - 1].sort_name;
                    if (Adapter != null && Adapter.Product.Count > 0)
                    {
                        Adapter.AddList(obj);
                    }
                    else
                    {
                        Adapter = new Product_Adapter(this, obj,this);
                        recyclerView.SetAdapter(Adapter);
                    }

                    onScrollListener.IsLoading = false;
                    HideShowProgress();
                    SearchOffset = obj[obj.Count - 1].sort_name;

                }
                else if (LastCount < 10 && !IsSearchCompleted)
                {
                    maxIndex++;
                    search?.Invoke(this, null);
                }

            };
        }

        private void GetSubCategories()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();

            p.GetAllSubCategories(database);
            p.SubCategories += (sender, obj) =>
            {
                SubCategory = obj;
                search?.Invoke(this, null);
            };
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

        private void ChangeLayout(RecyclerView _recyclerView)
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

            if (Adapter != null && Adapter.Product.Count != 0)
            {
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

        private void OptionMenuDialog(int pos)
        {
            var optionMenu = LayoutInflater.Inflate(Resource.Layout.menu_dialog, null, false);
            var btnShare = optionMenu.FindViewById<Button>(Resource.Id.btnShare);
            var btnWish = optionMenu.FindViewById<Button>(Resource.Id.btnWishlist);

            var alert = new Android.App.AlertDialog.Builder(this).Create();
            alert.SetView(optionMenu);
            alert.Window.EnterTransition = new Slide(GravityFlags.Bottom);
            alert.Window.RequestFeature(WindowFeatures.NoTitle);
            alert.Show();

            btnShare.Click += (o, s) =>
            {
                alert.Dismiss();
                ShareBottomDialog();
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

        public void OnItemClicked(int p, ImageView view)
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, view, Adapter.Product[p].ProductID);
            Intent i = new Intent(this, typeof(ActivityDetails));

            i.PutExtra("SellerID", Adapter.Product[p].SellerID);
            i.PutExtra("ProductID", Adapter.Product[p].ProductID);
            i.PutExtra("Product", Adapter.Product[p].Product);
            i.PutExtra("Price", Adapter.Product[p].Price);
            i.PutExtra("Buy_Price", Adapter.Product[p].Buy_Price);
            i.PutExtra("Description", Adapter.Product[p].Description);
            i.PutExtra("Sizes", Adapter.Product[p].Sizes);
            i.PutExtra("Thumbnail_1", Adapter.Product[p].Thumbnail_1);
            i.PutExtra("Thumbnail_2", Adapter.Product[p].Thumbnail_2);
            i.PutExtra("Thumbnail_3", Adapter.Product[p].Thumbnail_3);
            i.PutExtra("Category", Adapter.Product[p].Category);
            i.PutExtra("SubCategory", Adapter.Product[p].SubCategory);

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

        private void ShareBottomDialog()
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
                whatsappIntent.PutExtra(Intent.ExtraSubject, "My application name");
                string sAux = "\nLet me recommend you this application\n\n";
                sAux = sAux + "https://play.google.com/store/apps/details?id=the.package.id \n\n";

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
                i.PutExtra(Intent.ExtraSubject, "My application name");
                string sAux = "\nLet me recommend you this application\n\n";
                sAux = sAux + "https://play.google.com/store/apps/details?id=the.package.id \n\n";

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
            intent.PutExtra(Intent.ExtraSubject, "My application name");
            string sAux = "\nLet me recommend you this application\n\n";
            sAux = sAux + "https://play.google.com/store/apps/details?id=the.package.id \n\n";

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
            intent.PutExtra(Intent.ExtraSubject, "My application name");
            string sAux = "\nLet me recommend you this application\n\n";
            sAux = sAux + "https://play.google.com/store/apps/details?id=the.package.id \n\n";

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

    }
}