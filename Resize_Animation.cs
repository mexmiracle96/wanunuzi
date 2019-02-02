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
using Android.Views.Animations;

namespace Safari_Shopping_Mall
{
    public class ResizeAnimation : Animation
    {
        int targetHeight;
        View view;
        int startHeight;

        public ResizeAnimation(View view, int targetHeight, int startHeight)
        {
            this.view = view;
            this.targetHeight = targetHeight;
            this.startHeight = startHeight;
        }

        protected void applyTransformation(float interpolatedTime, Transformation t)
        {
            int newHeight = (int)(startHeight + targetHeight * interpolatedTime);
            //to support decent animation, change new heigt as Nico S. recommended in comments
            //int newHeight = (int) (startHeight+(targetHeight - startHeight) * interpolatedTime);
            view.LayoutParameters.Height = newHeight;
            view.RequestLayout();
        }

        public void initialize(int width, int height, int parentWidth, int parentHeight)
        {
            base.Initialize(width, height, parentWidth, parentHeight);
        }

        public bool willChangeBounds()
        {
            return true;
        }
    }
}
