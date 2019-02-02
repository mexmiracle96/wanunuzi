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
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Assist;
using UniversalImageLoader.Core.Display;
using Android.Graphics;
using Android.Graphics.Drawables;
using Firebase;

namespace Safari_Shopping_Mall
{
    public class App : Application
    {
        private static App myApp;

        public override void OnCreate()
        {
            base.OnCreate();

            //FirebaseOptions options = new FirebaseOptions.Builder()
            //.SetApplicationId("1:384011559049:android:5672a21bc3f19273") // Required for Analytics.
            //.SetApiKey("9vvq0gmuvHzbm9TwP4Mcldofm559yqypsLCeN0GF") // Required for Auth.
            //.SetDatabaseUrl("https://safari-shoppers.firebaseio.com/") // Required for RTDB.
            //.Build();

            //FirebaseApp.InitializeApp(this /* Context */, options);
        }

        public static App getInstance()
        {
            if (myApp == null)
                myApp = new App();

            return myApp;
        }

        public Bitmap loadBitmapFromView(View view,int width, int height)
        {
            Bitmap returnedBitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(returnedBitmap);
            Drawable bgDrawable = view.Background;
            if(bgDrawable != null)
            {
                bgDrawable.Draw(canvas);
            }
            else
            {
                canvas.DrawColor(Color.White);
                view.Draw(canvas);
            }
            return returnedBitmap;
        }
    }
}