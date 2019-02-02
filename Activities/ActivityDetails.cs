using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Graphics;
using Safari_Shopping_Mall.Accessors;
using Android.Support.V4.View;
using System.Net.Http;
using Safari_Shopping_Mall.Adapters;
using Android.Text;
using Android.Support.Design.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using System.Timers;
using Android.Support.V4.App;
using Java.Util;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Refractored.Controls;
using Android.Support.V4.Widget;
using System.Linq;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Support.V4.Content;
using Java.Net;
using Android.Views.Animations;
using Safari_Shopping_Mall.Fragments;
using System.Threading.Tasks;
using Com.Bumptech.Glide.Load;
using Com.Bumptech.Glide.Load.Engine;
using Com.Bumptech.Glide.Request.Target;
using Java.Lang;
using Android.Support.V7.Graphics;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/Details")]
    public class ActivityDetails : AppCompatActivity,ViewTreeObserver.IOnScrollChangedListener, Palette.IPaletteAsyncListener
        , ViewPager.IOnPageChangeListener,View.IOnClickListener,NestedScrollView.IOnScrollChangeListener, IRequestListener
    {
        #region Variables
        SqlLiteSession S;
        HttpClient client = new HttpClient();
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private bool IsEditReviewVisible = false;
        private NestedScrollView ScrollView;
        private CardView mEditReview;
        private RecyclerView recyclerView;
        private Review_Adapter Adapter;
        private int ScreenSizeHeight;
        private const int FULL_VISIBLE_AT = 2;
        private Products _products;
        private TextView Product;
        private TextView Description;
        private TextView Price;
        private TextView Sizes;
        private TextView Colors;
        private TextView Condition;
        private TextView Location;
        private TextView Phone1;
        private TextView Email;
        private TextView Seller;
        private SqlLiteSession Broker = new SqlLiteSession();
        private bool controlsVisible = true;
        private const int HIDE_THRESHOLD = 20;

        private LinearLayout mDotsLayout;
        private ViewPager mSlideViewpager;
        private List<Images> img;
        private SlideAdapter_Details sliderAdapter;
        private TextView[] mDots;
        private List<Reviews> reviews;
        private int LastLoad;
        private ProgressBar progressBar;
        private CoordinatorLayout layout;
        private ProgressbarSetup progressbar;
        private event EventHandler GetReview;
        private System.Timers.Timer t;
        private int cart_count;
        private string _lang;
        private ImageView mSellerImage;
        public int PageNo { get; private set; }
        public XamarinRecyclerViewOnScrollListener onScrollListener { get; private set; }
        public int CurrentPage { get; private set; }
        public string UserID { get; private set; }
        public TextView PriceTitle { get; private set; }
        public TextView PriceOff { get; private set; }
        public ImageButton BtnOption { get; private set; }
        public int alpha { get; private set; }
        public int red { get; private set; }
        public int blue { get; private set; }
        public int green { get; private set; }

        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private FloatingActionButton fab;
        private string _username;
        private TextView txtProductTitle;
        private TextView txtPriceTitle;
        private TextView textCartItemCount;
        private int mCartItemCount;
        private ImageView _CartItemCount;
        private TextView Phone2;
        private string _phone1;
        private string _phone2;
        private BottomSheetDialog mBottomSheetDialog;
        private DividerItemDecoration horizontalDecoration;
        private int scrolledDistance = 0;
        private FloatingActionButton fabShare;
        private FloatingActionButton fabWhatsapp;
        private FloatingActionButton fabReview;
        private TextView txtOff;
        private RelativeLayout mBottomButtons;
        private TextView OffPercent;
        private Button AddToCart;
        private Button Buy;
        private Color colorAlpha;
        private Color colorDark;
        private ImageView imgView;
        private TextView header;


        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            red = 0; green = 0;blue = 0;
            Display display = WindowManager.DefaultDisplay;
            Point size = new Point();
            display.GetSize(size);
            ScreenSizeHeight = size.Y;

            UserID = app.GetString("USERID", string.Empty);
            _username = app.GetString("Fullname", string.Empty);
            _lang = app.GetString("Language", "en");
            _phone1 = app.GetString("Phone1",string.Empty);
            _phone2 = app.GetString("Phone2", string.Empty);
            ChangeLanguage(_lang);

            //Postpone transition to give time to load all reasorces
            SupportPostponeEnterTransition();

            //Set Statusbar transparent
            base.OnCreate(savedInstanceState);
            S = new SqlLiteSession();

            // Create your application here
            SetContentView(Resource.Layout.activity_product_details);
           
            ScrollView = FindViewById<NestedScrollView>(Resource.Id.scrollView);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "";
            ScrollView.ViewTreeObserver.AddOnScrollChangedListener(this);
            ScrollView.SetOnScrollChangeListener(this);

            InitDecoration();

            AddToCart = FindViewById<Button>(Resource.Id.btnAddCart);
            Buy = FindViewById<Button>(Resource.Id.btnBuy);
            Product = FindViewById<TextView>(Resource.Id.txtProduct);
            Description = FindViewById<TextView>(Resource.Id.txtDescription);
            Price = FindViewById<TextView>(Resource.Id.txtPrice);
            Sizes = FindViewById<TextView>(Resource.Id.txtSizes);
            PriceTitle = FindViewById<TextView>(Resource.Id.txtPriceHead);
            PriceOff = FindViewById<TextView>(Resource.Id.txtOffHead);
            OffPercent = FindViewById<TextView>(Resource.Id.txtOffPercent);
            header = FindViewById<TextView>(Resource.Id.txtHeader);

            txtOff = FindViewById<TextView>(Resource.Id.txtOff);
            Condition = FindViewById<TextView>(Resource.Id.txtCondition);

            mSellerImage = FindViewById<ImageView>(Resource.Id.imageView2);
            Location = FindViewById<TextView>(Resource.Id.txtLocation);
            Phone1 = FindViewById<TextView>(Resource.Id.txtContacts);
            Email = FindViewById<TextView>(Resource.Id.txtEmail);

            fabWhatsapp = FindViewById<FloatingActionButton>(Resource.Id.fabWhatsapp);
            fabReview = FindViewById<FloatingActionButton>(Resource.Id.fabcomment);
            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            BtnOption = FindViewById<ImageButton>(Resource.Id.btnMore);

            BtnOption.Click += (o, s) =>
            {
                OptionMenuDialog(0);
            };

            fabReview.Click += FabReview_Click;
            fabWhatsapp.Click += FabWhatsapp_Click;
 
            mDotsLayout = FindViewById<LinearLayout>(Resource.Id.dotslayout);
            mSlideViewpager = FindViewById<ViewPager>(Resource.Id.productSlider);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            ImageButton btnPost = FindViewById<ImageButton>(Resource.Id.btnPost);
            mBottomButtons = FindViewById<RelativeLayout>(Resource.Id.bottomButtons);

            GetDetailsBind();
            mSlideViewpager.AddOnPageChangeListener(this);

            SupportStartPostponedEnterTransition();

            Buy.Click += (obj, sender) =>
            {
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetCanceledOnTouchOutside(false);
                progressDialog.SetMessage(Resources.GetString(Resource.String.prgdlg));
                progressDialog.Show();

                FirebaseDatabase database = FirebaseDatabase.Instance;
                FirebaseCallback firebase = new FirebaseCallback();

                firebase.CheckIfCanBuy(database);
                firebase.IsReady += (o, s) =>
                {
                    progressDialog.Hide();
                    if (!s)
                    {
                        var builder = new Android.App.AlertDialog.Builder(this);
                        builder.SetTitle(Resource.String.dlg_info);
                        builder.SetMessage(Resource.String.dlg_service_not_ready);
                        builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                        {
                            builder.Dispose();
                        });
                        builder.Show();
                    }
                    else
                    {
                        ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                        Intent i = new Intent(this, typeof(CustomizeActivity));
                        i.PutExtra("SellerID", _products.SellerID);
                        i.PutExtra("ProductID", _products.ProductID);
                        i.PutExtra("Price", _products.Price);
                        i.PutExtra("Sizes", _products.Sizes);
                        i.PutExtra("Product", _products.Product);
                        i.PutExtra("Buy_Price", _products.Buy_Price);
                        i.PutExtra("Offer", _products.Offer_Price);
                        i.PutExtra("EndDate", _products.OfferEnds);
                        i.PutExtra("Condition", _products.Condition);

                        i.PutExtra("Description", _products.Description);
                        i.PutExtra("Action", "Buy");

                        i.PutExtra("Thumbnail_1", _products.Thumbnail_1);
                        i.PutExtra("Thumbnail_2", _products.Thumbnail_2);
                        i.PutExtra("Thumbnail_3", _products.Thumbnail_3);
                        StartActivity(i, option.ToBundle());
                    }
                };

            };
            LastLoad = 0;
            PageNo = 0;
            addDotsIndicator(position: 0);
            mSlideViewpager.AddOnPageChangeListener(this);

            SetUpRecyclerView(recyclerView);

            progressbar = new ProgressbarSetup();
            AddToCart.Click += AddToCart_Click;
            fab.Click += (obj, s) =>
            {
                CallUs();
            };

            GetReviews();
        }

        private void FabWhatsapp_Click(object sender, EventArgs e)
        {
            ChatWhatsapp();
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

        private void FabReview_Click(object sender, EventArgs e)
        {
            CommentsBottomDialog();
        }

        private void CallUs()
        {
            string[] nos = new string[]
            {
                _phone1,
                _phone2
            };
            var phone = "";
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.make_a_call);
            builder.SetSingleChoiceItems(nos, 0, delegate (object sender, DialogClickEventArgs e)
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

        private void PostReviews(string userID, string productID, string review)
        {
            var r = new Reviews();
            r.UserID = userID;
            r.Reviewed_Product = productID;
            r.User = _username;
            r.Review = review;
            r.Post_Date = DateTime.Now.ToShortDateString();

            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.PostReview(database,r);
            Snackbar.Make(FindViewById(Android.Resource.Id.Content), "Thanks for your feedback",
            Snackbar.LengthShort).Show();

        }

        private void AddToCart_Click(object sender, EventArgs e)
        {
            if (app.GetBoolean("show_cart_msg",false) && _lang == "sw")
            {
                View view = LayoutInflater.Inflate(Resource.Layout.info_remainder, null, false);
                CheckBox mCheckBox = view.FindViewById<CheckBox>(Resource.Id.checkbox);
                var builder = new Android.App.AlertDialog.Builder(this);
                builder.SetView(view);
                builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                {
                    builder.Dispose();
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    Intent i = new Intent(this, typeof(CustomizeActivity));
                    i.PutExtra("SellerID", _products.SellerID);
                    i.PutExtra("ProductID", _products.ProductID);
                    i.PutExtra("Price", _products.Price);
                    i.PutExtra("Sizes", _products.Sizes);
                    i.PutExtra("Product", _products.Product);
                    i.PutExtra("Description", _products.Description);
                    i.PutExtra("Buy_Price", _products.Buy_Price);
                    i.PutExtra("Offer", _products.Offer_Price);
                    i.PutExtra("EndDate", _products.OfferEnds);
                    i.PutExtra("Condition", _products.Condition);

                    i.PutExtra("Action", "Cart");

                    i.PutExtra("Thumbnail_1", _products.Thumbnail_1);
                    i.PutExtra("Thumbnail_2", _products.Thumbnail_2);
                    i.PutExtra("Thumbnail_3", _products.Thumbnail_3);

                    StartActivity(i, option.ToBundle());
                });
                builder.Create().Window.RequestFeature(WindowFeatures.NoTitle);
                builder.Show();
                mCheckBox.CheckedChange += (obj, s) =>
                {
                    ISharedPreferencesEditor edit = app.Edit();
                    if (mCheckBox.Checked)
                    {
                        edit.PutBoolean("show_cart_msg", false);
                    }
                    else
                    {
                        edit.PutBoolean("show_cart_msg", true);
                    }
                    edit.Apply();

                };


            }
            else
            {
                ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                Intent i = new Intent(this, typeof(CustomizeActivity));
                i.PutExtra("SellerID", _products.SellerID);
                i.PutExtra("ProductID", _products.ProductID);
                i.PutExtra("Price", _products.Price);
                i.PutExtra("Sizes", _products.Sizes);
                i.PutExtra("Product", _products.Product);
                i.PutExtra("Description", _products.Description);
                i.PutExtra("Buy_Price", _products.Buy_Price);
                i.PutExtra("Offer", _products.Offer_Price);
                i.PutExtra("EndDate", _products.OfferEnds);
                i.PutExtra("Condition", _products.Condition);

                i.PutExtra("Action", "Cart");

                i.PutExtra("Thumbnail_1", _products.Thumbnail_1);
                i.PutExtra("Thumbnail_2", _products.Thumbnail_2);
                i.PutExtra("Thumbnail_3", _products.Thumbnail_3);

                StartActivity(i, option.ToBundle());

            }
        }

        private void addDotsIndicator(int position)
        {
            var images = new List<string>();
            var cnt = 0;
            images.Add(Intent.GetStringExtra("Thumbnail_1"));
            images.Add(Intent.GetStringExtra("Thumbnail_2"));
            images.Add(Intent.GetStringExtra("Thumbnail_3"));

            for (int i = 0; i < images.Count; i++)
            {
                if (!images[i].Contains("none"))
                {
                    cnt++;
                }
            }

            mDots = new TextView[cnt];
            mDotsLayout.RemoveAllViews();
            for (int i = 0; i < mDots.Length; i++)
            {
                mDots[i] = new TextView(this);
                mDots[i].TextFormatted = Html.FromHtml("&#8226");
                mDots[i].SetTextSize(Android.Util.ComplexUnitType.Sp, 35);
                mDots[i].SetTextColor(Color.ParseColor("#64f49b02"));

                mDotsLayout.AddView(mDots[i]);
            }
            if (mDots.Length > 0)
            {
                mDots[position].SetTextColor(Color.ParseColor("#f49b02"));
            }
        }

        private void addDotsIndicator(int position,Color colorAlpha, Color colorDark)
        {
            var images = new List<string>();
            var cnt = 0;
            images.Add(Intent.GetStringExtra("Thumbnail_1"));
            images.Add(Intent.GetStringExtra("Thumbnail_2"));
            images.Add(Intent.GetStringExtra("Thumbnail_3"));

            for (int i = 0; i < images.Count; i++)
            {
                if (!images[i].Contains("none"))
                {
                    cnt++;
                }
            }

            mDots = new TextView[cnt];
            mDotsLayout.RemoveAllViews();
            for (int i = 0; i < mDots.Length; i++)
            {
                mDots[i] = new TextView(this);
                mDots[i].TextFormatted = Html.FromHtml("&#8226");
                mDots[i].SetTextSize(Android.Util.ComplexUnitType.Sp, 35);
                mDots[i].SetTextColor(colorAlpha);

                mDotsLayout.AddView(mDots[i]);
            }
            if (mDots.Length > 0)
            {
                mDots[position].SetTextColor(colorDark);
            }
        }

        public void OnScrollChanged()
        {
            if (red != 0 || green != 0 || blue != 0)
            {
                mToolbar.SetBackgroundColor(Color.Argb((int)(GetOpacity() * 225), red, green, blue));
            }
            else
            {
                mToolbar.SetBackgroundColor(Color.Argb((int)(GetOpacity() * 225), 0, 0, 36));
            }
            mToolbar.SetTitleTextColor(Color.Argb((int)(GetOpacity() * 225), 225, 255, 255));

        }

        private float GetOpacity()
        {
            float fullVisibleAtPx = ScreenSizeHeight / FULL_VISIBLE_AT;
            float alpha = ScrollView.ScrollY / fullVisibleAtPx;

            if (alpha > 1)
            {
                return 1;
            }
            else if(alpha < 0)
            {
                return 0;
            }
            return ScrollView.ScrollY / fullVisibleAtPx;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_product_details, menu);
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
                case Resource.Id.action_share:
                    ShareBottomDialog();
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

        private void IntentSearch()
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent i = new Intent(this, typeof(ActivitySearch));
            StartActivity(i, option.ToBundle());
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
            if (red != 0 && green != 0 && blue != 0)
            {
                addDotsIndicator(position,colorAlpha,colorDark);
            }
            else
            {
                addDotsIndicator(position);
            }            //GetDominantColor(sliderAdapter.images[position]);
        }

        private void InitDecoration()
        {
            horizontalDecoration = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            Drawable horizontalDivider = ContextCompat.GetDrawable(this, Resource.Drawable.horizontal_line);
            horizontalDecoration.SetDrawable(horizontalDivider);
        }

        private void SetUpRecyclerView(RecyclerView _recyclerView)
        {
            //Create our layout manager
            _recyclerView.AddItemDecoration(horizontalDecoration);

            recyclerView.HasFixedSize = true;
            var layoutManager = new LinearLayoutManager(this);

            onScrollListener = new XamarinRecyclerViewOnScrollListener(layoutManager);
            onScrollListener.LoadMoreEvent += OnScrollListener_LoadMoreEventAsync;
            recyclerView.AddOnScrollListener(onScrollListener);
            recyclerView.SetLayoutManager(layoutManager);
        }

        private  void OnScrollListener_LoadMoreEventAsync(object sender, EventArgs e)
        {
            RelativeLayout.LayoutParams layout = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
            layout.AddRule(LayoutRules.AlignParentBottom | LayoutRules.CenterHorizontal);
            progressBar.LayoutParameters = layout;
            //Load more stuff here
            onScrollListener.IsLoading = true;
            HideShowProgress();


            HideShowProgress();
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

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {

            RunOnUiThread(() =>
            {
                if (CurrentPage == 3)
                {
                    CurrentPage = 0;
                    mSlideViewpager.SetCurrentItem(CurrentPage++, true);
                }
                else
                {
                    mSlideViewpager.SetCurrentItem(CurrentPage++, true);
                }
            });
        }

        private void GetDetailsBind()
        {
            _products = new Products();

            _products.SellerID = Intent.GetStringExtra("SellerID");
            _products.ProductID = Intent.GetStringExtra("ProductID");
            _products.Product = Intent.GetStringExtra("Product");

            _products.Description = Intent.GetStringExtra("Description");
            _products.Price = Intent.GetDoubleExtra("Price", 0);
            _products.Offer_Price = Intent.GetDoubleExtra("Offer", 0);
            _products.OfferEnds = Intent.GetStringExtra("EndDate");
            _products.Condition = Intent.GetStringExtra("Condition");

            _products.Sizes = Intent.GetStringExtra("Sizes");

            _products.Thumbnail_1 = Intent.GetStringExtra("Thumbnail_1");
            _products.Thumbnail_2 = Intent.GetStringExtra("Thumbnail_2");
            _products.Thumbnail_3 = Intent.GetStringExtra("Thumbnail_3");

            _products.CategoryID = Intent.GetStringExtra("Category");
            _products.SubCategoryID = Intent.GetStringExtra("SubCategory");

            mSlideViewpager.TransitionName = _products.ProductID;
            SupportActionBar.Title = _products.Product;
            ///Setup slider
            var img = new List<string>();
            
            img.Add(_products.Thumbnail_1);
            img.Add(_products.Thumbnail_2);
            img.Add(_products.Thumbnail_3);
            var images = img.Where(a => !a.Contains("none")).ToList();

            sliderAdapter = new SlideAdapter_Details(this, images);
            //GetDominantColor(sliderAdapter.images[0]);

            mSlideViewpager.Adapter = sliderAdapter;
            sliderAdapter.ItemClick += (o, s) =>
            {
                var trans = SupportFragmentManager.BeginTransaction();
                ImagePreview previewer = new ImagePreview();
                previewer.SetStyle((int)DialogFragmentStyle.NoFrame, Android.Resource.Style.ThemeBlackNoTitleBarFullScreen);
                previewer.Show(trans,"TAG");
            };

            //Bind Details
            Glide.With(this).Load(_products.Seller_Thumbnail).Apply(RequestOptions.OverrideOf(500, 500).CircleCrop()).Into(mSellerImage);
            CircleImageView mPesa = FindViewById<CircleImageView>(Resource.Id.mpesa);
            CircleImageView mTigoPesa = FindViewById<CircleImageView>(Resource.Id.tigopesa);
            CircleImageView mHalopesa = FindViewById<CircleImageView>(Resource.Id.halopesa);
            CircleImageView mAirtel = FindViewById<CircleImageView>(Resource.Id.airtelmoney);
            CircleImageView mTpesa = FindViewById<CircleImageView>(Resource.Id.tpesa);
            CircleImageView mZantel = FindViewById<CircleImageView>(Resource.Id.zantel);


            Glide.With(this).Load(Resource.Drawable.mpesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mPesa);
            Glide.With(this).Load(Resource.Drawable.tigopesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mTigoPesa);
            Glide.With(this).Load(Resource.Drawable.halopesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mHalopesa);
            Glide.With(this).Load(Resource.Drawable.airtelmoney).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mAirtel);
            Glide.With(this).Load(Resource.Drawable.tpesa).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mTpesa);
            Glide.With(this).Load(Resource.Drawable.easymoney).Apply(RequestOptions.OverrideOf(90, 150).CenterInside()).Into(mZantel);
            var txtOff2 = FindViewById<TextView>(Resource.Id.txtOff2);
            var txtProduct2 = FindViewById<TextView>(Resource.Id.txtProduct2);
            var imageView = FindViewById<ImageView>(Resource.Id.imageView);
            var txtcost = FindViewById<TextView>(Resource.Id.textCost);


            if (_products.Offer_Price > 0 && ValidateOffer(_products.OfferEnds))
            {
                Price.Text = "TSh " + ThousandsSeparator(_products.Offer_Price.ToString());
                txtcost.Text = "TSh " + ThousandsSeparator(_products.Offer_Price.ToString());
                PriceTitle.Text = "TSh " + ThousandsSeparator(_products.Offer_Price.ToString());

                txtOff2.Text = "TSh " + ThousandsSeparator(_products.Price.ToString());
                PriceOff.Text = "TSh " + ThousandsSeparator(_products.Price.ToString());

                txtOff.Text = "TSh " + ThousandsSeparator(_products.Price.ToString());
                OffPercent.Text = GetString(Resource.String.title_disc) + " " +  PercentOf(_products.Price, _products.Price - _products.Offer_Price);
                PriceOff.PaintFlags = (PriceOff.PaintFlags | PaintFlags.StrikeThruText);
                PriceOff.Visibility = ViewStates.Visible;

                txtOff.PaintFlags = (PriceOff.PaintFlags | PaintFlags.StrikeThruText);
                txtOff2.PaintFlags = (txtOff2.PaintFlags | PaintFlags.StrikeThruText);

                txtOff.Visibility = ViewStates.Visible;
                txtOff2.Visibility = ViewStates.Visible;
                OffPercent.Visibility = ViewStates.Visible;
            }
            else
            {
                PriceTitle.Text = "TSh " + ThousandsSeparator(_products.Price.ToString());
                Price.Text = "TSh " + ThousandsSeparator(_products.Price.ToString());
                txtcost.Text = "TSh " + ThousandsSeparator(_products.Price.ToString());

                PriceOff.Visibility = ViewStates.Gone;
                txtOff.Visibility = ViewStates.Gone;
                txtOff2.Visibility = ViewStates.Gone;
                OffPercent.Visibility = ViewStates.Gone;

            }

            Product.Text = _products.Product;
            header.Text = _products.Product;

            Description.Text = _products.Description;
            Sizes.Text = _products.Sizes;
            Condition.Text = _products.Condition;

            txtProduct2.Text = _products.Product;
            var company = app.GetString("Company", string.Empty);
            var location = app.GetString("Location", string.Empty);
            var phone1 = app.GetString("Phone1", string.Empty);
            var phone2 = app.GetString("Phone2", string.Empty);
            var email = app.GetString("Email", string.Empty);
            var thumbnail = app.GetString("Thumbnail", string.Empty);

            //Seller.Text = company;
            Phone1.Text = phone1 + " | " + phone2;
            Email.Text = email;
            Location.Text = location;

            var wanunuziLogo = app.GetString("Thumbnail", string.Empty);
            Glide.With(this).Load(thumbnail).Apply(RequestOptions.OverrideOf(90, 90).CenterInside()).Into(mSellerImage);

            //Clear adapter
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.CountWatchedProduct(database, _products.CategoryID, _products.SubCategoryID, _products.ProductID);
            Glide.With(this).Load(images[0]).Apply(RequestOptions.OverrideOf(400, 400)).Listener(this).Into(imageView);
        }

        public void OnClick(View v)
        {
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent i = new Intent(this, typeof(MyCart_Activity));
            StartActivity(i, option.ToBundle());
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

        private class Results
        {
            public string Result { get; set; }
        }

        private void GetReviews()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.GetReview(database,_products.ProductID);
            p.Reviews += (ob, s) =>
            {
                Adapter = new Review_Adapter(this, s);
                recyclerView.SetAdapter(Adapter);
            };
        }

        private void GetPurchase()
        {
            //FirebaseDatabase database = FirebaseDatabase.Instance;
            //FirebaseCallback p = new FirebaseCallback();
            //_products.UserID = UserID;
            //p.GetPurchased(database,_products);
            //p.GetProduct += (sender, obj) =>
            //{
            //    if(obj.Count > 0)
            //    {
            //        Infor_Dialog();
            //    }
            //};
        }

        private void Infor_Dialog()
        {
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetMessage("Important");
            builder.SetMessage(Resource.String.dlg_purchases_info);
            builder.SetPositiveButton(Resource.String.dlg_btn_continue, delegate
            {
                builder.Dispose();
                ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                Intent i = new Intent(this, typeof(CustomizeActivity));
                i.PutExtra("ProductID", _products.ProductID);
                i.PutExtra("Price", _products.Price);
                i.PutExtra("Sizes", _products.Sizes);
                i.PutExtra("Product", _products.Product);
                i.PutExtra("Description", _products.Description);
                i.PutExtra("Currency", _products.Currency);
                i.PutExtra("Action", "Buy");

                i.PutExtra("Thumbnail_1", _products.Thumbnail_1);
                i.PutExtra("Thumbnail_2", _products.Thumbnail_2);
                i.PutExtra("Thumbnail_3", _products.Thumbnail_3);

                i.PutExtra("Condition", _products.Condition);
                i.PutExtra("Seller_Thumbnail", _products.Seller_Thumbnail);

                i.PutExtra("Seller", _products.Seller);
                i.PutExtra("Seller_Mobile_1", _products.Seller_Mobile_1);
                i.PutExtra("Seller_Mobile_2", _products.Seller_Mobile_2);
                i.PutExtra("Seller_Email", _products.Seller_Email);
                i.PutExtra("Location_Adrress", _products.Location_Adrress);

                StartActivity(i, option.ToBundle());
            });
            builder.SetNegativeButton(Resource.String.dialog_cancel,delegate
            {
                builder.Dispose();
            });
            builder.Show();
        }

        protected override void OnResume()
        {
            base.OnResume();
            InvalidateOptionsMenu();
        }

        public string ThousandsSeparator(string input)
        {
            var old = input.ToArray();
            var thousands = old.Length / 3;
            var newValue = input;
            var sep = 3;
            for (int i = 0; i < thousands; i++)
            {
                if (sep < old.Length)
                    newValue = newValue.Insert(old.Length - sep, ",");
                sep = sep + 3;
            }
            return newValue;
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

        private void CommentsBottomDialog()
        {
            View bottomSheetLayout = LayoutInflater.Inflate(Resource.Layout.btm_comments_dlg, null);
            (bottomSheetLayout.FindViewById(Resource.Id.button_close)).Click += (o, s) =>
            {
                //Close
                mBottomSheetDialog.Dismiss();
            };
            (bottomSheetLayout.FindViewById(Resource.Id.button_done)).Click += (o, s) =>
            {
                //post
                mBottomSheetDialog.Dismiss();
                var mReview = bottomSheetLayout.FindViewById<EditText>(Resource.Id.edtReview);
                if (!string.IsNullOrEmpty(mReview.Text))
                {
                    PostReviews(UserID, _products.ProductID, mReview.Text);
                    mReview.Text = string.Empty;
                }
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
        public void OnScrollChange(NestedScrollView v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
        {
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

        public bool ValidateOffer(string date)
        {
            try
            {
                var endDate = DateTime.FromBinary(long.Parse(date));
                var startDate = DateTime.Today;
                var diff = (endDate.Date - startDate.Date).Days;

                if (diff > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {

                return false;
            }
        }

        private void showViews()
        {
            fabReview.Show();
            fabWhatsapp.Hide();
            fab.Show();

            mBottomButtons.Animate().TranslationY(0).SetInterpolator(new DecelerateInterpolator(2)).Start();

        }

        private void hideViews()
        {
            fabReview.Hide();
            fabWhatsapp.Show();
            fab.Hide();


            CoordinatorLayout.LayoutParams lp = (CoordinatorLayout.LayoutParams)mBottomButtons.LayoutParameters;
            int fabBottomMargin = lp.BottomMargin;
            mBottomButtons.Animate().TranslationY(mBottomButtons.Height + fabBottomMargin).SetInterpolator(new AccelerateInterpolator(2)).Start();

        }

        private void OptionMenuDialog(int pos)
        {
            var optionMenu = LayoutInflater.Inflate(Resource.Layout.menu_dialog, null, false);
            var btnShare = optionMenu.FindViewById<Button>(Resource.Id.btnShare);
            var btnWish = optionMenu.FindViewById<Button>(Resource.Id.btnWishlist);

            var alert = new Android.App.AlertDialog.Builder(this).Create();
            alert.SetView(optionMenu);
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
                var product = _products.Product;
                var id = _products.ProductID;
                var categoryID = _products.CategoryID;
                var subcategoryID = _products.SubCategoryID;

                HashMap map = new HashMap();
                map.Put("ProductID", id);
                map.Put("CategoryID", categoryID);
                map.Put("SubCategoryID", subcategoryID);

                var db = FirebaseDatabase.Instance;
                var _ref = db.GetReference("Wishlists");
                _ref.Child(UserID).Child(id).SetValue(map);

                var text = Resources.GetString(Resource.String.snkbr_added_wishlist);
                Snackbar.Make(FindViewById(Android.Resource.Id.Content), product + " " + text, Snackbar.LengthShort).Show();
            };

        }

        public string PercentOf(double price, double discountPrice)
        {
            var discount = price - discountPrice;
            var rem1 = price / 100;

            var percent = (int)(discountPrice / rem1);

            return "-" + percent + "%";
        }

        private void GetDominantColor(string thumbnail)
        {

            imgView = FindViewById<ImageView>(Resource.Id.framelayout_bottom_sheet);
            CoordinatorLayout.LayoutParams lp = (CoordinatorLayout.LayoutParams)imgView.LayoutParameters;
            int fabBottomMargin = lp.BottomMargin;
            imgView.Animate().TranslationY(mBottomButtons.Height + fabBottomMargin).SetInterpolator(new AccelerateInterpolator(1)).Start();
            Glide.With(this).Load(thumbnail).Listener(this).Into(imgView);

        }

        public bool OnLoadFailed(GlideException p0, Java.Lang.Object p1, ITarget p2, bool p3)
        {
            //throw new NotImplementedException();\
            return false;
        }

        public bool OnResourceReady(Java.Lang.Object p0, Java.Lang.Object p1, ITarget p2, DataSource p3, bool p4)
        {
            var bitmap = ((BitmapDrawable)p0).Bitmap;

            PaletteColor(bitmap);
            return false;
        }

        public void PaletteColor(Bitmap bitmap)
        {
            Palette.From(bitmap).Generate(this);
        }

        public void OnGenerated(Palette palette)
        {
            Palette.Swatch color = null;
            if(palette.DarkVibrantSwatch != null)
            {
                color = palette.DarkVibrantSwatch;
            }
            else if (palette.VibrantSwatch != null)
            {
                color = palette.VibrantSwatch;
            }
            else if (palette.DarkMutedSwatch != null)
            {
                color = palette.DarkMutedSwatch;
            }
            else if (palette.MutedSwatch != null)
            {
                color = palette.MutedSwatch;
            }
            else
            {
                return;
            }
            alpha = Color.GetAlphaComponent(color.Rgb);
            red = Color.GetRedComponent(color.Rgb);
            blue = Color.GetBlueComponent(color.Rgb);
            green = Color.GetGreenComponent(color.Rgb);

            Buy.SetBackgroundColor(Color.Argb(alpha, red, green, blue));
            fab.BackgroundTintList = ColorStateList.ValueOf(Color.Argb(alpha, red, green, blue));
            fabReview.BackgroundTintList = ColorStateList.ValueOf(Color.Argb(alpha, red, green, blue));
            Window.SetStatusBarColor(Color.Argb(alpha, red, green, blue));
            mToolbar.SetBackgroundColor(Color.Argb(0, red, green, blue));
            AddToCart.SetTextColor(Color.Argb(alpha, red, green, blue));
            RelativeLayout block = FindViewById<RelativeLayout>(Resource.Id.linearlayout);
            block.SetBackgroundColor(Color.Argb(255, red, green, blue));

            LayerDrawable layerDrawable = (LayerDrawable)ContextCompat.GetDrawable(this, Resource.Drawable.btnflat);
            GradientDrawable gradientDrawable = (GradientDrawable)layerDrawable.FindDrawableByLayerId(Resource.Id.border);
            gradientDrawable.SetStroke(2, ColorStateList.ValueOf(Color.Argb(alpha, red, green, blue)));

            colorAlpha = Color.Argb(170, red, green, blue);
            colorDark = Color.Argb(alpha, red, green, blue);
            addDotsIndicator(0, colorAlpha, colorDark);
        }
    }
}