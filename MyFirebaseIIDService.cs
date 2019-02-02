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
using Firebase.Iid;
using Android.Util;
using Firebase.Messaging;
using Firebase.Database;

namespace Safari_Shopping_Mall
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        const string TAG = "MyFirebaseIIDService";
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, "Refreshed token: " + refreshedToken);
            FirebaseMessaging.Instance.SubscribeToTopic("offers");
            FirebaseMessaging.Instance.SubscribeToTopic("purchases");

            SendRegistrationToServer(refreshedToken);
        }
        void SendRegistrationToServer(string token)
        {

        }
    }
}