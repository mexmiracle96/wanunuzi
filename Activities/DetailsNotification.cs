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
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.Widget;
using Android.Transitions;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Safari_Shopping_Mall.Accessors;
using Safari_Shopping_Mall.Adapters;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Com.Bumptech.Glide.Load.Resource.Drawable;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Android.Support.V4.App;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme2")]
    public class DetailsNotification : AppCompatActivity,Helpers.onAdapterViewClicked
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private ImageView mImageView;
        private TextView mTilte;
        private TextView mDescription;
        private RecyclerView mRecyclerView;
        private DividerItemDecoration horizontalDecoration;
        private Product_Adapter mAdapter;
        private TextView mDate;
        private TextView mTag;
        private LinearLayout mOfferContainer;
        private List<Items> items;
        
        public string SubCategory { get; private set; }
        public string Category { get; private set; }
        public string ProductID { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {

            Fade fade = new Fade(FadingMode.In);
            Fade fadeOut = new Fade(FadingMode.Out);

            Window.EnterTransition = fade;
            Window.ExitTransition = fadeOut;

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.notication_details);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            GetDataBind();

        }

        private void GetDataBind()
        {
            mImageView = FindViewById<ImageView>(Resource.Id.backdrop);
            mTilte = FindViewById<TextView>(Resource.Id.txtTitle);
            mDescription = FindViewById<TextView>(Resource.Id.txtDecription);
            mTag = FindViewById<TextView>(Resource.Id.txtTag);
            mDate = FindViewById<TextView>(Resource.Id.txtDate);
            mOfferContainer = FindViewById<LinearLayout>(Resource.Id.offersContainer);

            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            var json = Intent.GetStringExtra("json");
            var body = Intent.GetStringExtra("body");
            var title = Intent.GetStringExtra("title");
            var date = Intent.GetStringExtra("date");
            SupportActionBar.Title = title;

            mTilte.Text = title;
            mDescription.Text = body;
            mDate.Text = date;

            //Glide.With(this).Load()
            //.Apply(RequestOptions.OverrideOf(600, 600).FitCenter())
            //.Apply(RequestOptions.PlaceholderOf(Resource.Drawable.placeholder).FitCenter())
            //.Transition(DrawableTransitionOptions.WithCrossFade())
            //.Into(mImageView);

            if (!json.Contains("none"))
            {
                InitDecoration();
                InitRecyclerView(mRecyclerView);
                //parse json
                items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Items>>(json);
                mTag.Text = items[0].Tag;

                GetProducts();
            }
            else
            {
                mOfferContainer.Visibility = ViewStates.Gone;
            }
        }

        private void InitDecoration()
        {
            horizontalDecoration = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            Drawable horizontalDivider = ContextCompat.GetDrawable(this, Resource.Drawable.horizontal_line);
            horizontalDecoration.SetDrawable(horizontalDivider);
        }

        private void InitRecyclerView(RecyclerView _recyclerView)
        {
            var layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(layoutManager);

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

        public class Items
        {
            public string CategoryID { get; set; }

            public string SubCategoryID { get; set; }

            public string ProductID { get; set; }

            public string Tag { get; set; }
        }

        private void GetProducts()
        {
            Category = items[items.Count - 1].CategoryID;
            SubCategory = items[items.Count - 1].SubCategoryID;
            ProductID = items[items.Count - 1].ProductID;

            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.GetSingleItem(Category, SubCategory,ProductID,database);
            p.GetProduct += (sender, obj) =>
            {
                if (mAdapter != null && mAdapter.Product.Count > 0)
                {
                    mAdapter.AddList(obj);
                }
                else
                {
                    mAdapter = new Product_Adapter(this, obj, this);
                    mRecyclerView.SetAdapter(mAdapter);
                }

                items.RemoveAt(items.Count - 1);

                GetProducts();

                if (items.Count == 0)
                {
                }
            };

        }

        public void OnItemOptionClicked(int p)
        {
            //OptionMenuDialog(p);
        }

    }
}