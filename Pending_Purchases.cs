using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using Android.Support.V7.Widget;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Safari_Shopping_Mall.Adapters;
using Android.Support.V7.Widget.Helper;
using Safari_Shopping_Mall.Accessors;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Safari_Shopping_Mall.Activities;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;

namespace Safari_Shopping_Mall.Fragments
{
    public class Pending_Purchases : Android.Support.V4.App.Fragment, IOnStartDragListener
    {

        private ProgressBar progressBar;
        private RecyclerView recyclerView;
        private List<Products> products;
        private XamarinRecyclerViewOnScrollListener onScrollListener;
        private RelativeLayout mLoaderContent;
        private int PageNo { get; set; }
        public string UserID { get; private set; }

        private PendingAdapter Adapter;
        private ItemTouchHelper mItemTouchHelper;
        private int mPageOffset;
        private DividerItemDecoration horizontalDecoration;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private string purchasesID;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            UserID = app.GetString("USERID", string.Empty);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.pending_purchases, container, false);
            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar);
            mLoaderContent = view.FindViewById<RelativeLayout>(Resource.Id.relativeLayout);
            mLoaderContent.Visibility = ViewStates.Invisible;

            InitDecoration();
            SetUpRecyclerView(recyclerView);
            CheckLastPurchases();

            return view;
        }

        private void GetPending(string purchasesID)
        {
            int mPageLimit = 6;
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.GetPurchased(database, UserID,purchasesID);
            p.GetProduct += (sender, obj) =>
            {
                mPageOffset = mPageOffset + mPageLimit;
                if (obj.Count > 0)
                {
                    obj.RemoveAll(x => x.Paid == true);
                }
                else
                {
                    mLoaderContent.Visibility = ViewStates.Visible;
                    return;
                }

                if (Adapter != null && Adapter.Product.Count > 0)
                {
                    Adapter.AddList(obj);
                }
                else
                {
                    Adapter = new PendingAdapter(Activity, obj);
                    recyclerView.SetAdapter(Adapter);
                }

                onScrollListener.IsLoading = false;
                HideShowProgress();

            };
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

            recyclerView.HasFixedSize = true;
            var layoutManager = new LinearLayoutManager(Activity);

            onScrollListener = new XamarinRecyclerViewOnScrollListener(layoutManager);
            onScrollListener.LoadMoreEvent += OnScrollListener_LoadMoreEventAsync;
            onScrollListener.OnHide += OnScrollListener_OnHide;
            onScrollListener.OnShow += OnScrollListener_OnShow;
            recyclerView.AddOnScrollListener(onScrollListener);
            recyclerView.SetLayoutManager(layoutManager);

        }

        private void OnScrollListener_LoadMoreEventAsync(object sender, EventArgs e)
        {
            onScrollListener.IsLoading = true;
            GetPending(purchasesID);
            HideShowProgress();
        }

        private void OnScrollListener_OnShow(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void OnScrollListener_OnHide(object sender, EventArgs e)
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

            res.Configuration.UpdateFrom(Config);
        }

        public void OnStartDrag(RecyclerView.ViewHolder viewHolder)
        {
            //throw new NotImplementedException();
        }

        private void InitDecoration()
        {
            horizontalDecoration = new DividerItemDecoration(Activity, DividerItemDecoration.Vertical);
            Drawable horizontalDivider = ContextCompat.GetDrawable(Activity, Resource.Drawable.horizontal_line);
            horizontalDecoration.SetDrawable(horizontalDivider);
        }

        private void CheckLastPurchases()
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.CheckPendingPurchased(database, UserID);
            p.lastPurch += (o, data) =>
            {
                if (data.Count > 0)
                {
                    purchasesID = data[0].PurchasesID;
                    GetPending(data[0].PurchasesID);
                }
                else
                {
                    progressBar.Visibility = ViewStates.Gone;
                    mLoaderContent.Visibility = ViewStates.Visible;
                }

            };
        }
    }
}