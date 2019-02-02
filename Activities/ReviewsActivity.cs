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
using Safari_Shopping_Mall.Helpers;
using Firebase.Database;
using Safari_Shopping_Mall.Adapters;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Views.Animations;
using Android.Support.Design.Widget;
using Safari_Shopping_Mall.Accessors;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class ReviewsActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private RecyclerView mRecyclerView;
        private ProgressBar progressBar;
        private XamarinRecyclerViewOnScrollListener onScrollListener;
        private int mPageOffset = 0;
        private Review_Adapter mAdapter;
        private DividerItemDecoration horizontalDecoration;
        private CardView mBottomCardView;
        private int LastLoad;
        private EditText mReview;
        private ImageButton mPostReview;

        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private string _username;

        public string Username { get; private set; }
        public string UserID { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Username = app.GetString("Fullname", string.Empty);
            UserID = app.GetString("USERID", string.Empty);

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_review);

            // Create your application here
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            mBottomCardView = FindViewById<CardView>(Resource.Id.bottom_cardview);
            mReview = FindViewById<EditText>(Resource.Id.edtReview);
            mPostReview = FindViewById<ImageButton>(Resource.Id.btnPost);

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "";

            InitDecoration();
            SetUpRecyclerView(mRecyclerView);

            mPostReview.Click += (ob, se) =>
            {
                if (mReview.Text != string.Empty)
                {
                    PostReview();
                    mReview.Text = string.Empty;
                }
            };
            GetReviews();
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
        private void PostReview()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();

            var review = new Reviews
            {
                User = Username,
                UserID = UserID,
                Review = mReview.Text,
                Post_Date = DateTime.Now.ToShortDateString()
            };
            p.PostAppReview(database,review);
            var text = Resources.GetString(Resource.String.snk_thanks_feedback);
            Snackbar.Make(FindViewById(Android.Resource.Id.Content),text, Snackbar.LengthShort).Show();
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

            _recyclerView.HasFixedSize = true;
            var layoutManager = new LinearLayoutManager(this);

            onScrollListener = new XamarinRecyclerViewOnScrollListener(layoutManager);
            onScrollListener.LoadMoreEvent += OnScrollListener_LoadMoreEventAsync;
            onScrollListener.OnHide += OnScrollListener_OnHide;
            onScrollListener.OnShow += OnScrollListener_OnShow;
            _recyclerView.AddOnScrollListener(onScrollListener);
            _recyclerView.SetLayoutManager(layoutManager);

            _recyclerView.AddItemDecoration(horizontalDecoration);

        }

        private void OnScrollListener_LoadMoreEventAsync(object sender, EventArgs e)
        {
            if (LastLoad > 10)
            {
                onScrollListener.IsLoading = true;
                HideShowProgress();
                GetReviews();
            }
        }

        private void OnScrollListener_OnShow(object sender, EventArgs e)
        {
            showViews();
        }

        private void showViews()
        {
            mBottomCardView.Animate().TranslationY(0).SetInterpolator(new DecelerateInterpolator(2)).Start();
        }

        private void OnScrollListener_OnHide(object sender, EventArgs e)
        {
            hideViews();
        }

        private void hideViews()
        {
            CoordinatorLayout.LayoutParams lp = (CoordinatorLayout.LayoutParams)mBottomCardView.LayoutParameters;
            int fabBottomMargin = lp.BottomMargin;
            mBottomCardView.Animate().TranslationY(mBottomCardView.Height + fabBottomMargin).SetInterpolator(new AccelerateInterpolator(2)).Start();
        }

        private void GetReviews()
        { 
            int mPageLimit = 10;
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.GetReview(database,mPageLimit,mPageOffset);
            p.Reviews += (sender, obj) =>
            {
                progressBar.Visibility = ViewStates.Invisible;
                mPageOffset = mPageOffset + mPageLimit;

                if (mAdapter != null && mAdapter.reviews.Count > 0)
                {
                    mAdapter.reviews.Clear();
                    mAdapter.AddList(obj);
                }
                else                    
                {
                    mAdapter = new Review_Adapter(this, obj);
                    mRecyclerView.SetAdapter(mAdapter);
                }
                LastLoad = obj.Count;
                onScrollListener.IsLoading = false;
                HideShowProgress();
            };
        }

        private void InitDecoration()
        {
            horizontalDecoration = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            Drawable horizontalDivider = ContextCompat.GetDrawable(this, Resource.Drawable.horizontal_line);
            horizontalDecoration.SetDrawable(horizontalDivider);

        }

    }
}