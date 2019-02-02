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
using Safari_Shopping_Mall.Helpers;

namespace Safari_Shopping_Mall.Fragments
{
    public class Sort_Menu_Dialogy : Android.Support.V4.App.DialogFragment
    {
        private ListView mLvPrice;
        private ListView mLvCondition;
        private IOnSorting listener;
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            if (Activity is IOnSorting)
            {
                listener = (IOnSorting)Activity;
            }

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view =  inflater.Inflate(Resource.Layout.sorting_menu, container, false);
            mLvPrice = view.FindViewById<ListView>(Resource.Id.list_sortprice);
            mLvCondition = view.FindViewById<ListView>(Resource.Id.list_sortCondition);

            InitSortingCondition_Adapter();
            InitSortingPrice_Adapter();

            mLvPrice.ItemClick += MLvPrice_ItemClick;
            mLvCondition.ItemClick += MLvCondition_ItemClick;
            return view;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var dialog = base.OnCreateDialog(savedInstanceState);
            dialog.SetTitle("Add Review");
            dialog.Window.RequestFeature(WindowFeatures.NoTitle);

            return dialog;
        }

        private void MLvCondition_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //throw new NotImplementedException();
            this.Dismiss();
            listener.sorting_New();
        }

        private void MLvPrice_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //throw new NotImplementedException();
            this.Dismiss();
            listener.sortingLow_Price();
        }

        private void InitSortingPrice_Adapter()
        {
            var items = Activity.Resources.GetStringArray(Resource.Array.sorting_price);
            var adapter = new ArrayAdapter(Context, Android.Resource.Layout.SimpleListItemSingleChoice, items);
            mLvPrice.Adapter = adapter;

        }

        private void InitSortingCondition_Adapter()
        {
            var items = Activity.Resources.GetStringArray(Resource.Array.sorting_condtion);
            var adapter = new ArrayAdapter(Context, Android.Resource.Layout.SimpleListItemSingleChoice, items);
            mLvCondition.Adapter = adapter;
        }

    }
}