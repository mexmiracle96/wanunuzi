using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Support.V4.App;
using Safari_Shopping_Mall.Activities;
using Safari_Shopping_Mall.Adapters;
using Android.Support.V7.Widget.Helper;
using Safari_Shopping_Mall.Helpers;
using System.Data;
using Safari_Shopping_Mall.Accessors;
using Android.Transitions;
using Java.Util;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Firebase.Database;
using System.Linq;
using Newtonsoft.Json;

namespace Safari_Shopping_Mall
{
    [Activity(Label = "Wanunuzi", Theme = "@style/AppTheme")]
    public class MyCart_Activity : AppCompatActivity, IOnStartDragListener,IOnCartListener
    {
        private Android.Support.V7.Widget.Toolbar mToolbar;
        private Product_Cart_Adapter Adapter;
        private RecyclerView recyclerView;
        private TextView mTotalAmount;
        private List<Carts> MyCarts;
        private List<Products> _Products;
        private ItemTouchHelper mItemTouchHelper;
        private Button btnBuy;
        private string _lang;
        private FloatingActionButton fab;
        private double total;
        private TextView textCartItemCount;
        ISharedPreferences app = Application.Context.GetSharedPreferences("AppData", FileCreationMode.Private);
        private int mCartItemCount;
        private DividerItemDecoration horizontalDecoration;
        private Button mbtnShop;
        private ImageView _CartItemCount;
        private ProgressbarSetup progress;
        private Android.App.AlertDialog _dialog;
        private Button _dialogPositiveButton;
        private Button _dialogNegativeButton;
        private SqlLiteSession Broker = new SqlLiteSession();
        private ProgressDialog progressDialog;
        private string UserID;
        private TextView txtNothing;
        private TextView txtOff;

        public View Customer_Info { get; private set; }
        public List<User> LastPurchases { get; private set; }
        public SqlLiteSession SqlSession { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            _lang = app.GetString("Language", "en");
            UserID = app.GetString("USERID",string.Empty);

            ChangeLanguage(_lang);

            base.OnCreate(savedInstanceState);
            Slide slide = new Slide(GravityFlags.Top);
            Explode explode = new Explode();

            Window.EnterTransition = slide;
            Window.ExitTransition = explode;


            // Create your application here
            SetContentView(Resource.Layout.mycart);

            mToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolBar);
            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            btnBuy = FindViewById<Button>(Resource.Id.btnBuy);
            mTotalAmount = FindViewById<TextView>(Resource.Id.txtTotalPrice);
            txtOff = FindViewById<TextView>(Resource.Id.txtOff);
            txtNothing = FindViewById<TextView>(Resource.Id.txtNothing);


            SetSupportActionBar(mToolbar);
            SupportActionBar ab = SupportActionBar;
            ab.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "";

            BindCartsToRecycler(recyclerView, GetCartSqlLite());
            if (Adapter != null && Adapter.MyCarts.Count > 0)
            {
                Calculate_Total_Amount();
            }
            btnBuy.Click += (o, e) =>
            {
                if (GetCartSqlLite().Count > 0)
                {

                    var progressDialog = new ProgressDialog(this);
                    progressDialog.SetCanceledOnTouchOutside(false);
                    progressDialog.SetMessage(Resources.GetString(Resource.String.prgdlg));
                    progressDialog.Show();

                    FirebaseDatabase database = FirebaseDatabase.Instance;
                    FirebaseCallback firebase = new FirebaseCallback();

                    firebase.CheckIfCanBuy(database);
                    firebase.IsReady += (b, s) =>
                    {
                        progressDialog.Hide();
                        if (!s)
                        {
                            var builder = new Android.App.AlertDialog.Builder(this);
                            builder.SetTitle(Resource.String.dlg_info);
                            builder.SetMessage(Resource.String.dlg_service_not_ready);
                            builder.SetPositiveButton(Resource.String.dialog_ok, delegate
                            {
                                builder.Dispose();
                            });
                            builder.Show();
                        }
                        else
                        {
                            CheckLastPurchases();
                        }
                    };
                }
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void BindCartsToRecycler(RecyclerView _recyclerView, List<Products> carts)
        {
            //Create our layout manager
            horizontalDecoration = new DividerItemDecoration(this, DividerItemDecoration.Vertical);
            Drawable horizontalDivider = ContextCompat.GetDrawable(this, Resource.Drawable.horizontal_line);
            horizontalDecoration.SetDrawable(horizontalDivider);

            _recyclerView.AddItemDecoration(horizontalDecoration);
            _recyclerView.SetItemAnimator(new DefaultItemAnimator());
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));

