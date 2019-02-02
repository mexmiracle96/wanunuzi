using System;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Safari_Shopping_Mall.Accessors;
using Android.Support.V4.App;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.Design.Widget;
using Android.Transitions;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Firebase.Database;
using Safari_Shopping_Mall.Helpers;
using Android.Support.V4.View;
using Firebase.Messaging;
using Java.Util;
using Android.Graphics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Android.Support.V7.Widget;
using Safari_Shopping_Mall.Adapters;
using Newtonsoft.Json.Linq;

namespace Safari_Shopping_Mall.Activities
{
    [Activity(Label = "Wanunuzi", NoHistory = true, Theme = "@style/AppTheme")]
    public class CustomizeActivity : AppCompatActivity
    {
        #region Variables
        private EditText mSizes;
        private EditText mItems;
        private Button mDone;
        private CheckBox mMessage;
        private TextView mPriceEach;
        private Purchasing_Type type;
        private int qty;
        private double total;
        private double Price { get; set; }
        private string Colors { get; set; }
        private string Sizes { get; set; }
        private string ProductID { get; set; }
        private string Product { get; set; }
        private string Description { get; set; }
        private string Thumbnail { get; set; }
        private string Currency { get; set; }
        private string _Action { get; set; }
        public string UserID { get; private set; }
        public string PayNo { get; private set; }
        public List<User> LastPurchases { get; private set; }

        private string selected_size = "none";
        private string selected_color;
        private ProgressbarSetup progress;
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private Android.App.AlertDialog _dialog;
        private Products _products;

        #endregion
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private int mCartItemCount;
        private ImageView _CartItemCount;
        private Products products;
        private SqlLiteSession Broker = new SqlLiteSession();
        private ProgressbarSetup progress2;
        private ProgressDialog progressDialog;
        private double mOfferPrice;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            UserID = app.GetString("USERID", string.Empty);
            var _lang = app.GetString("Language", "en");
            ChangeLanguage(_lang);

            base.OnCreate(savedInstanceState);
            _Action = Intent.GetStringExtra("Action");
            progress = new ProgressbarSetup();
            // Create your application here
            SetContentView(Resource.Layout.sale_dialog);
            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            Slide slide = new Slide(GravityFlags.Top);
            GetDataBind();


            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.Title = products.Product;

            mItems = FindViewById<EditText>(Resource.Id.edtQty);
            mSizes = FindViewById<EditText>(Resource.Id.edtSizes);
            mDone = FindViewById<Button>(Resource.Id.btnDone);

            mItems.InputType = Android.Text.InputTypes.Null;
            mSizes.InputType = Android.Text.InputTypes.Null;
            mSizes.FocusableInTouchMode = false;
            mItems.Clickable = false;

            mItems.Text = "1";
            mSizes.Text = products.Sizes.Split(',')[0];

            if (_Action == "Cart")
            {
                mDone.Text = Resources.GetString(Resource.String.add_to_cart);
            }
            else
            {
                setCalcTotalPrice();
                //mDone.Text = Resources.GetString(Resource.String.btn_continue_payment);
            }

            mDone = FindViewById<Button>(Resource.Id.btnDone);
            mItems.TextChanged += (obj, sender) =>
            {
                setCalcTotalPrice();
            };
            mDone.Click += MDone_Click;
            mItems.Click += MItems_Click;
            mSizes.Click += MSizes_Click;
        }

        private void MSizes_Click(object sender, EventArgs e)
        {
            SetSizes();
        }

