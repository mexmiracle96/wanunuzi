using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SupportActionBar = Android.Support.V7.App.ActionBar;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Safari_Shopping_Mall.Fragments;
using Safari_Shopping_Mall.Adapters;
using Safari_Shopping_Mall.Accessors;
using System.Data;
using Android.Support.V4.Content;
using Android.Graphics.Drawables;
using Safari_Shopping_Mall.Helpers;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget.Helper;
using Safari_Shopping_Mall.Activities;
using Android.Support.V4.App;

namespace Safari_Shopping_Mall
{
    [Activity(Label = "Wanunuzi", Theme ="@style/AppTheme2")]
    public class NotificationActivitity : AppCompatActivity,IOnStartDragListener, IOnNotificationRemoved
    {
        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        private Android.Support.V7.Widget.Toolbar mToolbar;
        private RecyclerView mRecyclerView;
        private Nofication_Adapter Adapter;
        private SqlLiteSession Broker = new SqlLiteSession();
        private List<Notifications> data;
        private DividerItemDecoration horizontalDecoration;
        private ItemTouchHelper mItemTouchHelper;
        private ProgressBar mProgressbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.notification_activity);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            mProgressbar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.menu_notification);

            InitDecoration();

            var notification = GetNotification();
            InitRecyclerView(mRecyclerView,notification);

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return true;
        }

        public List<Notifications> GetNotification()
        {
            data = new List<Notifications>();
            foreach (DataRow item in Broker.GetNotifications().Rows)
            {
                var notification = new Notifications
                {
                    read = int.Parse(item["read"].ToString()),
                    Id = int.Parse(item["ID"].ToString()),
                    Body = item["body"].ToString(),
                    Date = item["date"].ToString(),
                    Title = item["title"].ToString(),
                    Json = item["json"].ToString()
                };
                data.Add(notification);
            }
            return data;
        }

        private void InitRecyclerView(RecyclerView _recyclerView,List<Notifications> data)
        {
            var layoutManager = new LinearLayoutManager(this);
            _recyclerView.SetLayoutManager(layoutManager);
            _recyclerView.AddItemDecoration(horizontalDecoration);

            Adapter = new Nofication_Adapter(this, data, this, this);
            Adapter.ItemClick += Adapter_ItemClick;
            mRecyclerView.SetAdapter(Adapter);

            ItemTouchHelper.Callback callback = new SimpleItemTouchHelperCallback(Adapter);
            mItemTouchHelper = new ItemTouchHelper(callback);
            mItemTouchHelper.AttachToRecyclerView(_recyclerView);

            mProgressbar.Visibility = ViewStates.Gone;
        }

        private void Adapter_ItemClick(object sender, int p)
        {
            Broker.UpdateReaded(Adapter._Notifications[p].Id);
            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent i = new Intent(this, typeof(DetailsNotification));

            i.PutExtra("body", Adapter.Notif[p].Body);
            i.PutExtra("title", Adapter.Notif[p].Title);
            i.PutExtra("date", Adapter.Notif[p].Date);
            i.PutExtra("json", Adapter.Notif[p].Json);
            StartActivity(i, option.ToBundle());
        }

        private void InitDecoration()
        {
            horizontalDecoration = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            Drawable horizontalDivider = ContextCompat.GetDrawable(this, Resource.Drawable.horizontal_line);
            horizontalDecoration.SetDrawable(horizontalDivider);
        }

        public void OnStartDrag(RecyclerView.ViewHolder viewHolder)
        {
            mItemTouchHelper.StartDrag(viewHolder);
        }

        public void ItemRemoved(int pos, Notifications item)
        {
            Snackbar.Make(FindViewById(Android.Resource.Id.Content)," Deleted", Snackbar.LengthShort)
               .Show();
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

        public void ItemAdded(int position, Notifications item)
        {
            //throw new NotImplementedException();
        }
    }
}