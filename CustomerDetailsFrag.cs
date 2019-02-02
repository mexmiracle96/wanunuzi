using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Safari_Shopping_Mall.Accessors;
using Java.Util;
using Firebase.Database;
using Android.Support.V4.View;
using Firebase.Messaging;
using Safari_Shopping_Mall.Helpers;

namespace Safari_Shopping_Mall.Fragments
{
    public class CustomerDetailsFrag : Android.Support.V4.App.Fragment
    {
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        ISharedPreferences app2 = Application.Context.GetSharedPreferences("PrivateData", FileCreationMode.Private);

        private string _lang;
        private EditText mFirstname;
        private EditText mLastname;
        private EditText mPhone;
        private EditText mRegion;
        private Button mDone;
        private ScrollView mScrollView;
        private string _phonenumber;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            _lang = app.GetString("Language", "en");

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.purchasing_info, container, false);
            mFirstname = view.FindViewById<EditText>(Resource.Id.edtFirstName);
            mLastname = view.FindViewById<EditText>(Resource.Id.edtLastName);
            mPhone = view.FindViewById<EditText>(Resource.Id.edtPhoneNo);
            mRegion = view.FindViewById<EditText>(Resource.Id.edtLocation);
            mDone = view.FindViewById<Button>(Resource.Id.btnDone);
            mScrollView = view.FindViewById<ScrollView>(Resource.Id.scrollView);

            //mScrollView.ViewTreeObserver.AddOnGlobalLayoutListener(this);