            Adapter = new Product_Cart_Adapter(this, carts, this,this);
            //Adapter.ItemClick += Adapter_ItemClick;
            _recyclerView.SetAdapter(Adapter);

            ItemTouchHelper.Callback callback = new SimpleItemTouchHelperCallback(Adapter);
            mItemTouchHelper = new ItemTouchHelper(callback);
            mItemTouchHelper.AttachToRecyclerView(recyclerView);
        }

        private List<Products> GetCartSqlLite()
        {
            var p = new Products();
            var _products = new List<Products>();

            SqlSession = new SqlLiteSession();
            foreach (DataRow item in SqlSession.CheckLastCart().Rows)
            {
                p = new Products()
                {
                    SellerID = item["SellerID"].ToString(),
                    ID = int.Parse(item["ID"].ToString()),
                    ProductID = item["ProductID"].ToString(),
                    Product = item["Product"].ToString(),
                    Condition = item["Condition"].ToString(),
                    Description = item["Description"].ToString(),
                    Price = double.Parse(item["Price"].ToString()),
                    Buy_Price = double.Parse(item["Buy_Price"].ToString()),
                    Sizes = item["Sizes"].ToString(),
                    Offer_Price = double.Parse(item["Offer"].ToString()),
                    OfferEnds = item["EndDate"].ToString(),

                    Selected_Size = item["Selected_Size"].ToString(),
                    Qty = int.Parse(item["Qty"].ToString()),
                    Thumbnail_1 = item["Thumbnail_1"].ToString(),
                    Thumbnail_2 = item["Thumbnail_2"].ToString(),
                    Thumbnail_3 = item["Thumbnail_3"].ToString(),
                };

                if(p.Offer_Price > 0)
                {
                    if (CheckOfferExpiration(p.OfferEnds))
                    {
                        p.Offer_Price = 0;
                        SqlSession.UpdateOffer(p);
                    }
                }
                _products.Add(p);
            }

            return _products;
        }

        private bool CheckOfferExpiration(string date)
        {
            try
            {
                var endDate = DateTime.FromBinary(long.Parse(date));
                var startDate = DateTime.Today;
                var diff = (endDate.Date - startDate.Date).Days;
                if (diff < 0)
                    return true;

            }
            catch (Exception)
            {

                ///throw;
            }

            return false;
        }

