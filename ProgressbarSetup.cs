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

namespace Safari_Shopping_Mall
{
    public class ProgressbarSetup
    {
        private ProgressDialog progress;

        public void MyProgreeDialog(Context c, int message)
        {
            var msg = c.Resources.GetString(message);
            progress = new ProgressDialog(c);
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.Indeterminate = true;
            progress.SetMessage(msg);
            progress.SetCancelable(false);
        }

        public void Show()
        {
            progress.Show();
        }

        public void Hide()
        {
            progress.Hide();
        }

    }
}