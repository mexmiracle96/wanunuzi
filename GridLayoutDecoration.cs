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
using Android.Graphics;

namespace Safari_Shopping_Mall
{
    public class SpacesItemDecoration : RecyclerView.ItemDecoration
    {
    private int space;

    public SpacesItemDecoration(int space)
    {
        this.space = space;
    }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            outRect.Left = space;
            outRect.Right = space;
            outRect.Bottom = space;

            // Add top margin only for the first item to avoid double space between items
            if (parent.GetChildAdapterPosition(view) == 0)
            {
                outRect.Top = space;
            }
        }
    }
}