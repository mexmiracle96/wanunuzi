using System;
using Mono.Data.Sqlite;
using System.IO;
using System.Data;
using Safari_Shopping_Mall.Accessors;
using Android.Util;

namespace Safari_Shopping_Mall
{
    public class SqlLiteSession
    {
        private SqliteConnection SQLConn;
        private SqliteCommand SQLCmd;
        private string dbPath;

        public string AdsID { get; private set; }

        public SqlLiteSession()
        {
            InitDatabase();
        }

        private void ConnectTo()
        {
            SQLConn = new SqliteConnection("Data Source=" + dbPath);
            SQLCmd = SQLConn.CreateCommand();
        }

        public DataTable GetCart()
        {
            var listcart = new DataTable();
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }

                var command = "SELECT * from [MyCartTb]";
                Console.WriteLine("Reading data");

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                listcart.Load(SQLCmd.ExecuteReader());

            }
            catch (Exception)
            {

                throw;
            }
            return listcart;
        }

        private void InitDatabase()
        {
            // determine the path for the database file
            DataTable dt = new DataTable();
            dbPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
            "appdb.db3");
            bool exists = File.Exists(dbPath);

            if (!exists)
            {
                Console.WriteLine("Creating database");
                // Need to create the database before seeding it with some data
                SqliteConnection.CreateFile(dbPath);
                ConnectTo();
                IniTables();
            }
            else
            {
                ConnectTo();
            }
            // query the database to prove data was inserted!
        }

        private void IniTables()
        {
            try
            {
                // Open the database connection and create table with data
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }

                var commands = new string[]
                {
                      "CREATE TABLE [MyCartTb] (ID INTEGER PRIMARY KEY AUTOINCREMENT,SellerID,ProductID ntext,Product ntext,Description ntext,Condition ntext,Price ntext,Buy_Price ntext,Offer ntext,EndDate ntext,Qty INTEGER,Sizes ntext,Selected_Size ntext,Thumbnail_1 ntext,Thumbnail_2 ntext,Thumbnail_3 ntext)",
                      "CREATE TABLE [MyNotification] (ID INTEGER PRIMARY KEY AUTOINCREMENT,body ntext,title ntext,json ntext,date ntext,read integer)"
                };


                foreach (var c in commands)
                {
                    SQLCmd = new SqliteCommand(c, SQLConn);
                    SQLCmd.CommandText = c;
                    var r = SQLCmd.ExecuteNonQuery();
                }

            }
            catch (Exception)
            {

                //throw;
            }
        }

        internal void SaveNotification(Notifications ntf)
        {
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "INSERT INTO [MyNotification] ([body],[title],[json],[date],[read])" +
                                "VALUES ('" + ntf.Body + "','" + ntf.Title + "','" + ntf.Json + "','" + ntf.Date + "','" + 0 + "')";

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                var count = SQLCmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Log.Debug("SQL", "Error: " + e.Message);
            }
        }

        public int InsertCart(Products p)
        {
            var rows = 0;
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "INSERT INTO [MyCartTb] ([SellerID],[ProductID],[Product],[Description],[Condition],[Price],[Buy_Price],[Offer],[EndDate],[Qty],[Sizes],[Selected_Size],[Thumbnail_1],[Thumbnail_2],[Thumbnail_3]) " +
                                "VALUES ('" + p.SellerID + "','" + p.ProductID+ "','" + p.Product + "','" + p.Description + "','" + p.Condition + "','" + p.Price + "','" + p.Buy_Price + "','" + p.Offer_Price + "','" + p.OfferEnds + "','" + p.Qty + "','" + p.Sizes + "','" + p.Selected_Size + "','" + p.Thumbnail_1 + "','" + p.Thumbnail_2 + "','" + p.Thumbnail_3 + "')";

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                rows = SQLCmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            return rows;
        }

        public DataTable CheckLastCart()
        {
            var listcart = new DataTable();
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "SELECT * from [MyCartTb]";
                Console.WriteLine("Reading data");

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                listcart.Load(SQLCmd.ExecuteReader());

            }
            catch (Exception)
            {

                throw;
            }
            return listcart;
        }

        public DataTable LastCart()
        {
            var listcart = new DataTable();
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }

                var command = "SELECT * from [MyCartTb] order by ROWID DESC limit 1";
                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                listcart.Load(SQLCmd.ExecuteReader());

            }
            catch (Exception)
            {

                throw;
            }
            return listcart;
        }

        public DataTable LastNotification()
        {
            var listcart = new DataTable();
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }

                var command = "SELECT * from [MyNotification] order by ROWID DESC limit 1";
                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                listcart.Load(SQLCmd.ExecuteReader());

            }
            catch (Exception)
            {

                throw;
            }
            return listcart;
        }

        public int GetCheckIfCartExist(Products p)
        {
            var myCart = new DataTable();
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "SELECT * from [MyCartTb] WHERE Sizes = '" + p.Selected_Size + "' AND Qty = " + p.Qty + " AND ProductID = '" + p.ProductID +"'";
                Console.WriteLine("Reading data");

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                myCart.Load(SQLCmd.ExecuteReader());

            }
            catch (Exception)
            {
                //throw;
            }
            return myCart.Rows.Count;
        }

        public int UpdateCart(Products p)
        {
            var eff = 0;
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "UPDATE [MyCartTb] SET Selected_Size = '" + p.Selected_Size + "',Qty = '" + p.Qty + "'  WHERE ID = '" + p.ID + "' ";

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                eff = SQLCmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            return eff;
        }

        public int DeleteMyCart(int ID)
        {
            var eff = 0;
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "DELETE FROM [MyCartTb] WHERE ID = '" + ID + "' ";

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                eff = SQLCmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            return eff;
        }

        public bool GetReadedNotifications()
        {
            var unread = false;

            var listcart = new DataTable();
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "SELECT read FROM [MyNotification]";
                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                listcart.Load(SQLCmd.ExecuteReader());

                if (listcart.Rows.Count > 0)
                {
                    var command_2 = "SELECT read FROM [MyNotification] WHERE read = 0 ";
                    SQLCmd = new SqliteCommand(command_2, SQLConn);
                    SQLCmd.CommandText = command_2;

                    listcart = new DataTable();
                    listcart.Load(SQLCmd.ExecuteReader());

                    if (listcart.Rows.Count > 0)
                        unread = true;
                }
            }
            catch (Exception)
            {
                //throw;
            }
            return unread;
        }

        public void UpdateReaded(int id)
        {
            var listcart = new DataTable();
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "UPDATE [MyNotification] SET read = 1 " + " where ID = " + id;
                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                var eff = SQLCmd.ExecuteNonQuery();

            }
            catch (Exception)
            {
                //throw;
            }
        }

        public int DeleteNotification(int ID)
        {
            var eff = 0;
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "DELETE FROM [MyNotification] WHERE ID = '" + ID + "' ";

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                eff = SQLCmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                //throw;
            }
            return eff;
        }

        public int UpdateOffer(Products p)
        {
            var eff = 0;
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }
                var command = "UPDATE [MyCartTb] SET Offer = " + p.Offer_Price + " WHERE ID = " + p.ID;

                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                eff = SQLCmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            return eff;
        }

        public DataTable GetNotifications()
        {
            var notification = new DataTable();
            try
            {
                if (SQLConn.State == ConnectionState.Closed || SQLConn.State == ConnectionState.Broken)
                {
                    SQLConn.Open();
                }

                var command = "SELECT * from [MyNotification] order by ROWID DESC";
                SQLCmd = new SqliteCommand(command, SQLConn);
                SQLCmd.CommandText = command;
                notification.Load(SQLCmd.ExecuteReader());

            }
            catch (Exception)
            {
                //throw;
            }
            return notification;
        }
    }
}