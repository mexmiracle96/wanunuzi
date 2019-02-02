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
using Android.Graphics.Drawables;
using Android.Support.V4.Content.Res;
using Android.Graphics;

namespace Safari_Shopping_Mall
{
    public class SimpleItemDecorator : RecyclerView.ItemDecoration
    {
        private readonly Drawable _divider;

        public SimpleItemDecorator(Context context)
        {

            _divider = ResourcesCompat.GetDrawable(context.Resources, Resource.Drawable.line_divider, context.Theme);
        }

        public override void OnDrawOver(Canvas cValue, RecyclerView parent, RecyclerView.State state)
        {
            var left = parent.PaddingLeft;
            var right = parent.Width - parent.PaddingRight;

            for (var i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChildAt(i);

                var parameters = child.LayoutParameters.JavaCast<RecyclerView.LayoutParams>();

                var top = child.Bottom + parameters.BottomMargin;
                var bottom = top + _divider.IntrinsicHeight;

                _divider.SetBounds(left, top, right, bottom);

                if ((parent.GetChildAdapterPosition(child) == parent.GetAdapter().ItemCount - 1) && parent.Bottom < bottom)
                { // this prevent a parent to hide the last item's divider
                    parent.SetPadding(parent.PaddingLeft, parent.PaddingTop, parent.PaddingRight, bottom - parent.Bottom);
                }

                _divider.Draw(cValue);
            }
        }

    }
}