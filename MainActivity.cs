using Android.App;
using Android.OS;
using Android.Content;
using System;

using Android.Support.V7.App;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Android.Transitions;
using Android.Support.V7.Widget;
using Safari_Shopping_Mall.Adapters;
using System.Collections.Generic;
using System.Timers;
using Safari_Shopping_Mall.Accessors;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using Safari_Shopping_Mall.Activities;
using Android.Text;
using Android.Graphics;
using Java.Util;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Gms.Common;
using Com.Bumptech.Glide.Load.Resource.Drawable;
using Android.Util;
using Firebase.Iid;
using Firebase.Messaging;
using static Android.Resource;
using Android.Views.Animations;
using Android.Content.PM;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using static Android.Animation.Animator;
using static Android.Animation.ValueAnimator;
using CoolTechWorks.Views.Shimmer;
using Java.Net;
using static Safari_Shopping_Mall.Accessors.Categories;
using Java.Lang;

namespace Safari_Shopping_Mall
{
    [Activity(Label = "Wanunuzi", MainLauncher = false,LaunchMode=LaunchMode.SingleTop, Icon = "@drawable/icon", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
        , ViewPager.IOnPageChangeListener,onAdapterViewClicked,NestedScrollView.IOnScrollChangeListener
        ,IOnItemClick
    {
        #region Variables Declaration
        private RecyclerView recyclerViewNew;
        private RecyclerView recyclerViewCat;
        private Category_Adapter mCatAdapter;
        private string[] items;
        private TextView[] mDots;
        private ViewPager mSlideViewpager;
        private LinearLayout mDotsLayout;
        private DrawerLayout mDrawerLayout;
        private TopProducts_Adapter mTopProductsAdapter;
        private int CurrentPage;
        private System.Timers.Timer t;
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private BottomNavigationView mBottomNav;
        private CardView mBottomCard;

        private List<Products> products;
        private SqlLiteSession Session = new SqlLiteSession();
        private string _lang;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private string selected;
        private string _username;
        private DividerItemDecoration horizontalDecoration;
        private ShimmerRecyclerView mContentLoader;
        private List<Int32> Advert;
        private DividerItemDecoration gridlayoutDecoration;
        private string _phonenumber;
        private FloatingActionButton fab;
        private NestedScrollView nestedScrollView;
        private AppBarLayout appBarLayout1;
        private bool isScrollBottom = false;
        private CoordinatorLayout coordinatorLayout;
        private int oldScrollYPostion = 0;
        private int mCartItemCount;
        private ProgressDialog progressDialog;
        private XamarinRecyclerViewOnScrollListener onScrollListener;
        private List<SubCategories> _SubCategory;
        private int mPageOffset = 0;
        private MultViewAdapter multviewTypeAdapter;
        private RecyclerView multviewRecyclerView;
        private int _index = 0;
        private string subCat;
        private CardView mBottomCardView;
        public string Cat { get; private set; }
        public string UserID { get; private set; }

        private event EventHandler _Pagination;
        #endregion
        static readonly string TAG = "MainActivity";
        private ValueAnimator animator = new ValueAnimator();
        private ViewPager mSlider1;
        private ViewPager mSlider2;
        private ImageSliderAdapter mSlideAdapter1;
        private ImageSliderAdapter mSlideAdapter2;
        private MyStyles LastStyle;
        private bool fabshown = false;
        private int scrolledDistance = 0;
        private bool controlsVisible = true;
        private const int HIDE_THRESHOLD = 20;
        private bool isLoading = false;
        private ProgressBar mProgressBar;
        private List<string> TopCategories = new List<string>();
        private List<SubCategories> _subCat;
        private RecyclerView mHRecyclerView;
        private TopViewProd_Adapter mHAdapter;
        private List<Adverts> mTopList;
        private List<Adverts> mBottList;
        private int CurrentPage1 = 0;
        private int CurrentPage2 = 0;
        private LinearLayout mMainContent;
        private FirebaseDatabase db;
        private BottomSheetDialog mBottomSheetDialog;
        private FloatingActionButton fabReview;
        private SubCategoriesAdapter mSubCategoriesAdapter;
        private RecyclerView mSubCatRecycler;
        private bool doubleBackToExit = false;

        protected override void OnCreate(Bundle bundle)
        {
            _lang = app.GetString("Language", "en");
            _username = app.GetString("Fullname", string.Empty);
            _phonenumber = app.GetString("PhoneNumber", string.Empty);
            UserID = app.GetString("USERID", string.Empty);
            db = FirebaseDatabase.Instance;

            ChangeLanguage(_lang);
            //Subscribe to notification

            base.OnCreate(bundle);

            InitDecoration();
            setUpTransitionAnim();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            setUpActivity();
            setUpSlider();
           

            GetTopProducts();
            GetCategories();
            multviewRecyclerView = FindViewById<RecyclerView>(Resource.Id.multrecyclerView);
            MultViewRecyclerView(multviewRecyclerView);

            _Pagination += MainActivity__Pagination;
            GetSubCategories();

        }

        private void MainActivity__Pagination(object sender, EventArgs e)
        {
            subCat = _SubCategory[_index].SubCategoryID;
            Cat = _SubCategory[_index].CategoryID;
            var trans = _SubCategory[_index].Translation;
            GetTrending(trans,Cat, subCat);
        }

        private void setUpActivity()
        {
            //InitServices();
            Advert = new List<Int32>();
            Advert.Add(Resource.Drawable.images);
            Advert.Add(Resource.Drawable.images1);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            mContentLoader = FindViewById<ShimmerRecyclerView>(Resource.Id.shimmer_recycler_view);
            mBottomNav = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation_view);
            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fabReview = FindViewById<FloatingActionButton>(Resource.Id.fabWhatsapp);
            mSubCatRecycler = FindViewById<RecyclerView>(Resource.Id.subcategoriesSlider);

            nestedScrollView = FindViewById<NestedScrollView>(Resource.Id.nestedScrollview);
            appBarLayout1 = FindViewById<AppBarLayout>(Resource.Id.appbar);
            coordinatorLayout = FindViewById<CoordinatorLayout>(Resource.Id.coordinatorLayout);
            mBottomCardView = FindViewById<CardView>(Resource.Id.bottom_cardview);
            mProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            mMainContent = FindViewById<LinearLayout>(Resource.Id.mMainContent);
            fabReview.Click += (o, s) =>
            {
                ChatWhatsapp();
            };

            mMainContent.Visibility = ViewStates.Gone;
            mContentLoader.SetLayoutManager(new LinearLayoutManager(this));
            mContentLoader.ShowShimmerAdapter();
            fab.Hide();

            fab.Click += (o, s) =>
            {
                ShareBottomDialog();
            };
            Button btnSearch = FindViewById<Button>(Resource.Id.btnsearchview);
            btnSearch.Click += (o, s) =>
            {
                ActivityOptionsCompat option1 = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                Intent i = new Intent(this, typeof(ActivitySearch));
                StartActivity(i, option1.ToBundle());
            };

            mBottomNav.SetOnNavigationItemSelectedListener(this);
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
            {
                var txtUsername = (TextView)navigationView.GetHeaderView(0).FindViewById(Resource.Id.txtUser);
                var txtContacts = (TextView)navigationView.GetHeaderView(0).FindViewById(Resource.Id.txtContacts);

                txtUsername.Text = _username;
                txtContacts.Text = _phonenumber;
                SetUpDrawerContent(navigationView);
            }

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_black_24dp);
            ab.SetDisplayHomeAsUpEnabled(true);
            recyclerViewCat = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            var myPurch = FindViewById<Button>(Resource.Id.btnPurchases);
            var myCart = FindViewById<Button>(Resource.Id.btnCart);

            myPurch.Click += (o, s) =>
            {
                FAQ();
            };


            myCart.Click += (o, s) =>
            {
                Information();
            };

            var gridlayout = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
            mSubCatRecycler.SetLayoutManager(gridlayout);

            var newdATE = DateTime.Now.Date;
            var d = new DateTime(newdATE.Year, newdATE.Month, newdATE.Day);

            FirebaseDatabase database = db;
            DatabaseReference myRef = database.Reference;
            HashMap map = new HashMap();
            map.Put("UserID", UserID);
            map.Put("Date", d.ToBinary());
            myRef.Child("Visitors").Child(d.ToBinary().ToString()).Child(UserID).SetValue(map);

        }

        private void setUpTransitionAnim()
        {
            Explode explode = new Explode();
            explode.ExcludeTarget(Resource.Id.appbar, true);
            Window.ExitTransition = explode;
        }

        private void setUpSlider()
        {
            t = new System.Timers.Timer();
            t.Interval = TimeSpan.FromSeconds(7).TotalMilliseconds;
            t.Elapsed += T_Elapsed;
            CurrentPage1 = 0;
            CurrentPage2 = 0;

            mHRecyclerView = FindViewById<RecyclerView>(Resource.Id.productSlider);
            mSlider1 = FindViewById<ViewPager>(Resource.Id.slidertop);
            mSlider2 = FindViewById<ViewPager>(Resource.Id.sliderbottom);

            mSlider1.AddOnPageChangeListener(this);
            mSlider2.AddOnPageChangeListener(this);

            var layoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
            mHRecyclerView.SetLayoutManager(layoutManager);
        }

        private void SetUpDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.myAccount:
                        myAccount();
                        break;
                    case Resource.Id.myCart:
                        myCart();
                        break;
                    case Resource.Id.myPurchases:
                        myFav();
                        break;
                    case Resource.Id.notification:
                        Notifications();
                        break;
                    case Resource.Id.settings:
                        settings();
                        break;
                    case Resource.Id.faq:
                        FAQ();
                        break;
                    case Resource.Id.logout:
                        LogOut();
                        break;
                    case Resource.Id.myWishlist:
                        WishlistActivity();
                        break;
                    case Resource.Id.about:
                        about();
                        break;
                    default:
                        break;
                }
                e.MenuItem.SetChecked(true);
                mDrawerLayout.CloseDrawers();
            };
        }

        private void WishlistActivity()
        {
            ActivityOptionsCompat options = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent f = new Intent(this, typeof(Activity_Wishlist));
            StartActivity(f, options.ToBundle());
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(new Action(() =>
            { 
                if (CurrentPage1 == mTopList.Count)
                    CurrentPage1 = 0;
                if(CurrentPage2 == mBottList.Count)
                    CurrentPage2 = 0;

                mSlider1.SetCurrentItem(CurrentPage1, true);
                mSlider2.SetCurrentItem(CurrentPage2, true);
                CurrentPage1++;
                CurrentPage2++;
            }));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_notification, menu);
            var item = menu.FindItem(Resource.Id.action_notification);

            if (Session.GetReadedNotifications())
            {
                item.SetIcon(Resource.Drawable.ic_icons8_google_alerts);
            }
            else
            {
                item.SetIcon(Resource.Drawable.ic_notifications_black_24dp);
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;
                case Resource.Id.action_cart:
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    Intent i = new Intent(this, typeof(MyCart_Activity));
                    StartActivity(i, option.ToBundle());
                    return true;
                case Resource.Id.action_notification:
                    Notifications();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            //t.Start();
           SupportActionBar.Title = "";
        }

        private void PullCartFragment()
        {
            ActivityOptionsCompat options = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent f = new Intent(this, typeof(MyCart_Activity));
            StartActivity(f, options.ToBundle());

        }

        private void Notifications()
        {
            ActivityOptionsCompat options = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent f = new Intent(this, typeof(NotificationActivitity));
            StartActivity(f, options.ToBundle());
        }

        //override onn
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.btnReview:
                    ReviewIntent();
                    return true;
                case Resource.Id.btnCall:
                    CallUs();
                    return true;
                case Resource.Id.categories:
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    Intent c = new Intent(this, typeof(Activit_Category));
                    StartActivity(c, option.ToBundle());
                    return true;
                default:
                    break;
            }
            return OnNavigationItemSelected(item);
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
            //addDotsIndicator(position);

        }

        private void addDotsIndicator(int position)
        {
            mDots = new TextView[5];
            mDotsLayout.RemoveAllViews();
            for (int i = 0; i < mDots.Length; i++)
            {
                mDots[i] = new TextView(this);
                mDots[i].TextFormatted = Html.FromHtml("&#8226");
                mDots[i].SetTextSize(Android.Util.ComplexUnitType.Sp, 35);
                mDots[i].SetTextColor(Android.Graphics.Color.ParseColor("#0e63b7"));

                mDotsLayout.AddView(mDots[i]);
            }
            if (mDots.Length > 0)
            {
                mDots[position].SetTextColor(Android.Graphics.Color.ParseColor("#2091EB"));
            }

        }

        private void OnScrollListener_LoadMoreEventAsync(object sender, EventArgs e)
        {
            _index++;
            if (_index < _SubCategory.Count)
            {
                subCat = _SubCategory[_index].SubCategoryID;
                Cat = _SubCategory[_index].CategoryID;
                var trans = _SubCategory[_index].Translation;
                GetTrending(trans,Cat, subCat);
            }  
        }

        protected override void OnPause()
        {
            base.OnPause();
            //t.Stop();
        }

        private void GetTopProducts()
        {
            mTopList = new List<Adverts>();
            mBottList = new List<Adverts>();

            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback f = new FirebaseCallback();
            f.GetAds(database);
            f.Advert += (sender, obj) =>
            {
                var half = obj.Count / 2;

                for (int i = 0; i < half; i++)
                {
                    mTopList.Add(obj[i]);
                    obj.RemoveAt(i);
                }

                for (int i = 0; i < obj.Count; i++)
                {
                    mBottList.Add(obj[i]);
                }

                mSlideAdapter1 = new ImageSliderAdapter(this, mTopList);
                mSlideAdapter2 = new ImageSliderAdapter(this, mBottList);
                mSlider1.Adapter = mSlideAdapter1;
                mSlider2.Adapter = mSlideAdapter2;

                t.Start();
                //Release resources
                database.Dispose();
                f.Dispose();
            };
        }

        private void GetCategories()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;

            FirebaseCallback p = new FirebaseCallback();
            p.GetCategories(database);
            p.Categories += (sender, obj) =>
            {
                var categoriesAdapter = new MultAdapter(this, StructureViewModel(obj));
                //Create our layout manager
                var layout = new GridLayoutManager(this, 3, LinearLayoutManager.Vertical, false);
                layout.SetSpanSizeLookup(new MyGridLayoutManagerSpanSizeLookup(
                    getSpanSizeFunc: position =>
                    {
                        switch (categoriesAdapter.GetItemViewType(position))
                        {
                            case 0:
                                return 1;
                            case 1:
                                return 2;
                            case 2:
                                return 1;
                            default:
                                return -1;
                        }
                    }
                ));
                recyclerViewCat.SetLayoutManager(layout);
                recyclerViewCat.SetAdapter(categoriesAdapter);
                categoriesAdapter.ItemSub += (s, o) =>
                {
                    Intent intent = new Intent(this, typeof(Activity_SubCategory));
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    intent.PutExtra("Category", categoriesAdapter.mCategories[o].Category);
                    intent.PutExtra("CategoryID", categoriesAdapter.mCategories[o].CategoryID);
                    intent.PutExtra("Translation", categoriesAdapter.mCategories[o].Translation);

                    StartActivity(intent, option.ToBundle());

                };
                categoriesAdapter.ItemAll += (s, o) =>
                {
                    Intent intent = new Intent(this, typeof(Activit_Category));
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    StartActivity(intent, option.ToBundle());

                };

                //Release resources
                database.Dispose();
                p.Dispose();
            };
        }

        #region LeftDrawerSelections

        private void myCart()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(MyCart_Activity));
            this.StartActivity(intent, option.ToBundle());
        }

        private void myAccount()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(MyAccount));
            this.StartActivity(intent, option.ToBundle());
        }

        private void FAQ()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(Faq_Activity));
            this.StartActivity(intent, option.ToBundle());
        }

        private void Information()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(Information_Activity));
            this.StartActivity(intent, option.ToBundle());
        }

        private void myFav()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(FavActivity));
            this.StartActivity(intent, option.ToBundle());
        }

        private void settings()
        {
            var check = 0;
            if (_lang == "sw")
            {
                check = 1;
            }

            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.dlg_choose_lang);
            builder.SetSingleChoiceItems(Resource.Array.language_choice, check, delegate (object sender, DialogClickEventArgs e)
            {
                builder.Dispose();
                var language = Resources.GetStringArray(Resource.Array.language_choice);
                selected = language[e.Which];

            });
            builder.SetPositiveButton(Resource.String.dialog_ok, delegate
            {
                builder.Dispose();
                if (selected == "Kiswahili")
                {
                    ISharedPreferencesEditor editor = app.Edit();
                    editor.PutString("Language", "sw");
                    editor.Apply();
                    this.Recreate();
                }
                else
                {
                    ISharedPreferencesEditor editor = app.Edit();
                    editor.PutString("Language", "en");
                    editor.Apply();
                    this.Recreate();

                }
            });
            builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
            {
                builder.Dispose();
            });
            builder.Create().Show();
        }

        private void about()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(CompanyInfo));
            this.StartActivity(intent, option.ToBundle());

        }

        private void ReviewIntent()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(ReviewsActivity));
            this.StartActivity(intent, option.ToBundle());
        }

        #endregion

        private void CallUs()
        {
            string[] nos = new string[]
            {
                "0762719512",
                "0732824282"
            };
            var phone = "";
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.menu_contact_us);
            builder.SetSingleChoiceItems(nos,0, delegate (object sender, DialogClickEventArgs e)
            {
                phone = nos[e.Which];
            });
            builder.SetPositiveButton(Resource.String.btn_call, delegate
             {
                 builder.Dispose();
                 if (!string.IsNullOrEmpty(phone))
                     phone = nos[0];
                 Intent _phone = new Intent(Intent.ActionCall,
                 Android.Net.Uri.Parse(string.Format("tel:{0}", phone)));
                 StartActivity(_phone);
             });
            builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
             { builder.Dispose(); });
            builder.Show();
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

            res.UpdateConfiguration(Config,Dm);
        }

        private void InitDecoration()
        {
            horizontalDecoration = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            Android.Graphics.Drawables.Drawable horizontalDivider = ContextCompat.GetDrawable(this, Resource.Drawable.horizontal_line);
            horizontalDecoration.SetDrawable(horizontalDivider);

            gridlayoutDecoration = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            Android.Graphics.Drawables.Drawable gridlayoutDivider = ContextCompat.GetDrawable(this, Resource.Drawable.grid_decorator);
            gridlayoutDecoration.SetDrawable(horizontalDivider);

        }

        private void MultViewRecyclerView(RecyclerView _recyclerView)
        {
            //Create our layout manager
            DividerItemDecoration d = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            var layout = new GridLayoutManager(this,2, LinearLayoutManager.Vertical, false);
            layout.SetSpanSizeLookup(new MyGridLayoutManagerSpanSizeLookup(
                getSpanSizeFunc: position =>
                {
                    switch (multviewTypeAdapter.GetItemViewType(position))
                    {
                        case 0:
                            return 2;
                        case 1:
                            return 2;
                        case 2:
                            return 2;
                        case 3:
                            return 2;
                        case 4:
                            return 1;
                        default:
                            return -1;
                    }
                }
            ));

            //_recyclerView.AddItemDecoration(new SpacesItemDecoration(3));
            onScrollListener = new XamarinRecyclerViewOnScrollListener(layout);
            onScrollListener.LoadMoreEvent += OnScrollListener_LoadMoreEventAsync;
            _recyclerView.AddOnScrollListener(onScrollListener);
            _recyclerView.SetLayoutManager(layout);

        }

        public void OnItemClicked(int p, ImageView view)
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, view, multviewTypeAdapter.mProduct_Mode[p].ProductID);
            Intent i = new Intent(this, typeof(ActivityDetails));

            i.PutExtra("SellerID", multviewTypeAdapter.mProduct_Mode[p].SellerID);
            i.PutExtra("ProductID", multviewTypeAdapter.mProduct_Mode[p].ProductID);
            i.PutExtra("Product", multviewTypeAdapter.mProduct_Mode[p].Product);
            i.PutExtra("Price", multviewTypeAdapter.mProduct_Mode[p].Price);
            i.PutExtra("Buy_Price", multviewTypeAdapter.mProduct_Mode[p].Buy_Price);
            i.PutExtra("Description", multviewTypeAdapter.mProduct_Mode[p].Description);
            i.PutExtra("Sizes", multviewTypeAdapter.mProduct_Mode[p].Sizes);
            i.PutExtra("Thumbnail_1", multviewTypeAdapter.mProduct_Mode[p].Thumbnail_1);
            i.PutExtra("Thumbnail_2", multviewTypeAdapter.mProduct_Mode[p].Thumbnail_2);
            i.PutExtra("Thumbnail_3", multviewTypeAdapter.mProduct_Mode[p].Thumbnail_3);
            i.PutExtra("Offer", multviewTypeAdapter.mProduct_Mode[p].Offer_Price);
            i.PutExtra("EndDate", multviewTypeAdapter.mProduct_Mode[p].OfferEnds);
            i.PutExtra("Condition", multviewTypeAdapter.mProduct_Mode[p].Condition);

            i.PutExtra("Category", multviewTypeAdapter.mProduct_Mode[p].CategoryID);
            i.PutExtra("SubCategory", multviewTypeAdapter.mProduct_Mode[p].SubCategoryID);
            StartActivity(i, option.ToBundle());

        }

        private void GetSubCategories()
        {
            isLoading = true;
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            FirebaseCallback p2 = new FirebaseCallback();
            p2.Connected(database,UserID);

            p.GetAllSubCategories(database);
            p.SubCategories += (sender, obj) =>
            {
                var data = obj.Where(a => !a.Icon_Thumbnail.Contains("none")).ToList();
                mSubCategoriesAdapter = new SubCategoriesAdapter(this, data);
                mSubCategoriesAdapter.ClickListener += MSubCategoriesAdapter_ClickListener;
                mSubCatRecycler.SetAdapter(mSubCategoriesAdapter);

                _SubCategory = obj;
                _subCat = _SubCategory.OrderByDescending(i => i.Watched).ToList();

                for (int i = 0; i < _subCat.Count; i++)
                {
                    if (i > 14 && i < _subCat.Count)
                    {
                        _subCat.RemoveAt(i);
                    }
                }

                _Pagination?.Invoke(this, null);

                database.Dispose();
                p.Dispose();

                MostViewProducts();
                mContentLoader.Visibility = ViewStates.Invisible;
                mMainContent.Visibility = ViewStates.Visible;
                nestedScrollView.SetOnScrollChangeListener(this);
                mContentLoader.HideShimmerAdapter();

            };
            GetCompanyInfo();
        }

        private void MSubCategoriesAdapter_ClickListener(object sender, int o)
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent intent = new Intent(this, typeof(ShoppingActivity));
            intent.PutExtra("CategoryID", mSubCategoriesAdapter.Product[o].CategoryID);
            intent.PutExtra("Category", mSubCategoriesAdapter.Product[o].SubCategory);
            intent.PutExtra("Translation", mSubCategoriesAdapter.Product[o].Translation);

            intent.PutExtra("SubCategory", mSubCategoriesAdapter.Product[o].SubCategoryID);
            StartActivity(intent, option.ToBundle());

        }

        private void GetTrending(string Translation,string Category,string SubCategory)
        {
            mProgressBar.Visibility = ViewStates.Visible;

            var title = _SubCategory[_index].SubCategory;
            var mPageLimit = 4;
            mPageOffset = 0;

            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();

            p.GetProducts(Category, SubCategory, mPageOffset, mPageLimit, database);
            p.GetProduct += (sender, obj) =>
            {
                mProgressBar.Visibility = ViewStates.Invisible;

                if (obj.Count > 0)
                {
                    var Prod = StructureViewModel(obj,Translation,title, Category, SubCategory);
                    if (multviewTypeAdapter == null)
                    {
                        multviewTypeAdapter = new MultViewAdapter(this, Prod, this);
                        multviewRecyclerView.SetAdapter(multviewTypeAdapter);
                        multviewTypeAdapter.ItemClick += (o, s) =>
                        {
                            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                            Intent intent = new Intent(this, typeof(ShoppingActivity));
                            intent.PutExtra("CategoryID", multviewTypeAdapter.mProduct_Mode[s].CategoryID);
                            intent.PutExtra("SubCategory", multviewTypeAdapter.mProduct_Mode[s].SubCategoryID);
                            intent.PutExtra("Translation", multviewTypeAdapter.mProduct_Mode[s].Translation);

                            intent.PutExtra("Category", multviewTypeAdapter.mProduct_Mode[s].Head);
                            StartActivity(intent, option.ToBundle());
                        };
                    }
                    else
                    {
                        multviewTypeAdapter.AddList(Prod);
                    }
                }
                isLoading = false;
            };
        }

        private List<Categories> StructureViewModel(List<Categories> p)
        {
            var ViewModel = new List<Categories>();

            for (int i = 0; i < p.Count; i++)
            {
                //first row
                if (i == 0)
                {
                    p[i].Columns = Span.ONE;
                    ViewModel.Add(p[i]);
                }
                else if (i == 1)
                {
                    p[i].Columns = Span.TWO;
                    ViewModel.Add(p[i]);
                }
                //second row
                else if ( i == 2)
                {
                    p[i].Columns = Span.TWO;
                    ViewModel.Add(p[i]);
                }
                else if (i  == 3)
                {
                    p[i].Columns = Span.ONE;
                    ViewModel.Add(p[i]);
                }
                else if (i + 1 == p.Count)
                {
                    p[i].Columns = Span.LAST;
                    ViewModel.Add(p[i]);
                }
                else
                {
                    p[i].Columns = Span.ONE;
                    ViewModel.Add(p[i]);
                }

            }
            return ViewModel;
        }

        private List<Products> StructureViewModel(List<Products> p, string Trans, string Category,string CategoryID,string SubCategoryID)
        {
            var ViewModel = new List<Products>();
            var prod = new Products
            {
                Head = Category,
                Translation = Trans,
                SubCategoryID = SubCategoryID,
                CategoryID = CategoryID,
                Style = MyStyles.Head
            };
            ViewModel.Add(prod);
            if (LastStyle == MyStyles.Thumbnail)
            {
                for (int i = 0; i < p.Count; i++)
                {
                    p[i].Style = MyStyles.ListView;
                    ViewModel.Add(p[i]);
                }
                LastStyle = MyStyles.ListView;

                var ads = new Products
                {
                    Style = MyStyles.Ads
                };
                ViewModel.Add(ads);
            }
            else if (LastStyle == MyStyles.ListView)
            {
                for (int i = 0; i < p.Count; i++)
                {

                    p[i].Style = MyStyles.GridView;
                    ViewModel.Add(p[i]);
                }
                LastStyle = MyStyles.GridView;

            }
            else if (LastStyle == MyStyles.GridView)
            {
                for (int i = 0; i < p.Count; i++)
                {

                    p[i].Style = MyStyles.ListView;
                    ViewModel.Add(p[i]);
                }
                LastStyle = MyStyles.ListView;
            }
            else
            {
                for (int i = 0; i < p.Count; i++)
                {

                    p[i].Style = MyStyles.Thumbnail;
                    ViewModel.Add(p[i]);
                }
                LastStyle = MyStyles.Thumbnail;

            }
            return ViewModel;
        }

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    var result = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                    var builder = new Android.App.AlertDialog.Builder(this);
                    builder.SetMessage(result);
                    builder.SetTitle("Google Play Service");
                    builder.SetPositiveButton("Ok", delegate { builder.Dispose(); });
                    builder.Show();
                }
                else
                {
                    var builder = new Android.App.AlertDialog.Builder(this);
                    builder.SetMessage("Sorry, this device is not supported");
                    builder.SetTitle("Google Play Service");
                    builder.SetPositiveButton("Ok", delegate { builder.Dispose(); });
                    builder.Show();

                    Finish();
                }
                return false;
            }
            else
            {
                var builder = new Android.App.AlertDialog.Builder(this);
                builder.SetMessage("Google Play Services is available.");
                builder.SetTitle("Google Play Service");
                builder.SetPositiveButton("Ok", delegate { builder.Dispose(); });
                builder.Show();
                return true;
            }
        }

        public void OnScrollChange(NestedScrollView v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
        {
            if (scrollY ==(v.GetChildAt(0).MeasuredHeight - v.MeasuredHeight) && !isLoading)
            {
                _index++;
                if (_index < _SubCategory.Count)
                {
                    subCat = _SubCategory[_index].SubCategoryID;
                    Cat = _SubCategory[_index].CategoryID;
                    var trans = _SubCategory[_index].Translation;
                    GetTrending(trans,Cat, subCat);
                    isLoading = true;
                }
                else
                    isLoading = true;
            }
            if (scrolledDistance > HIDE_THRESHOLD && controlsVisible)
            {
                hideViews();
                controlsVisible = false;
                scrolledDistance = 0;
            }

            else if (scrolledDistance < -HIDE_THRESHOLD && !controlsVisible)
            {
                showViews();
                controlsVisible = true;
                scrolledDistance = 0;
            }

            var dy = scrollY - oldScrollY;

            if ((controlsVisible && dy > 0) || (!controlsVisible && dy < 0))
            {
                scrolledDistance += dy;
            }

        }

        private void GetCompanyInfo()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;

            FirebaseCallback p = new FirebaseCallback();
            p.GetCompanyInfo(database);
            p.Seller += (sender, obj) =>
            {
                mContentLoader.Visibility = ViewStates.Invisible;
                mMainContent.Visibility = ViewStates.Visible;
                nestedScrollView.SetOnScrollChangeListener(this);
                mContentLoader.HideShimmerAdapter();

                ISharedPreferencesEditor edit = app.Edit();
                edit.PutString("Company", obj[0].Seller);
                edit.PutString("Location", obj[0].Location);
                edit.PutString("Phone1", obj[0].PhoneNo1);
                edit.PutString("Phone2", obj[0].PhoneNo1);
                edit.PutString("Email", obj[0].Email);
                edit.PutString("Thumbnail", obj[0].Thumbnail);
                edit.PutString("Payment", obj[0].PayInfo);
                edit.PutString("Toolbar", obj[0].Titlebar);

                edit.Apply();


                var toolbarLogo = FindViewById<ImageView>(Resource.Id.toolbar_title);
                Glide.With(this).Load(obj[0].Titlebar)
                 .Apply(RequestOptions.PlaceholderOf(Resource.Drawable.title))
                 .Transition(DrawableTransitionOptions.WithCrossFade()).Into(toolbarLogo);

                //Release resources
                database.Dispose();
                p.Dispose();

            };

        }

        private void LogOut()
        {
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.dlg_info);
            builder.SetMessage(Resource.String.dlg_logout_msg);
            builder.SetPositiveButton(Resource.String.dlg_btn_yes, delegate
            {
                builder.Dispose();
                FirebaseMessaging.Instance.UnsubscribeFromTopic(_phonenumber);
                ISharedPreferencesEditor editor = app.Edit();
                editor.Clear();
                editor.Commit();

                Intent intent = new Intent(this, typeof(LoginActivity));
                this.StartActivity(intent);
                this.Finish();
            });
            builder.SetNegativeButton(Resource.String.dlg_btn_no, delegate
             { builder.Dispose(); });
            builder.Show();
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

        private void hideViews()
        {

            CoordinatorLayout.LayoutParams lp = (CoordinatorLayout.LayoutParams)mBottomCardView.LayoutParameters;
            int fabBottomMargin = lp.BottomMargin;
            mBottomCardView.Animate().TranslationY(mBottomCardView.Height + fabBottomMargin).SetInterpolator(new AccelerateInterpolator(2)).Start();
            fab.Show();
            fabReview.Hide();
            //MoveUpFab();
        }

        private void showViews()
        {
            fab.Hide();
            fabReview.Show();
            //MoveDownFab();
            mBottomCardView.Animate().TranslationY(0).SetInterpolator(new DecelerateInterpolator(2)).Start();

        }

        private void MostViewProducts()
        {
            var Category = _subCat[(_subCat.Count - 1)].CategoryID;
            var SubCategory = _subCat[(_subCat.Count - 1)].SubCategoryID;
            var mPageLimit = 1;

            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            mProgressBar.Visibility = ViewStates.Visible;

            p.GetProducts(Category, SubCategory, 0, mPageLimit, database);
            p.GetProduct += (sender, obj) =>
            {
                mProgressBar.Visibility = ViewStates.Invisible;
                if (obj.Count > 0)
                {
                    if(mHAdapter == null)
                    {
                        mHAdapter = new TopViewProd_Adapter(this, obj,this);
                        mHRecyclerView.SetAdapter(mHAdapter);
                    }
                    else
                    {
                        mHAdapter.AddList(obj);
                    }
                }
                try
                {
                    _subCat.RemoveAt(_subCat.Count - 1);
                    MostViewProducts();
                }catch
                {

                }
               
            };

        }

        public void ItemClicked(int p, ImageView Image)
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, Image, mHAdapter.Product[p].ProductID);
            Intent i = new Intent(this, typeof(ActivityDetails));

            i.PutExtra("SellerID", mHAdapter.Product[p].SellerID);
            i.PutExtra("ProductID", mHAdapter.Product[p].ProductID);
            i.PutExtra("Product", mHAdapter.Product[p].Product);
            i.PutExtra("Price", mHAdapter.Product[p].Price);
            i.PutExtra("Buy_Price", mHAdapter.Product[p].Buy_Price);
            i.PutExtra("Description", mHAdapter.Product[p].Description);
            i.PutExtra("Sizes", mHAdapter.Product[p].Sizes);
            i.PutExtra("Thumbnail_1", mHAdapter.Product[p].Thumbnail_1);
            i.PutExtra("Thumbnail_2", mHAdapter.Product[p].Thumbnail_2);
            i.PutExtra("Thumbnail_3", mHAdapter.Product[p].Thumbnail_3);
            i.PutExtra("Offer", mHAdapter.Product[p].Offer_Price);
            i.PutExtra("EndDate", mHAdapter.Product[p].OfferEnds);
            i.PutExtra("Condition", mHAdapter.Product[p].Condition);

            i.PutExtra("Category", mHAdapter.Product[p].CategoryID);
            i.PutExtra("SubCategory", mHAdapter.Product[p].SubCategoryID);
            StartActivity(i, option.ToBundle());

        }

        public void OnItemOptionClicked(int p)
        {
            OptionMenuDialog(p);
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
            //alert.Window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));
            alert.Show();
            btnShare.Click += (o, s) =>
            {
                alert.Dismiss();
                ShareBottomDialog();
            };
            btnWish.Click += (o, s) =>
            {
                alert.Dismiss();
                var product = multviewTypeAdapter.mProduct_Mode[pos].Product;
                var id = multviewTypeAdapter.mProduct_Mode[pos].ProductID;
                var categoryID = multviewTypeAdapter.mProduct_Mode[pos].CategoryID;
                var subcategoryID = multviewTypeAdapter.mProduct_Mode[pos].SubCategoryID;

                HashMap map = new HashMap();
                map.Put("ProductID", id);
                map.Put("CategoryID", categoryID);
                map.Put("SubCategoryID", subcategoryID);

                var data = FirebaseDatabase.Instance;
                var wish = data.Reference;
                wish.Child("Wishlists").Child(UserID).Child(id).SetValue(map);

                var text = Resources.GetString(Resource.String.snkbr_added_wishlist);
                Snackbar.Make(FindViewById(Android.Resource.Id.Content), product + " " + text, Snackbar.LengthShort).Show();
            };

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

        private void ChatWhatsapp()
        {
            PackageManager packageManager = PackageManager;
            Intent i = new Intent(Intent.ActionView);
            var phone = "255754339452";
            var message = string.Empty;
            try
            {
                string url = "https://api.whatsapp.com/send?phone=" + phone + "&text=" + URLEncoder.Encode(message, "UTF-8");
                i.SetPackage("com.whatsapp");
                i.SetData(Android.Net.Uri.Parse(url));
                if (i.ResolveActivity(packageManager) != null)
                {
                    StartActivity(i);
                }
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

        private void MoveDownFab()
        {
            var move = AnimationUtils.LoadAnimation(this, Resource.Animation.hide_fab1);


            CoordinatorLayout.LayoutParams layoutParams = (CoordinatorLayout.LayoutParams)fabReview.LayoutParameters;
            layoutParams.RightMargin += (int)(fabReview.Width * 0.25);
            layoutParams.BottomMargin += (int)(fabReview.Height * 1.7);
            fabReview.LayoutParameters = (layoutParams);
            fabReview.StartAnimation(move);
        }

        private void MoveUpFab()
        {
            var move = AnimationUtils.LoadAnimation(this, Resource.Animation.show_fab1);

            
            CoordinatorLayout.LayoutParams layoutParams = (CoordinatorLayout.LayoutParams)fabReview.LayoutParameters;
            layoutParams.RightMargin += (int)(fabReview.Width * 0.25);
            layoutParams.BottomMargin += (int)(fabReview.Height * 1.7);
            fabReview.LayoutParameters = (layoutParams);
            fabReview.StartAnimation(move);
        }

        public override void OnBackPressed()
        {
            if (doubleBackToExit)
            {
                Finish();
                return;
            }
            doubleBackToExit = true;
            Toast.MakeText(this, Resource.String.title_close_app, ToastLength.Short).Show();
            new Handler().PostDelayed(new Runnable(new Action(() =>
                {
                    doubleBackToExit = false;
                })),4000);
        }
    }
    public enum MyStyles
    {
        Head = 0,
        ListView = 1,
        GridView = 2,
        Thumbnail = 3,
        Ads = 4 
    }
}

