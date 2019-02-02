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
using Android.Support.V7.Widget;

namespace Safari_Shopping_Mall
{
    public class RecyclerviewScrollListener2
    {
        public class XamarinRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
        {
            public event EventHandler LoadMoreEvent;
            public bool IsLoading { get; set; }
            private GridLayoutManager LayoutManager;
            private const int HIDE_THRESHOLD = 20;
            private int scrolledDistance = 0;
            private bool controlsVisible = true;
            public event EventHandler OnHide;
            public event EventHandler OnShow;

            public XamarinRecyclerViewOnScrollListener(GridLayoutManager layoutManager)
            {
                LayoutManager = layoutManager;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                base.OnScrolled(recyclerView, dx, dy);

                var visibleItemCount = recyclerView.ChildCount;
                var totalItemCount = recyclerView.GetAdapter().ItemCount;
                var pastVisiblesItems = LayoutManager.FindFirstVisibleItemPosition();

                if ((visibleItemCount + pastVisiblesItems) >= totalItemCount && !IsLoading && dy > 0)
                {
                    LoadMoreEvent?.Invoke(this, null);
                }

                if (scrolledDistance > HIDE_THRESHOLD && controlsVisible)
                {
                    OnHide?.Invoke(this, null);
                    controlsVisible = false;
                    scrolledDistance = 0;
                }

                else if (scrolledDistance < -HIDE_THRESHOLD && !controlsVisible)
                {
                    OnShow?.Invoke(this, null);
                    controlsVisible = true;
                    scrolledDistance = 0;
                }

                if ((controlsVisible && dy > 0) || (!controlsVisible && dy < 0))
                {
                    scrolledDistance += dy;
                }

            }
        }

    }
}