        private void MItems_Click(object sender, EventArgs e)
        {
            NumberPicker EditItems = new NumberPicker(this);
            EditItems.MaxValue = 50;
            EditItems.MinValue = 1;
            var id = View.GenerateViewId();
            EditItems.Id = id;
            FrameLayout container = new FrameLayout(this);
            FrameLayout.LayoutParams param = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            param.SetMargins(10, 0, 10, 0);
            EditItems.LayoutParameters = param;
            container.AddView(EditItems);

            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle("Weka idadi ya bidhaa");
            builder.SetView(container);
            builder.SetPositiveButton(Resource.String.dialog_ok, delegate
            {
                builder.Dispose();
                qty = EditItems.Value;
                mItems.Text = qty.ToString();

                setCalcTotalPrice();
            });
            builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
            {
                builder.Dispose();
            });
            builder.Show();
        }

        private void MDone_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(mItems.Text))
            {
                mItems.Text = "1";
            }

            var qty = int.Parse(mItems.Text);
            products.Qty = qty;
            //products.Selected_Size = mSizes.Text;

            var S = new SqlLiteSession();

            if (_Action == "Cart")
            {
                if (S.GetCheckIfCartExist(products) > 0)
                {
                    var text = Resources.GetString(Resource.String.snk_cart_exist);
                    Snackbar.Make(FindViewById(Android.Resource.Id.Content), products.Product + " " + text, Snackbar.LengthShort).Show();
                }
                else
                {
                    products.Selected_Size = selected_size;
                    if (S.InsertCart(products) > 0)
                    {
                        var text = Resources.GetString(Resource.String.snkbar_added_to_basket);
                        Snackbar.Make(FindViewById(Android.Resource.Id.Content), products.Product + " " + text, Snackbar.LengthShort).Show();
                    }
                }
            }
            else if (_Action == "Edit")
            {
                if (S.UpdateCart(products) > 0)
                {
                    var text = Resources.GetString(Resource.String.snk_modified);
                    Snackbar.Make(FindViewById(Android.Resource.Id.Content), _products.Product + " " + text, Snackbar.LengthShort).Show();
                }

            }
            else
            {
                CheckLastPurchases();
            }
        }

        private void setCalcTotalPrice()
        {
            mOfferPrice = products.Offer_Price;
            var price = products.Price;

            try
            {
                if (mOfferPrice > 0 && ValidateOffer(products.OfferEnds))
                {
                    qty = int.Parse(mItems.Text);
                    total = Math.Round((mOfferPrice * qty), 2);
                    mDone.Text = "Total " + ThousandsSeparator(total.ToString()) + " | " + GetString(Resource.String.btn_continue_payment);
                }
                else
                {
                    qty = int.Parse(mItems.Text);
                    total = Math.Round((price * qty), 2);
                    mDone.Text = "Total " + ThousandsSeparator(total.ToString()) + " | " + GetString(Resource.String.btn_continue_payment);
                }
            }
            catch (Exception)
            {
                //mTotalPrice.Text = "0";
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_shopping_cart, menu);
            var item = menu.FindItem(Resource.Id.action_cart);

            View actionView = MenuItemCompat.GetActionView(item);
            _CartItemCount = actionView.FindViewById<ImageView>(Resource.Id.cart_badge);

            mCartItemCount = Broker.GetCart().Rows.Count;
            if (mCartItemCount == 0)
            {
                _CartItemCount.Visibility = ViewStates.Gone;
            }
            else
            {
                _CartItemCount.Visibility = ViewStates.Visible;
            }

            actionView.Click += (o, e) =>
            {
                ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                Intent i = new Intent(this, typeof(MyCart_Activity));
                StartActivity(i, option.ToBundle());
            };

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.search:
                    return true;
                case Resource.Id.action_cart:
                    ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
                    Intent j = new Intent(this, typeof(MyCart_Activity));
                    StartActivity(j, option.ToBundle());
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void DirectPurchasing(Products p,User S)
        {
            string purchasesID = string.Empty;
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback firebase = new FirebaseCallback();
            var myRef = database.Reference;
            if (S == null)
            {
                purchasesID = myRef.Push().Key;
            }
            else
            {
                purchasesID = S.PurchasesID;
            }


            //Insert transaction required
            var json = JsonConvert.SerializeObject(p);

            var jsonObject2 = new Purchases();
            jsonObject2.PurchasesID = purchasesID;
            jsonObject2.UserID = UserID;

            p.Selected_Size = selected_size;
            p.Qty = qty;
            var json2 = JsonConvert.SerializeObject(p);
            var json1 = JsonConvert.SerializeObject(jsonObject2);

            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent i = new Intent(this, typeof(ShippingDetails));
            i.PutExtra("products",json2);
            i.PutExtra("transaction",json1);
            //i.PutExtra("from", "cart");

            StartActivity(i, option.ToBundle());

        }

        public void GetDataBind()
        {
            products = new Products();
            products.SellerID = Intent.GetStringExtra("SellerID");
            products.ProductID = Intent.GetStringExtra("ProductID");
            products.Product = Intent.GetStringExtra("Product");
            products.Price = Intent.GetDoubleExtra("Price", 0);
            products.Buy_Price = Intent.GetDoubleExtra("Buy_Price",0);
            products.Offer_Price = Intent.GetDoubleExtra("Offer", 0);
            products.OfferEnds = Intent.GetStringExtra("EndDate");
            products.Condition = Intent.GetStringExtra("Condition");

            products.Thumbnail_1 = Intent.GetStringExtra("Thumbnail_1");
            products.Thumbnail_2 = Intent.GetStringExtra("Thumbnail_2");
            products.Thumbnail_3 = Intent.GetStringExtra("Thumbnail_3");

            products.Sizes = Intent.GetStringExtra("Sizes");
            products.Description = Intent.GetStringExtra("Description");

            ImageView mImg = FindViewById<ImageView>(Resource.Id.productSlider);
            //TextView mCost = FindViewById<TextView>(Resource.Id.txtPrice);
            //var  mOffPrice = FindViewById<TextView>(Resource.Id.txtOff);
            //mProduct.Text = products.Product;

            if (products.Offer_Price > 0 && ValidateOffer(products.OfferEnds))
            {
                //mCost.Text = "TSh " + ThousandsSeparator(products.Offer_Price.ToString());
                //mOffPrice.Text = "TSh " + ThousandsSeparator(products.Price.ToString());
                //mOffPrice.PaintFlags = (mOffPrice.PaintFlags | PaintFlags.StrikeThruText);
                //mOffPrice.Visibility = ViewStates.Visible;
            }
            else
            {
                //mTotalPrice.Text = "TSh " + ThousandsSeparator(products.Price.ToString());
                //mOffPrice.Visibility = ViewStates.Gone;
                //mCost.Text = "TSh " + ThousandsSeparator(products.Price.ToString());
            }

            Glide.With(this).Load(Intent.GetStringExtra("Thumbnail_1")).Apply(RequestOptions.OverrideOf(500, 500).CenterInside()).Into(mImg);

        }

        private void CheckLastPurchases()
        {
            progressDialog = new ProgressDialog(this);
            progressDialog.SetMessage(Resources.GetString(Resource.String.prgdlg));
            progressDialog.SetCanceledOnTouchOutside(false);
            progressDialog.SetCancelable(false);
            progressDialog.Show();
            
            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();

            p.CheckPendingPurchased(database,UserID);
            p.lastPurch += (o, data) =>
            {
                progressDialog.Hide();
                if(data.Count > 0)
                {
                    LastPurchases = data;
                    var builder = new Android.App.AlertDialog.Builder(this);
                    builder.SetTitle(Resource.String.dlg_info);
                    builder.SetMessage(Resource.String.dlg_purchases_info);
                    builder.SetPositiveButton(Resource.String.dlg_btn_continue, delegate
                     {
                         builder.Dispose();
                         DirectPurchasing(products, LastPurchases[0]);

                     });
                    builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
                    {
                        builder.Dispose();
                    });

                    builder.Show();
                }
                else
                {
                    DirectPurchasing(products,null);
                }
            };
        }

        public string ThousandsSeparator(string input)
        {
            var old = input.ToArray();
            var thousands = old.Length / 3;
            var newValue = input;
            var sep = 3;
            for (int i = 0; i < thousands; i++)
            {
                if (sep < old.Length)
                    newValue = newValue.Insert(old.Length - sep, ",");
                sep = sep + 3;
            }
            return newValue;
        }

        public bool ValidateOffer(string date)
        {
            try
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
            catch (Exception)
            {

                return false;
            }
        }

        private void SetSizes()
        {
            var sizes = products.Sizes.Split(',');
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.make_selection);
            builder.SetSingleChoiceItems(sizes, 0, delegate (object o, DialogClickEventArgs e)
              {
                  selected_size = sizes[e.Which];
                  mSizes.Text = selected_size;
              });
            builder.SetPositiveButton(Resource.String.dialog_ok, delegate
             {
                 builder.Dispose();
             });
            builder.Show();
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

            res.UpdateConfiguration(Config, Dm);
        }

    }
    public enum Purchasing_Type
    {
        Retailer,
        WholeSaler
    }

}