            mDone.Click += MDone_Click;
            return view;
        }

        private void MDone_Click(object sender, EventArgs e)
        {
            SavingPurchases();
        }

        private void SavingPurchases()
        {
            if (!string.IsNullOrEmpty(mFirstname.Text) && !string.IsNullOrEmpty(mLastname.Text) &&
                !string.IsNullOrEmpty(mRegion.Text) && !string.IsNullOrEmpty(mPhone.Text) && !string.IsNullOrEmpty(mRegion.Text))
            {
                var progressDialog = new ProgressDialog(Activity);
                progressDialog.SetCanceledOnTouchOutside(false);
                progressDialog.SetMessage(Resources.GetString(Resource.String.prgdlg));
                progressDialog.Show();

                var customer = new User
                {
                    Firstname = mFirstname.Text,
                    Lastname = mLastname.Text,
                    PhoneNumber = mPhone.Text,
                    Location = mRegion.Text,
                    UserID = app.GetString("USERID", string.Empty),
                };

                var trans = JsonConvert.DeserializeObject<Purchases>(Activity.Intent.GetStringExtra("transaction"));
                var from = Activity.Intent.GetStringExtra("from");

                if (from == null)
                {
                    var p = JsonConvert.DeserializeObject<Products>(Activity.Intent.GetStringExtra("products"));
                    DirectPurchasing(p, Activity.Intent.GetStringExtra("products"), trans, customer);
                    progressDialog.Hide();

                    var builder = new Android.App.AlertDialog.Builder(Activity);
                    builder.SetTitle(Resource.String.dlg_info);
                    builder.SetMessage(Resource.String.order_completed);
                    builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                    {
                        builder.Dispose();
                        ViewPager pager = Activity.FindViewById<ViewPager>(Resource.Id.viewpager);
                        pager.SetCurrentItem(1, true);

                    });
                    builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
                    {
                        builder.Dispose();
                    });
                    builder.Show();
                }
                else
                {
                    var p = JsonConvert.DeserializeObject<List<Products>>(Activity.Intent.GetStringExtra("products"));
                    CartPurchasing(p, Activity.Intent.GetStringExtra("products"), trans, customer);
                    progressDialog.Hide();

                    var builder = new Android.App.AlertDialog.Builder(Activity);
                    builder.SetTitle(Resource.String.dlg_info);
                    builder.SetMessage(Resource.String.order_completed);
                    builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                    {
                        builder.Dispose();
                        ViewPager pager = Activity.FindViewById<ViewPager>(Resource.Id.viewpager);
                        pager.SetCurrentItem(1, true);

                    });
                    builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
                    {
                        builder.Dispose();
                    });
                    builder.Show();

                }

            }
            else if (string.IsNullOrEmpty(mFirstname.Text))
            {
                mFirstname.SetHint(Resource.String.hint_firstname_required);
            }
            else if (string.IsNullOrEmpty(mLastname.Text))
            {
                mLastname.SetHint(Resource.String.hint_lastname_required);
            }
            else if (string.IsNullOrEmpty(mPhone.Text))
            {
                mLastname.SetHint(Resource.String.hint_phone_required);
            }
            else if (string.IsNullOrEmpty(mRegion.Text))
            {
                mLastname.SetHint(Resource.String.hint_location_required);
            }
        }

        private void ChangeLanguage(string language)
        {
            var res = this.Resources;
            var Dm = res.DisplayMetrics;
            var Config = res.Configuration;
            if (language != null)
            {
                Config.SetLocale(new Locale(language));
            }
            else
            {
                Config.SetLocale(new Locale("en"));
            }
            res.UpdateConfiguration(Config,Dm);
        }

        private bool DirectPurchasing(Products p, string productsSerialized, Purchases trans, User S)
        {
            var currentDate = DateTime.Now;
            var newDate = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day);
            string purchasesID = string.Empty;
            FirebaseDatabase database = FirebaseDatabase.Instance;
            var myRef = database.Reference;

            HashMap mapCustomer = new HashMap();
            mapCustomer.Put("sort_name", S.Firstname.ToLower() + S.Lastname.ToLower() + trans.PurchasesID);
            mapCustomer.Put("PurchasesID", trans.PurchasesID);
            mapCustomer.Put("Uid", S.UserID);
            mapCustomer.Put("PayNumber", encodePhoneNumber(S.PhoneNumber));
            mapCustomer.Put("Paid", false);
            mapCustomer.Put("Discount", 0.0);
            mapCustomer.Put("Qty", 1);
            mapCustomer.Put("Total", CalculateAmount(productsSerialized));
            mapCustomer.Put("Customer", S.Firstname + "  " + S.Lastname);
            mapCustomer.Put("Location", S.Location);
            mapCustomer.Put("Date", newDate.ToBinary().ToString());

            HashMap mapProducts = new HashMap();
            mapProducts.Put("SellerID", p.SellerID);
            mapProducts.Put("PurchasesID", trans.PurchasesID);
            mapProducts.Put("Uid", S.UserID);
            mapProducts.Put("ProductID", p.ProductID);
            mapProducts.Put("Product", p.Product);
            mapProducts.Put("Price", p.Price);
            mapProducts.Put("Buy_Price", p.Buy_Price);
            mapProducts.Put("Offer", p.Offer_Price);
            mapProducts.Put("EndDate", p.OfferEnds);
            mapProducts.Put("Condition", p.Condition);

            mapProducts.Put("Description", p.Description);
            mapProducts.Put("Thumbnail_1", p.Thumbnail_1);
            mapProducts.Put("Thumbnail_2", p.Thumbnail_2);
            mapProducts.Put("Thumbnail_3", p.Thumbnail_3);
            mapProducts.Put("Selected_Size", p.Selected_Size);
            mapProducts.Put("Qty", p.Qty);

            //Remove Old Match PurchasesID details
            myRef.Child("Customers").Child(S.UserID).Child(trans.PurchasesID).RemoveValue();
            myRef.Child("Purchases").Child(S.UserID).Child(trans.PurchasesID).RemoveValue();


            //insert PurchasesID details
            myRef.Child("Customers").Child(S.UserID).Child(trans.PurchasesID).SetValue(mapCustomer);
            myRef.Child("Purchases").Child(S.UserID).Child(trans.PurchasesID).Child(p.ProductID).SetValue(mapProducts);

            //Insert transaction required
            HashMap _trans = new HashMap();

            _trans.Put("PurchasesID", trans.PurchasesID);
            _trans.Put("TokenID",app2.GetString("token",string.Empty));
            _trans.Put("Uid", S.UserID);
            _trans.Put("Products", productsSerialized);
            _trans.Put("Paid", false);
            _trans.Put("sort_name", S.Firstname.ToLower() + S.Lastname.ToLower() + purchasesID);
            _trans.Put("Customer", S.Firstname + "  " + S.Lastname);
            _trans.Put("Location", S.Location);
            _trans.Put("PayNumber", encodePhoneNumber(S.PhoneNumber));
            _trans.Put("Watched", false);
            _trans.Put("Qty", 1);
            _trans.Put("Discount", 0.0);
            _trans.Put("Total", CalculateAmount(productsSerialized));
            _trans.Put("Date", newDate.ToBinary().ToString());

            myRef.Child("Transactions").Child(trans.PurchasesID).SetValue(_trans);

            var editor = app.Edit();
            editor.PutString("PurchasesID", trans.PurchasesID);
            editor.Apply();

            return true;
        }

        private bool CartPurchasing(List<Products> pr, string productsSerialized, Purchases trans, User S)
        {
            var currentDate = DateTime.Now;
            var newDate = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day);

            string purchasesID = string.Empty;
            FirebaseDatabase database = FirebaseDatabase.Instance;
            var myRef = database.Reference;

            HashMap mapCustomer = new HashMap();
            mapCustomer.Put("sort_name", S.Firstname.ToLower() + S.Lastname.ToLower() + trans.PurchasesID);
            mapCustomer.Put("PurchasesID", trans.PurchasesID);
            mapCustomer.Put("Uid", S.UserID);
            mapCustomer.Put("PayNumber", encodePhoneNumber(S.PhoneNumber));
            mapCustomer.Put("Paid", false);
            mapCustomer.Put("Discount", 0.0);
            mapCustomer.Put("Qty", pr.Count);
            mapCustomer.Put("Total", CalculateAmount(productsSerialized));
            mapCustomer.Put("Customer", S.Firstname + "  " + S.Lastname);
            mapCustomer.Put("Location", S.Location);
            mapCustomer.Put("Date", newDate.ToBinary().ToString());

            //Remove Old Match PurchasesID details
            myRef.Child("Customers").Child(S.UserID).Child(trans.PurchasesID).RemoveValue();
            myRef.Child("Purchases").Child(S.UserID).Child(trans.PurchasesID).RemoveValue();

            //insert PurchasesID details
            myRef.Child("Customers").Child(S.UserID).Child(trans.PurchasesID).SetValue(mapCustomer);
            foreach (var p in pr)
            {
                HashMap mapProducts = new HashMap();
                mapProducts.Put("SellerID", p.SellerID);
                mapProducts.Put("PurchasesID", trans.PurchasesID);
                mapProducts.Put("Uid", S.UserID);
                mapProducts.Put("ProductID", p.ProductID);
                mapProducts.Put("Product", p.Product);
                mapProducts.Put("Price", p.Price);
                mapProducts.Put("Buy_Price", p.Buy_Price);
                mapProducts.Put("Offer", p.Offer_Price);
                mapProducts.Put("EndDate", p.OfferEnds);
                mapProducts.Put("Condition", p.Condition);

                mapProducts.Put("Description", p.Description);
                mapProducts.Put("Thumbnail_1", p.Thumbnail_1);
                mapProducts.Put("Thumbnail_2", p.Thumbnail_2);
                mapProducts.Put("Thumbnail_3", p.Thumbnail_3);
                mapProducts.Put("Selected_Size", p.Selected_Size);
                mapProducts.Put("Qty", p.Qty);

                myRef.Child("Purchases").Child(S.UserID).Child(trans.PurchasesID).Child(p.ProductID).SetValue(mapProducts);
            }


            //Insert transaction required
            HashMap _trans = new HashMap();

            _trans.Put("PurchasesID", trans.PurchasesID);
            _trans.Put("TokenID", app2.GetString("token", string.Empty));
            _trans.Put("Uid", S.UserID);
            _trans.Put("Products", productsSerialized);
            _trans.Put("Paid", false);
            _trans.Put("sort_name", S.Firstname.ToLower() + S.Lastname.ToLower() + purchasesID);
            _trans.Put("Customer", S.Firstname + "  " + S.Lastname);
            _trans.Put("Location", S.Location);
            _trans.Put("PayNumber", encodePhoneNumber(S.PhoneNumber));
            _trans.Put("Watched", false);
            _trans.Put("Discount", 0.0);
            _trans.Put("Qty", pr.Count);
            _trans.Put("Total", CalculateAmount(productsSerialized));
            _trans.Put("Date", newDate.ToBinary().ToString());

            myRef.Child("Transactions").Child(trans.PurchasesID).SetValue(_trans);
            var editor = app.Edit();
            editor.PutString("PurchasesID", trans.PurchasesID);
            editor.Apply();

            return true;
        }

        //public void OnGlobalLayout()
        //{
        //    Rect r = new Rect();
        //    mScrollView.GetWindowVisibleDisplayFrame(r);
        //    int screenHeight = mScrollView.RootView.Height;

        //    // r.bottom is the position above soft keypad or device button.
        //    // if keypad is shown, the r.bottom is smaller than that before.
        //    int keypadHeight = screenHeight - r.Bottom;

        //    if (keypadHeight > screenHeight * 0.15)
        //    { // 0.15 ratio is perhaps enough to determine keypad height.
        //      // keyboard is opened
        //        mDone.Animate().Alpha(0f).SetDuration(300).Start();
        //        //mDone.Visibility = ViewStates.Gone;
        //    }
        //    else
        //    {
        //        mDone.Animate().Alpha(1f).SetDuration(300).Start();
        //        //mDone.Visibility = ViewStates.Visible;
        //    }
        //}

        public bool ValidateOffer(string date)
        {
            var endDate = DateTime.FromBinary(long.Parse(date));
            var startDate = DateTime.Today;
            var diff = (endDate.Date - startDate.Date).Days;

            if (diff > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private double CalculateAmount(string _products)
        {
            var total = 0.0;
            try
            {
                var products = JsonConvert.DeserializeObject<List<Products>>(_products);
                foreach (var item in products)
                {
                    if (item.Offer_Price > 0 && ValidateOffer(item.OfferEnds))
                    {
                        total += item.Offer_Price * item.Qty;
                    }
                    else
                    {
                        total += item.Price * item.Qty;
                    }
                }

            }
            catch (Exception)
            {
                var products = JsonConvert.DeserializeObject<Products>(_products);
                if (products.Offer_Price > 0 && ValidateOffer(products.OfferEnds))
                {
                    total += products.Offer_Price * products.Qty;
                }
                else
                {
                    total += products.Price * products.Qty;
                }

            }

            return total;
        }

        private string encodePhoneNumber(string phone)
        {
            var result = phone.Remove(0, 1);
            result = "255" + result;

            return result;
        }

        private void checkUsedNumber(string phone)
        {
            FirebaseDatabase fb = FirebaseDatabase.Instance;
            FirebaseCallback call = new FirebaseCallback();
            call.CheckUsedNumber(fb, phone);
            call.IsReady += (o, s) =>
            {
                if (s)
                {

                }
                else
                {
                    var builder = new AlertDialog.Builder(Activity);
                    builder.SetTitle(Resource.String.dlg_info);
                    builder.SetMessage(Resource.String.title_used_number);
                    builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                    {
                        builder.Dispose();
                    });
                    builder.Show();
                }
            };
        }
    }
}