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
using Firebase.Messaging;
using Android.Util;
using Android.Support.V4.App;
using Safari_Shopping_Mall.Accessors;
using Safari_Shopping_Mall.Activities;

namespace Safari_Shopping_Mall
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private SqlLiteSession sql = new SqlLiteSession();
        const string TAG = "MyFirebaseMsgService";
        Handler handler;
        Action runnable;

        public override void OnMessageReceived(RemoteMessage message)
        {
            Log.Debug(TAG, "From: " + message.From);

            IDictionary<String, String> data = message.Data;

            var body = data["body"];
            var title = data["title"];
            var json = data["json"];
            var iD = data["PurchasesID"];
            var pid = app.GetString("PurchasesID", string.Empty);

            //var Date = data["Date"];
            if (!string.IsNullOrEmpty(iD) && iD == pid)
            {
                var notification = new Notifications
                {
                    Body = body,
                    Date = null,
                    Title = title,
                    Json = json
                };
                sql.SaveNotification(notification);
                SendNotification(body, data);
                DisplayAlertNotification(title, body);
            }
            else if(string.IsNullOrEmpty(iD))
            {
                var notification = new Notifications
                {
                    Body = body,
                    Date = null,
                    Title = title,
                    Json = json
                };
                sql.SaveNotification(notification);
                SendNotification(body, data);
                DisplayAlertNotification(title, body);
            }
            else
            {
                return;
            }

        }

        void SendNotification(string messageBody, IDictionary<string, string> data)
        {
            var intent = new Intent(this, typeof(NotificationActivitity));
            intent.AddFlags(ActivityFlags.ClearTop);
            foreach (var key in data.Keys)
            {
                intent.PutExtra(key, data[key]);
            }

            var pendingIntent = PendingIntent.GetActivity(this,
                                                          NotificationActivitity.NOTIFICATION_ID,
                                                          intent,
                                                          PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this)
                                      .SetSmallIcon(Resource.Mipmap.ic_launcher)
                                      .SetContentTitle("Wanunuzi.com")
                                      .SetContentText(messageBody)
                                      .SetAutoCancel(true)
                                      
                                      .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(NotificationActivitity.NOTIFICATION_ID, notificationBuilder.Build());
        }
        
        void DisplayAlertNotification(string title,string message)
        {
            Intent alert = new Intent(this, typeof(AlertActivity));
            alert.PutExtra("title", title);
            alert.PutExtra("body", message);
            alert.SetFlags(ActivityFlags.NewTask);
            StartActivity(alert);

        }
    }
}