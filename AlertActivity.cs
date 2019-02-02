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
using Android.Support.V7.App;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", Theme = "@style/Theme.AppCompat.Dialog",ExcludeFromRecents =true)]
    public class AlertActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            var title = Intent.GetStringExtra("title");
            var body = Intent.GetStringExtra("body");

            var alertDialog = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertDialog.SetTitle(Resource.String.app_name);
            alertDialog.SetMessage(body);
            alertDialog.SetPositiveButton("OK", delegate
             {
                 alertDialog.Dispose();
                 Finish();
             });
            alertDialog.Create().Show();
        }
    }
}