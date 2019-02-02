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
using Android.Support.V7.Widget;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Android.Support.Design.Widget;
using Safari_Shopping_Mall.Accessors;
using Safari_Shopping_Mall.Adapters;
using Android.Support.V7.Widget.Helper;
using Android.Support.V4.App;
using Safari_Shopping_Mall.Activities;
using Java.Util;
using Android.Support.V4.Content;
using Android.Graphics.Drawables;

namespace Safari_Shopping_Mall.Fragments
{
    public class Finished_Purchases : Android.Support.V4.App.Fragment
    {
        private ProgressBar progressBar;
        private RecyclerView recyclerView;
        private List<Products> products;
        private XamarinRecyclerViewOnScrollListener onScrollListener;
        private RelativeLayout mLoaderContent;
        private int PageNo { get; set; }
        public string UserID { get; private set; }

        private Purchases_Adapter Adapter;
        private ItemTouchHelper mItemTouchHelper;
        private int mPageOffset;
        private DividerItemDecoration horizontalDecoration;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private string purchasesID;
        private Button mBtnView;
        private bool ViewMore = false;
        private List<User> purchases;
        private int CurrentIndex = 0;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            UserID = app.GetString("USERID", string.Empty);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view =  inflater.Inflate(Resource.Layout.finished_purchases, container, false);

            //mBottomNav.SetOnNavigationItemSelectedListener(this);
            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar);
            mLoaderContent = view.FindViewById<RelativeLayout>(Resource.Id.relativeLayout);
            mBtnView = view.FindViewById<Button>(Resource.Id.btnViewAll);
            mLoaderContent.Visibility = ViewStates.Invisible; 

            SetUpRecyclerView(recyclerView);
            CheckLastPurchases();
            mBtnView.Click += MBtnView_Click;
            return view;
        }

        private void MBtnView_Click(object sender, EventArgs e)
        {
            Adapter.Purchases.Clear();
            Adapter.NotifyDataSetChanged();

            if (!ViewMore)
            {
                CurrentIndex = 0;
                ViewMore = true;
                mBtnView.SetText(Resource.String.title_view_less);
                GetProducts(purchases[CurrentIndex].PurchasesID);
            }
            else
            {
                CurrentIndex = 0;
                ViewMore = false;
                mBtnView.SetText(Resource.String.title_viewall);
                GetRecentlyPurchases(purchases[0].PurchasesID);
            }
        }

        private void GetProducts(string purchasesID)
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.GetPurchased(database, UserID, purchasesID);
            p.GetProduct += (sender, obj) =>
            {
                if (obj.Count > 0)
                {
                    foreach (var item in obj)
                    {
                        item.Date = purchases[CurrentIndex].Date;
                    }

                    if (Adapter != null && Adapter.Purchases.Count > 0)
                    {
                        Adapter.AddList(obj);
                    }
                    else
                    {
                        Adapter = new Purchases_Adapter(Activity,obj);
                        recyclerView.SetAdapter(Adapter);
                    }
                }
                else
                {
                    progressBar.Visibility = ViewStates.Gone;
                    mLoaderContent.Visibility = ViewStates.Visible;
                    return;
                }

                if (Adapter.ItemCount < 5)
                {
                    onScrollListener.IsLoading = true;
                    HideShowProgress();

                    CurrentIndex++;
                    if (CurrentIndex < purchases.Count)
                    {
                        GetProducts(purchases[CurrentIndex].PurchasesID);
                    }
                }
                else
                {
                    onScrollListener.IsLoading = false;
                    HideShowProgress();
                }

                if (CurrentIndex == purchases.Count -1)
                {
                    recyclerView.RemoveOnScrollListener(onScrollListener);
                    progressBar.Visibility = ViewStates.Gone;
                }
            };
        }

        private void GetRecentlyPurchases(string purchasesID)
        {
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();
            p.GetRecently(database, UserID, purchasesID);
            p.GetProduct += (sender, obj) =>
            {
                if (obj.Count > 0)
                {
                    foreach (var item in obj)
                    {
                        item.Date = purchases[CurrentIndex].Date;
                    }

                    Adapter = new Purchases_Adapter(Activity, obj);
                    recyclerView.SetAdapter(Adapter);
                }
                else
                {
                    recyclerView.RemoveOnScrollListener(onScrollListener);
                    mLoaderContent.Visibility = ViewStates.Visible;
                    return;
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
            if (ViewMore)
            {
                CurrentIndex++;
                if (CurrentIndex < purchases.Count)
                {
                    GetProducts(purchases[CurrentIndex].PurchasesID);
                }
            }
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
            p.CheckPaiedPurchased(database, UserID);
            p.lastPurch += (o, data) =>
            {
                purchases = data;
                if (purchases.Count > 0)
                {
                    purchases.Reverse(0, purchases.Count);

                    purchasesID = purchases[0].PurchasesID;
                    GetRecentlyPurchases(data[0].PurchasesID);
                    mBtnView.Visibility = ViewStates.Visible;
                }
                else
                {
                    mBtnView.Visibility = ViewStates.Gone;
                    progressBar.Visibility = ViewStates.Gone;
                    mLoaderContent.Visibility = ViewStates.Visible;
                }
            };
        }

    }
}