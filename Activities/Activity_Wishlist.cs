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
using Android.Support.V7.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Java.Util;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.Design.Widget;
using CoolTechWorks.Views.Shimmer;
using Android.Views.Animations;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Safari_Shopping_Mall.Accessors;
using Safari_Shopping_Mall.Adapters;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Transitions;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme2")]
    public class Activity_Wishlist : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener, onAdapterViewClicked
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private string _lang;
        private Wishlist_Adapter mAdapter;
        private XamarinRecyclerViewOnScrollListener onScrollListener;
        private DividerItemDecoration horizontalDecoration;
        private BottomNavigationView mBottomNav;
        private CardView mBottomCardView;
        private ProgressBar progressBar;
        private int mPageOffset;
        private MyEnum mCurrentLayoutManagerType;
        private RecyclerView recyclerView;
        private ShimmerRecyclerView LoadingSkeleton;
        private List<Wishlist> myList;
        private List<Products> _products;
        FirebaseDatabase database = FirebaseDatabase.Instance;
        FirebaseCallback p = new FirebaseCallback();
        private BottomSheetDialog mBottomSheetDialog;
        private FirebaseDatabase db;
        private int filters = 0;
        private bool sortByAsc = true;
        private DividerItemDecoration verticalDecoration;

        public string CategoryID { get; private set; }
        public string SubCategoryID { get; private set; }
        public string ProductID { get; private set; }
        public string UserID { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _lang = app.GetString("Language", "en");
            UserID = app.GetString("USERID", string.Empty);
            ChangeLanguage(_lang);
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_wishlist);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            mBottomNav = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation_view);
            mBottomCardView = FindViewById<CardView>(Resource.Id.bottom_cardview);
            LoadingSkeleton = FindViewById<ShimmerRecyclerView>(Resource.Id.shimmer_recycler_view);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            LoadingSkeleton.SetLayoutManager(new LinearLayoutManager(this));
            LoadingSkeleton.ShowShimmerAdapter();
            mBottomNav.SetOnNavigationItemSelectedListener(this);

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "Wishlist";
            mPageOffset = 0;
            mCurrentLayoutManagerType = MyEnum.ListView;
            InitDecoration();
            SetUpRecyclerView(recyclerView);
            p.GetProduct += P_GetProduct;

            _products = new List<Products>();
            progressBar.Visibility = ViewStates.Invisible;
            UserWishList();
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

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void SetUpRecyclerView(RecyclerView _recyclerView)
        {
            //Create our layout manager
            _recyclerView.AddItemDecoration(horizontalDecoration);
            _recyclerView.SetItemAnimator(new DefaultItemAnimator());

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
            showViews();
        }

        private void OnScrollListener_OnHide(object sender, EventArgs e)
        {
            hideViews();
        }

        private void OnScrollListener_LoadMoreEventAsync(object sender, EventArgs e)
        {
            _products = new List<Products>();
            if (myList.Count > 0)
            {
                onScrollListener.IsLoading = true;
                HideShowProgress();

                LoadingSkeleton.SetDemoChildCount(myList.Count);
                CategoryID = myList[myList.Count - 1].CategoryID;
                SubCategoryID = myList[myList.Count - 1].SubCategoryID;
                ProductID = myList[myList.Count - 1].ProductID;
                GetWishList(CategoryID, SubCategoryID, ProductID);
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
            });
            builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
            { builder.Dispose(); });
            builder.Show();
        }

        private void SortListed(bool sortByAsc)
        {
            if (mAdapter != null && mAdapter.ItemCount > 0)
            {

                if (sortByAsc)
                {
                    var products = mAdapter.Product.OrderBy(i => i.Product).ToList();
                    mAdapter.Clear();

                    mAdapter.AddList(products);
                }
                else
                {
                    var products = mAdapter.Product.OrderByDescending(i => i.Price).ToList();
                    mAdapter.Clear();
                    mAdapter.AddList(products);
                }

            }
        }

        private void ChangeLayout(RecyclerView _recyclerView)
        {
            if (mAdapter != null && mAdapter.ItemCount > 0)
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

                mAdapter = new Wishlist_Adapter(this, mAdapter.Product, this);
                mAdapter.SetLayoutManager(mCurrentLayoutManagerType);

                _recyclerView.SetAdapter(mAdapter);
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

        private void GetWishList(string categoryID,string subcategoryID, string productID)
        {
            onScrollListener.IsLoading = true;
            HideShowProgress();
            p.GetWishlist(categoryID, subcategoryID, productID, database);
        }

        private void P_GetProduct(object sender, List<Products> obj)
        {
            _products = _products.Concat(obj).ToList();

            if (_products.Count == 6)
            {
                LoadingSkeleton.HideShimmerAdapter();
                LoadingSkeleton.Visibility = ViewStates.Gone;

                if (mAdapter != null && mAdapter.Product.Count > 0)
                {
                    mAdapter.AddList(_products);
                }
                else
                {
                    mAdapter = new Wishlist_Adapter(this, _products, this);
                    recyclerView.SetAdapter(mAdapter);
                }

                onScrollListener.IsLoading = false;
                HideShowProgress();

            }
            else if (_products.Count > 0 && myList.Count == 0)
            {
                LoadingSkeleton.HideShimmerAdapter();
                LoadingSkeleton.Visibility = ViewStates.Gone;

                if (mAdapter != null && mAdapter.Product.Count > 0)
                {
                    mAdapter.AddList(_products);
                }
                else
                {
                    mAdapter = new Wishlist_Adapter(this, _products, this);
                    recyclerView.SetAdapter(mAdapter);
                }
                onScrollListener.IsLoading = false;
                HideShowProgress();

            }
            else
            {
                if (myList.Count > 0)
                {
                    CategoryID = myList[myList.Count - 1].CategoryID;
                    SubCategoryID = myList[myList.Count - 1].SubCategoryID;
                    ProductID = myList[myList.Count - 1].ProductID;
                    GetWishList(CategoryID, SubCategoryID, ProductID);

                    myList.RemoveAt(myList.Count - 1);
                }
                else
                {
                    LoadingSkeleton.HideShimmerAdapter();
                    LoadingSkeleton.Visibility = ViewStates.Gone;
                }
            }
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

        private void UserWishList()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.Wishlist(UserID,database);
            p.MyWishLish += (o, s) =>
            {
                myList = s;

                if (myList.Count > 0 )
                {
                    LoadingSkeleton.SetDemoChildCount(myList.Count);
                    CategoryID = myList[myList.Count - 1].CategoryID;
                    SubCategoryID = myList[myList.Count - 1].SubCategoryID;
                    ProductID = myList[myList.Count - 1].ProductID;
                    GetWishList(CategoryID,SubCategoryID,ProductID);
                    myList.RemoveAt(myList.Count - 1);

                }
                else
                {
                    LoadingSkeleton.HideShimmerAdapter();
                    LoadingSkeleton.Visibility = ViewStates.Gone;
                }
            };
        }

        public void OnItemClicked(int p, ImageView view)
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, view, mAdapter.Product[p].ProductID);
            Intent i = new Intent(this, typeof(ActivityDetails));

            i.PutExtra("SellerID", mAdapter.Product[p].SellerID);
            i.PutExtra("ProductID", mAdapter.Product[p].ProductID);
            i.PutExtra("Product", mAdapter.Product[p].Product);
            i.PutExtra("Price", mAdapter.Product[p].Price);
            i.PutExtra("Buy_Price", mAdapter.Product[p].Buy_Price);
            i.PutExtra("Offer", mAdapter.Product[p].Offer_Price);
            i.PutExtra("EndDate", mAdapter.Product[p].OfferEnds);

            i.PutExtra("Description", mAdapter.Product[p].Description);
            i.PutExtra("Sizes", mAdapter.Product[p].Sizes);
            i.PutExtra("Thumbnail_1", mAdapter.Product[p].Thumbnail_1);
            i.PutExtra("Thumbnail_2", mAdapter.Product[p].Thumbnail_2);
            i.PutExtra("Thumbnail_3", mAdapter.Product[p].Thumbnail_3);
            i.PutExtra("Category", mAdapter.Product[p].CategoryID);
            i.PutExtra("SubCategory", mAdapter.Product[p].SubCategoryID);

            StartActivity(i, option.ToBundle());
        }

        public void OnItemOptionClicked(int p)
        {
            OptionMenuDialog(p);
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

        private void OptionMenuDialog(int pos)
        {
            var optionMenu = LayoutInflater.Inflate(Resource.Layout.menu_dialog_2, null, false);
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
                var product = mAdapter.Product[pos].Product;
                var id = mAdapter.Product[pos].ProductID;
                var categoryID = mAdapter.Product[pos].CategoryID;
                var subcategoryID = mAdapter.Product[pos].SubCategoryID;

                db = FirebaseDatabase.Instance;
                var _ref = db.GetReference("Wishlists");
                _ref.Child(UserID).Child(id).RemoveValue();

                mAdapter.OnItemDismiss(pos);
                var text = Resources.GetString(Resource.String.snkbr_rem_wishlist);
                Snackbar.Make(FindViewById(Android.Resource.Id.Content), product + " " + text, Snackbar.LengthShort).Show();
            };

        }

    }
}