        public void OnStartDrag(RecyclerView.ViewHolder viewHolder)
        {
            mItemTouchHelper.StartDrag(viewHolder);
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

        private void Calculate_Total_Amount()
        {
            total = 0;
            double offPrice = 0;
            var offer = false;
            foreach (var item in Adapter.MyCarts)
            {
                if (item.Offer_Price > 0)
                {
                    offPrice += (item.Offer_Price * item.Qty);
                    offer = true;
                }
                else
                {
                    offPrice += (item.Price * item.Qty);
                }
            }

            //Find original price
            for (int i = 0; i < Adapter.MyCarts.Count; i++)
            {
                
                total += (Adapter.MyCarts[i].Price * Adapter.MyCarts[i].Qty);
            }

            //Overall results
            if (offer)
            {
                mTotalAmount.Text = "TSh " + ThousandsSeparator(offPrice.ToString());
                txtOff.Text = "TSh " + ThousandsSeparator(total.ToString());
                txtOff.PaintFlags = (txtOff.PaintFlags | Android.Graphics.PaintFlags.StrikeThruText);
                txtOff.Visibility = ViewStates.Visible;
            }
            else
            {
                mTotalAmount.Text = "TSh " + ThousandsSeparator(total.ToString());
                txtOff.Visibility = ViewStates.Gone;
            }

            if (Adapter.MyCarts.Count == 0)
                txtNothing.Visibility = ViewStates.Visible;
            else
                txtNothing.Visibility = ViewStates.Gone;
        }

        private void Purchasing(List<Products> pr,User S)
        {
            string purchasesID = string.Empty;
            FirebaseDatabase database = FirebaseDatabase.Instance;
            var myRef = database.Reference;
            if (S == null)
            {
                purchasesID = myRef.Push().Key;
            }
            else
            {
                purchasesID = S.PurchasesID;
            }

            if (!string.IsNullOrEmpty(S.PurchasesID))
                myRef.Child("Purchases").Child(UserID).Child(purchasesID).RemoveValue();



            var jsonObject2 = new Purchases();
            jsonObject2.PurchasesID = purchasesID;
            jsonObject2.UserID = UserID;

            //Insert transaction required
            var json = JsonConvert.SerializeObject(pr);
            var json1 = JsonConvert.SerializeObject(jsonObject2);

            ActivityOptionsCompat option = ActivityOptionsCompat.MakeSceneTransitionAnimation(this);
            Intent i = new Intent(this, typeof(ShippingDetails));
            i.PutExtra("products", json);
            i.PutExtra("transaction", json1);
            i.PutExtra("from", "cart");

            StartActivity(i, option.ToBundle());

        }

        protected override void OnResume()
        {
            base.OnResume();
            InvalidateOptionsMenu();
        }

        public void IItemAdded(int position, Products p)
        {
            try
            {
                Calculate_Total_Amount();
            }
            catch (Exception)
            {
                mTotalAmount.Text = "TSh 0";
            }
            var text = Resources.GetString(Resource.String.snkbar_added_to_basket);
            Snackbar.Make(FindViewById(Android.Resource.Id.Content), p.Product + " " + text, Snackbar.LengthShort).Show();

            InvalidateOptionsMenu();
        }

        public void IItemARemoved(int position, Products p)
        {
            try
            {
                if (Adapter.MyCarts.Count > 0)
                {
                    txtNothing.Visibility = ViewStates.Invisible;
                    Calculate_Total_Amount();
                }
                else
                {
                    txtNothing.Visibility = ViewStates.Visible;
                    txtOff.Visibility = ViewStates.Gone;
                    mTotalAmount.Text = "TSh 0.0";
                }

            }
            catch (Exception)
            {
                mTotalAmount.Text = "TSh 0";
            }

            var text = Resources.GetString(Resource.String.snk_removed_basket);
            Snackbar.Make(FindViewById(Android.Resource.Id.Content), p.Product + " " + text, Snackbar.LengthShort)
                .SetAction("UNDO",delegate
                {
                    Adapter.OnItemAdded(position, p);
                    Calculate_Total_Amount();
                }).Show();

            InvalidateOptionsMenu();
        }

        private void CheckLastPurchases()
        {
            progressDialog = new ProgressDialog(this);
            progressDialog.SetCanceledOnTouchOutside(false);
            progressDialog.SetMessage(Resources.GetString(Resource.String.prgdlg));
            progressDialog.Show();

            FirebaseDatabase database = FirebaseDatabase.Instance;
            FirebaseCallback p = new FirebaseCallback();

            p.CheckPendingPurchased(database, UserID);
            p.lastPurch += (o, data) =>
            {
                progressDialog.Hide();
                if (data.Count > 0)
                {
                    LastPurchases = data;
                    var builder = new Android.App.AlertDialog.Builder(this);
                    builder.SetTitle(Resource.String.dlg_info);
                    builder.SetMessage(Resource.String.dlg_purchases_info);
                    builder.SetPositiveButton(Resource.String.dlg_btn_continue, delegate
                    {
                        builder.Dispose();
                        Purchasing(GetCartSqlLite(),data[0]);

                    });
                    builder.SetNegativeButton(Resource.String.dialog_cancel, delegate
                    {
                        builder.Dispose();
                    });

                    builder.Show();
                }
                else
                {
                    Purchasing(GetCartSqlLite(),null);
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

    }
}