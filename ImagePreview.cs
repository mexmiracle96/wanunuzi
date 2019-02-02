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
using Android.Text;
using Android.Graphics;
using Android.Support.V4.View;
using Safari_Shopping_Mall.Adapters;

namespace Safari_Shopping_Mall.Fragments
{
    public class ImagePreview : Android.Support.V4.App.DialogFragment, ViewPager.IOnPageChangeListener
    {
        private TextView[] mDots;
        private LinearLayout mDotsLayout;
        private ViewPager mSlideViewpager;
        private ImageShowCaseAdapter mAdapter;
        private List<string> images;
        private ImageButton mClose;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            images = new List<string>();
            images.Add(Activity.Intent.GetStringExtra("Thumbnail_1"));
            images.Add(Activity.Intent.GetStringExtra("Thumbnail_2"));
            images.Add(Activity.Intent.GetStringExtra("Thumbnail_3"));

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.previewer, container, false);

            mSlideViewpager = view.FindViewById<ViewPager>(Resource.Id.viewpager);
            mDotsLayout = view.FindViewById<LinearLayout>(Resource.Id.dotslayout);
            mSlideViewpager.AddOnPageChangeListener(this);
            var img = images.Where(a => !a.Contains("none")).ToList();

            mAdapter = new ImageShowCaseAdapter(Activity, img);
            mSlideViewpager.Adapter = mAdapter;

            addDotsIndicator(0);
            return view;
            
        }

        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            //throw new NotImplementedException();
        }

        public void OnPageScrollStateChanged(int state)
        {
            //throw new NotImplementedException();
        }

        public void OnPageSelected(int position)
        {
            addDotsIndicator(position);
        }

        private void addDotsIndicator(int position)
        {
            var cnt = 0;

            for (int i = 0; i < images.Count; i++)
            {
                if (!images[i].Contains("none"))
                {
                    cnt++;
                }
            }

            mDots = new TextView[cnt];
            mDotsLayout.RemoveAllViews();
            for (int i = 0; i < mDots.Length; i++)
            {
                mDots[i] = new TextView(Activity);
                mDots[i].TextFormatted = Html.FromHtml("&#8226");
                mDots[i].SetTextSize(Android.Util.ComplexUnitType.Sp, 35);
                mDots[i].SetTextColor(Color.ParseColor("#64f49b02"));

                mDotsLayout.AddView(mDots[i]);
            }
            if (mDots.Length > 0)
            {
                mDots[position].SetTextColor(Color.ParseColor("#f49b02"));
            }
        }

    }
}