#if INTERNEL
using System;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Web.Services;
using System.Configuration;
using System.Collections.Generic;

namespace Stock_Trading_Simulator_Kernel
{
    /// <summary>
    /// 数据同步
    /// </summary>
    public partial class Synchronizer
    {
        private SqlConnection sqlConn_Sync = null;
        private SqlCommand sqlCmd_Sync = null;
        private SqlDataReader sqlReader_Sync = null;
        private SqlTransaction sqlTrans_Sync = null;

        //private SqlConnection sqlConn_Clr = null;
        //private SqlCommand sqlCmd_Clr = null;
        private SqlDataReader sqlReader_Clr = null;
        //private SqlTransaction sqlTrans_Clr = null;

        private SqlConnection sqlConn_Adm = null;
        private SqlCommand sqlCmd_Adm = null;
        private SqlDataReader sqlReader_Adm = null;
        private SqlTransaction sqlTrans_Adm = null;

        private SqlConnection sqlConn_Init = null;
        private SqlCommand sqlCmd_Init = null;
        private SqlDataReader sqlReader_Init = null;
        private SqlTransaction sqlTrans_Init = null;

        private SqlConnection sqlConn_Unin = null;
        private SqlCommand sqlCmd_Unin = null;
        private SqlDataReader sqlReader_Unin = null;
        private SqlTransaction sqlTrans_Unin = null;

        private SqlConnection sqlConn_Mrg = null;
        private SqlCommand sqlCmd_Mrg = null;
        private SqlDataReader sqlReader_Mrg = null;
        private SqlTransaction sqlTrans_Mrg = null;

        private List<TradingSystem.UserOrders> listOrdersHistory = null;
        private List<TradingHistory> listTradingHistory = null;
        private List<TradingSystem.UserFund> listUserCash = null;       // 订单状态发生改变时更新相应的资金(实际资金)
        private List<TradingSystem.UserFund> listUserUsableCash = null;    // 冻结、撤单等时更新相应的资金(可用资金)
        private List<TradingSystem.UserFund> listNewUserFund = null;
        private List<TradingSystem.UserOrders> listUserOrders = null;
        private List<int> listRemoveOrders = null;
        private List<TradingError> listTradingError = null;
        private List<TradingSystem.UserStocks> listUserStocks = null;
        private List<FundHistory> listFundHistory = null;

        private List<TradingSystem.UserOrders> listDBOrdersHistory = null;
        private List<TradingHistory> listDBTradingHistory = null;
        private List<TradingSystem.UserFund> listDBUserFund = null;
        private List<TradingSystem.UserFund> listDBUserUsableFund = null;
        private List<TradingSystem.UserFund> listDBNewUserFund = null;
        private List<TradingSystem.UserOrders> listDBUserOrders = null;
        private List<int> listDBRemoveOrders = null;
        private List<TradingError> listDBTradingError = null;
        private List<TradingSystem.UserStocks> listDBUserStocks = null;
        private List<FundHistory> listDBFundHistory = null;

        private Thread ThSync = null;
        private bool bSync = false;
        private ushort uSyncFlag = 0;
        private bool bDataCleared = false;
        private Dictionary<int, UserWealth> mapUserWealth = new Dictionary<int, UserWealth>();

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="strConn"></param>
        public Synchronizer(string strConn)
        {
            bSync = true;
            sqlConn_Sync = new SqlConnection(strConn.Trim());
            //sqlConn_Clr = new SqlConnection(strConn.Trim());
            sqlConn_Adm = new SqlConnection(strConn.Trim());
            sqlConn_Mrg = new SqlConnection(strConn.Trim());
            sqlConn_Init = new SqlConnection(strConn.Trim());
            sqlConn_Unin = new SqlConnection(strConn.Trim());

            listOrdersHistory = new List<TradingSystem.UserOrders>();
            listTradingHistory = new List<TradingHistory>();
            listUserCash = new List<TradingSystem.UserFund>();
            listUserUsableCash = new List<TradingSystem.UserFund>();
            listNewUserFund = new List<TradingSystem.UserFund>();
            listUserOrders = new List<TradingSystem.UserOrders>();
            listRemoveOrders = new List<int>();
            listTradingError = new List<TradingError>();
            listUserStocks = new List<TradingSystem.UserStocks>();
            listFundHistory = new List<FundHistory>();

            listDBOrdersHistory = new List<TradingSystem.UserOrders>();
            listDBTradingHistory = new List<TradingHistory>();
            listDBUserFund = new List<TradingSystem.UserFund>();
            listDBUserUsableFund = new List<TradingSystem.UserFund>();
            listDBNewUserFund = new List<TradingSystem.UserFund>();
            listDBUserOrders = new List<TradingSystem.UserOrders>();
            listDBRemoveOrders = new List<int>();
            listDBTradingError = new List<TradingError>();
            listDBUserStocks = new List<TradingSystem.UserStocks>();
            listDBFundHistory = new List<FundHistory>();
        }

        /// <summary>
        /// 初始化 （资金，订单，持股）
        /// </summary>
        /// <param name="mapUserFund">用户资金</param>
        /// <param name="mapUserOrders">用户订单</param>
        /// <param name="mapUserStocks">用户持股</param>
        /// <param name="LastOrderID">最后订单ID</param>
        /// <returns></returns>
        public bool Initialize(ref Dictionary<int, Dictionary<byte, TradingSystem.UserFund>> mapUserFund
            , ref Dictionary<int, TradingSystem.UserOrders> mapUserOrders
            , ref Dictionary<int, List<TradingSystem.UserStocks>> mapUserStocks, ref int LastOrderID)
        {
            try
            {
                listOrdersHistory.Clear(); // 订单历史队列
                listTradingHistory.Clear();// 交易历史队列
                listUserCash.Clear(); // 用户现金队列
                listUserOrders.Clear(); // 用户订单队列
                listUserStocks.Clear();// 用户持股队列
                listRemoveOrders.Clear(); // 移除订单队列
                listTradingError.Clear(); // 交易错误队列
                listFundHistory.Clear();// 资金历史队列
                listUserUsableCash.Clear(); // 用户可用资金队列
                listNewUserFund.Clear();// 新用户队列
                uSyncFlag = 0;// 异步标志

                if (mapUserFund == null)
                    mapUserFund = new Dictionary<int, Dictionary<byte, TradingSystem.UserFund>>();
                else
                    mapUserFund.Clear();

                if (mapUserOrders == null)
                    mapUserOrders = new Dictionary<int, TradingSystem.UserOrders>();
                else
                    mapUserOrders.Clear();

                if (mapUserStocks == null)
                    mapUserStocks = new Dictionary<int, List<TradingSystem.UserStocks>>();
                else
                    mapUserStocks.Clear();

                // 归并用户持仓记录（可买卖股数、均价的重新计算）
                if (!MergeDB())
                    return false;

                try
                {
                    string strTable = "tbUserWealth";
                    DataSet DSUserWealth = Common.UserWealthSvc.MapUserWealth();
                    if (DSUserWealth != null && DSUserWealth.Tables.Contains(strTable) && DSUserWealth.Tables[strTable] != null &&
                        DSUserWealth.Tables[strTable].Rows != null && DSUserWealth.Tables[strTable].Rows.Count > 0 &&
                        DSUserWealth.Tables[strTable].Columns != null && DSUserWealth.Tables[strTable].Columns.Contains("USD") &&
                        DSUserWealth.Tables[strTable].Columns.Contains("HKD") && DSUserWealth.Tables[strTable].Columns.Contains("RMB")
                        && DSUserWealth.Tables[strTable].Columns.Contains("UserID"))
                    {
                        for (int i = 0; i < DSUserWealth.Tables[strTable].Rows.Count; i++)
                        {
                            UserWealth data = new UserWealth();
                            data.Initialize();
                            data.WealthUSD = Common.ConvertPrice(Convert.ToDouble(DSUserWealth.Tables[strTable].Rows[i]["USD"].ToString().Trim()));
                            data.WealthHKD = Common.ConvertPrice(Convert.ToDouble(DSUserWealth.Tables[strTable].Rows[i]["HKD"].ToString().Trim()));
                            data.WealthRMB = Common.ConvertPrice(Convert.ToDouble(DSUserWealth.Tables[strTable].Rows[i]["RMB"].ToString().Trim()));
                            mapUserWealth[Convert.ToInt32(DSUserWealth.Tables[strTable].Rows[i]["UserID"].ToString().Trim())] = data;
                        }
                        Common.UsersStatus(mapUserWealth);
                    }
                    else
                    {
                        Common.Log("Failed to get [UserWealth] info from RankSvr.");
                    }
                }
                catch
                {
                    Common.Log("Failed to get [UserWealth] info from RankSvr.");
                }

                if (sqlConn_Init.State == ConnectionState.Closed)
                    sqlConn_Init.Open();
                LastOrderID = 0;

                RemotingInterface.RI_Fund stiFund = new RemotingInterface.RI_Fund(); stiFund.Clear();
                sqlCmd_Init = new SqlCommand("SELECT * FROM [UserFund] ORDER BY UserID", sqlConn_Init);
                sqlReader_Init = sqlCmd_Init.ExecuteReader();
                while (sqlReader_Init.Read())
                {
                    Dictionary<byte, TradingSystem.UserFund> mapCurrFund = null;
                    if (mapUserFund.ContainsKey((int)sqlReader_Init["UserID"]))
                        mapCurrFund = mapUserFund[(int)sqlReader_Init["UserID"]];

                    if (mapCurrFund == null)
                        mapCurrFund = new Dictionary<byte, TradingSystem.UserFund>();

                    TradingSystem.UserFund currFund = new TradingSystem.UserFund();
                    currFund.Initialize();
                    currFund.UserID = (int)sqlReader_Init["UserID"];
                    currFund.Cash = Convert.ToDouble(sqlReader_Init["Cash"].ToString().Trim());
                    currFund.UsableCash = Convert.ToDouble(sqlReader_Init["UsableCash"].ToString().Trim());
                    currFund.Curr = (TradingSystem.Currency)Convert.ToByte(sqlReader_Init["Currency"].ToString().Trim());
                    if (currFund.Curr == TradingSystem.Currency.Unknown)
                        continue;
                    switch (currFund.Curr)
                    {
                        case TradingSystem.Currency.USD:
                            if (mapUserWealth.ContainsKey(currFund.UserID))
                                currFund.Wealth = mapUserWealth[currFund.UserID].WealthUSD;
                            else
                                currFund.Wealth = Common.stkTrading.defaultUSD;
                            break;
                        case TradingSystem.Currency.HKD:
                            if (mapUserWealth.ContainsKey(currFund.UserID))
                                currFund.Wealth = mapUserWealth[currFund.UserID].WealthHKD;
                            else
                                currFund.Wealth = Common.stkTrading.defaultHKD;
                            break;
                        default:
                            if (mapUserWealth.ContainsKey(currFund.UserID))
                                currFund.Wealth = mapUserWealth[currFund.UserID].WealthRMB;
                            else
                                currFund.Wealth = Common.stkTrading.defaultRMB;
                            break;
                    }

                    mapCurrFund[(byte)currFund.Curr] = currFund;
                    mapUserFund[currFund.UserID] = mapCurrFund;
                    
                    if (Common.stkBuffer != null)
                    {
                        stiFund.Cash = Common.ConvertPrice(currFund.Cash);
                        stiFund.UsableCash = Common.ConvertPrice(currFund.UsableCash);
                        stiFund.Wealth = Common.ConvertPrice(currFund.Wealth);
                        stiFund.Curr = (RemotingInterface.RI_Currency)currFund.Curr;
                        Common.stkBuffer.SetUserFund(currFund.UserID, stiFund);
                    }
                }
                sqlReader_Init.Close();
                if (mapUserFund != null)
                    Common.Log(" *** [" + mapUserFund.Count + "] User Accounts've Been Loaded. *** ");

                RemotingInterface.RI_Order stiOrder = new RemotingInterface.RI_Order(); stiOrder.Clear();
                sqlCmd_Init = new SqlCommand("SELECT * FROM [UserOrders] WHERE " +
                    "(UserID IN (SELECT UserID FROM [UserFund])) AND " +
                    "(ExpiredDate >= DATENAME([year], GETDATE()) + '-' + DATENAME([month], GETDATE()) " +
                    "+ '-' + DATENAME([day], GETDATE())) ORDER BY OrderID", sqlConn_Init);
                sqlReader_Init = sqlCmd_Init.ExecuteReader();
                while (sqlReader_Init.Read())
                {
                    TradingSystem.UserOrders userOrders = new TradingSystem.UserOrders();
                    userOrders.Initialize();
                    userOrders.OrderID = (int)sqlReader_Init["OrderID"];
                    userOrders.UserID = (int)sqlReader_Init["UserID"];
                    userOrders.OrderDate = Convert.ToDateTime(sqlReader_Init["OrderDate"].ToString().Trim());
                    userOrders.UpdatedDate = Convert.ToDateTime(sqlReader_Init["UpdatedDate"].ToString().Trim());
                    userOrders.ExpiredDate = Convert.ToDateTime(sqlReader_Init["ExpiredDate"].ToString().Trim());
                    if (DateTime.Now.TimeOfDay > Common.EndPMTS && userOrders.ExpiredDate.TimeOfDay <= Common.EndPMTS)
                        continue;
                    userOrders.OrderPrice = Convert.ToDouble(sqlReader_Init["OrderPrice"].ToString().Trim());
                    userOrders.TradePrice = Convert.ToDouble(sqlReader_Init["TradePrice"].ToString().Trim());
                    userOrders.Curr = (TradingSystem.Currency)Convert.ToByte(sqlReader_Init["Currency"].ToString().Trim());
                    userOrders.OrdStatus = (TradingSystem.OrderStatus)Convert.ToByte(sqlReader_Init["OrderStatus"].ToString().Trim());
                    userOrders.OrdType = (TradingSystem.OrderType)Convert.ToByte(sqlReader_Init["OrderType"].ToString().Trim());
                    userOrders.StockCode = sqlReader_Init["StockCode"].ToString().Trim();
                    userOrders.Market = (TradingSystem.StockMarket)Convert.ToByte(sqlReader_Init["Market"].ToString().Trim());
                    userOrders.OrderVolume = (int)sqlReader_Init["OrderVolume"];
                    if (sqlReader_Init["OrderSide"].ToString().ToLower().Trim() == "true")
                        userOrders.Side = true;
                    else
                        userOrders.Side = false;
                    LastOrderID = userOrders.OrderID;
                    mapUserOrders[userOrders.OrderID] = userOrders;
                    if (Common.stkBuffer != null)
                    {
                        if (userOrders.OrderDate.Date == DateTime.Now.Date)
                        {
                            stiOrder = new RemotingInterface.RI_Order(); stiOrder.Clear();
                            stiOrder.ExpiredDate = userOrders.ExpiredDate;
                            stiOrder.OrderDate = userOrders.OrderDate;
                            stiOrder.OrderID = userOrders.OrderID;
                            stiOrder.OrderPrice = Common.ConvertPrice(userOrders.OrderPrice);
                            stiOrder.TradePrice = Common.ConvertPrice(userOrders.TradePrice);
                            stiOrder.Curr = (RemotingInterface.RI_Currency)userOrders.Curr;
                            stiOrder.OrderStatus = (RemotingInterface.RI_Status)userOrders.OrdStatus;
                            stiOrder.OrderType = (RemotingInterface.RI_Type)userOrders.OrdType;
                            stiOrder.OrderVolume = userOrders.OrderVolume;
                            stiOrder.Side = userOrders.Side;
                            stiOrder.StockCode = userOrders.StockCode.Trim();
                            stiOrder.StockMarket = (RemotingInterface.RI_Market)userOrders.Market;
                            stiOrder.UpdatedDate = userOrders.UpdatedDate;
                            Common.stkBuffer.SetUserOrders(userOrders.UserID, stiOrder);
                        }
                    }
                }
                sqlReader_Init.Close();

                RemotingInterface.RI_Stock stiStock = new RemotingInterface.RI_Stock(); stiStock.Clear();
                sqlCmd_Init = new SqlCommand("SELECT * FROM [UserStocks] WHERE " +
                    "(UserID IN (SELECT UserID FROM [UserFund])) AND (Volume > 0) ORDER BY UserID, StockCode, Market", sqlConn_Init);
                sqlReader_Init = sqlCmd_Init.ExecuteReader();
                List<TradingSystem.UserStocks> listStocks = new List<TradingSystem.UserStocks>();
                int nLastUserID = 0;
                while (sqlReader_Init.Read())
                {
                    TradingSystem.UserStocks userStock = new TradingSystem.UserStocks();
                    userStock.Initialize();
                    userStock.UserID = (int)sqlReader_Init["UserID"];
                    userStock.StockCode = sqlReader_Init["StockCode"].ToString().Trim();
                    userStock.Market = (TradingSystem.StockMarket)Convert.ToByte(sqlReader_Init["Market"].ToString().Trim());
                    userStock.Volume = (int)sqlReader_Init["Volume"];
                    userStock.AveragePrice = double.Parse(sqlReader_Init["AveragePrice"].ToString().Trim());
                    userStock.Curr = (TradingSystem.Currency)Convert.ToByte(sqlReader_Init["Currency"].ToString().Trim());
                    if (sqlReader_Init["Sellable"].ToString().Trim() == "1")
                        userStock.Sellable = true;
                    else
                        userStock.Sellable = false;
                    if (nLastUserID != userStock.UserID)
                    {
                        if (nLastUserID > 0)
                            mapUserStocks[nLastUserID] = listStocks;

                        listStocks = new List<TradingSystem.UserStocks>();
                        listStocks.Add(userStock);
                        nLastUserID = userStock.UserID;
                    }
                    else
                    {
                        listStocks.Add(userStock);
                    }
                    if (Common.stkBuffer != null)
                    {
                        stiStock.AveragePrice = Common.ConvertPrice(userStock.AveragePrice);
                        stiStock.Curr = (RemotingInterface.RI_Currency)userStock.Curr;
                        stiStock.Volume = userStock.Volume;
                        if (userStock.Sellable)
                            stiStock.SellableVolume = userStock.Volume;
                        else
                            stiStock.SellableVolume = 0;
                        stiStock.StockCode = userStock.StockCode.Trim();
                        stiStock.StockMarket = (RemotingInterface.RI_Market)userStock.Market;
                        if (Common.stkBuffer != null)
                        {
                            Common.stkBuffer.AddUserStocks(userStock.UserID, stiStock);
                        }
                    }
                }
                if (nLastUserID > 0 && listStocks.Count > 0)
                    mapUserStocks[nLastUserID] = listStocks;
                sqlReader_Init.Close();

                RemotingInterface.RI_Order stiHistoryOrder = new RemotingInterface.RI_Order();
                stiOrder.Clear();
                sqlCmd_Init = new SqlCommand("SELECT * FROM [OrdersHistory] WHERE " +
                    "(UserID IN (SELECT UserID FROM [UserFund])) AND (OrderDate >= " +
                    "DATENAME([year], GETDATE()) + '-' + DATENAME([month], GETDATE()) + '-' + " +
                    "DATENAME([day], GETDATE())) ORDER BY OrderID", sqlConn_Init);
                sqlReader_Init = sqlCmd_Init.ExecuteReader();
                while (sqlReader_Init.Read())
                {
                    TradingSystem.UserOrders userOrders = new TradingSystem.UserOrders();
                    userOrders.Initialize();
                    userOrders.OrderID = (int)sqlReader_Init["OrderID"];
                    userOrders.UserID = (int)sqlReader_Init["UserID"];
                    userOrders.OrderDate = Convert.ToDateTime(sqlReader_Init["OrderDate"].ToString().Trim());
                    userOrders.UpdatedDate = Convert.ToDateTime(sqlReader_Init["UpdatedDate"].ToString().Trim());
                    userOrders.ExpiredDate = Convert.ToDateTime(sqlReader_Init["ExpiredDate"].ToString().Trim());
                    if (DateTime.Now.TimeOfDay > Common.EndPMTS && userOrders.ExpiredDate.TimeOfDay <= Common.EndPMTS)
                        continue;
                    userOrders.OrderPrice = Convert.ToDouble(sqlReader_Init["OrderPrice"].ToString().Trim());
                    userOrders.TradePrice = Convert.ToDouble(sqlReader_Init["TradePrice"].ToString().Trim());
                    userOrders.OrdStatus = (TradingSystem.OrderStatus)Convert.ToByte(sqlReader_Init["OrderStatus"].ToString().Trim());
                    userOrders.OrdType = (TradingSystem.OrderType)Convert.ToByte(sqlReader_Init["OrderType"].ToString().Trim());
                    userOrders.StockCode = sqlReader_Init["StockCode"].ToString().Trim();
                    userOrders.Market = (TradingSystem.StockMarket)Convert.ToByte(sqlReader_Init["Market"].ToString().Trim());
                    userOrders.OrderVolume = (int)sqlReader_Init["OrderVolume"];
                    if (sqlReader_Init["OrderSide"].ToString().ToLower().Trim() == "true")
                        userOrders.Side = true;
                    else
                        userOrders.Side = false;
                    userOrders.Curr = (TradingSystem.Currency)Convert.ToByte(sqlReader_Init["Currency"].ToString().Trim());
                    mapUserOrders[userOrders.OrderID] = userOrders;
                    if (Common.stkBuffer != null)
                    {
                        if (userOrders.OrderDate.Date == DateTime.Now.Date)
                        {
                            stiHistoryOrder = new RemotingInterface.RI_Order();
                            stiOrder.Clear();
                            stiHistoryOrder.ExpiredDate = userOrders.ExpiredDate;
                            stiHistoryOrder.OrderDate = userOrders.OrderDate;
                            stiHistoryOrder.OrderID = userOrders.OrderID;
                            stiHistoryOrder.OrderPrice = Common.ConvertPrice(userOrders.OrderPrice);
                            stiHistoryOrder.TradePrice = Common.ConvertPrice(userOrders.TradePrice);
                            stiHistoryOrder.Curr = (RemotingInterface.RI_Currency)userOrders.Curr;
                            stiHistoryOrder.OrderStatus = (RemotingInterface.RI_Status)userOrders.OrdStatus;
                            stiHistoryOrder.OrderType = (RemotingInterface.RI_Type)userOrders.OrdType;
                            stiHistoryOrder.OrderVolume = userOrders.OrderVolume;
                            stiHistoryOrder.Side = userOrders.Side;
                            stiHistoryOrder.StockCode = userOrders.StockCode.Trim();
                            stiHistoryOrder.StockMarket = (RemotingInterface.RI_Market)userOrders.Market;
                            stiHistoryOrder.UpdatedDate = userOrders.UpdatedDate;
                            Common.stkBuffer.SetUserOrders(userOrders.UserID, stiHistoryOrder);
                        }
                    }
                }
                sqlReader_Init.Close();

                if (Common.stkBuffer != null)
                {
                    RemotingInterface.RI_Trading stiTrading = new RemotingInterface.RI_Trading();
                    sqlCmd_Init = new SqlCommand("SELECT * FROM [TradingHistory] WHERE (TradeDate >= " +
                        "DATENAME([year], GETDATE()) + '-' + DATENAME([month], GETDATE()) + '-' + DATENAME([day], GETDATE())) " +
                        "ORDER BY TradeDate", sqlConn_Init);
                    sqlReader_Init = sqlCmd_Init.ExecuteReader();
                    while (sqlReader_Init.Read())
                    {
                        stiTrading.Clear();
                        if (sqlReader_Init["OrderSide"].ToString().ToLower().Trim() == "true")
                            stiTrading.Side = true;
                        else
                            stiTrading.Side = false;
                        stiTrading.TradeDate = DateTime.Parse(sqlReader_Init["TradeDate"].ToString().Trim());
                        if (stiTrading.TradeDate.Date != DateTime.Now.Date ||
                            (DateTime.Now.TimeOfDay > Common.EndPMTS && stiTrading.TradeDate.TimeOfDay <= Common.EndPMTS))
                            continue;
                        stiTrading.StockCode = sqlReader_Init["StockCode"].ToString().Trim();
                        stiTrading.StockMarket = (RemotingInterface.RI_Market)byte.Parse(sqlReader_Init["Market"].ToString().Trim());
                        stiTrading.TradePrice = Common.ConvertPrice(double.Parse(sqlReader_Init["TradePrice"].ToString().Trim()));
                        stiTrading.Curr = (RemotingInterface.RI_Currency)Convert.ToByte(sqlReader_Init["Currency"].ToString().Trim());
                        stiTrading.TradeVolume = (int)sqlReader_Init["TradeVolume"];
                        if (Common.stkBuffer != null)
                        {
                            Common.stkBuffer.SetUserTradings((int)sqlReader_Init["UserID"], stiTrading);
                        }
                    }
                    sqlReader_Init.Close();

                    RemotingInterface.RI_FundChanges stiFundChanges = new RemotingInterface.RI_FundChanges();
                    sqlCmd_Init = new SqlCommand("SELECT * FROM [FundHistory] WHERE (ChangedTime >= " +
                        "DATENAME([year], GETDATE()) + '-' + DATENAME([month], GETDATE()) + '-' + DATENAME([day], GETDATE())) " +
                        "ORDER BY ChangedTime", sqlConn_Init);
                    sqlReader_Init = sqlCmd_Init.ExecuteReader();
                    while (sqlReader_Init.Read())
                    {
                        stiFundChanges.Clear();
                        stiFundChanges.ChangedCash = Convert.ToDouble(sqlReader_Init["ChangedCash"].ToString().Trim());
                        stiFundChanges.ChangedDate = Convert.ToDateTime(sqlReader_Init["ChangedTime"].ToString().Trim());
                        if (stiFundChanges.ChangedDate.Date != DateTime.Now.Date ||
                            (DateTime.Now.TimeOfDay > Common.EndPMTS && stiFundChanges.ChangedDate.TimeOfDay <= Common.EndPMTS))
                            continue;
                        stiFundChanges.Curr = (RemotingInterface.RI_Currency)Convert.ToByte(sqlReader_Init["Currency"].ToString().Trim());
                        stiFundChanges.OrderID = (int)sqlReader_Init["OrderID"];
                        stiFundChanges.OriginalCash = Convert.ToDouble(sqlReader_Init["OriginalCash"].ToString().Trim());
                        if (Common.stkBuffer != null)
                        {
                            Common.stkBuffer.SetUserFundChanges((int)sqlReader_Init["UserID"], stiFundChanges);
                        }
                    }
                    sqlReader_Init.Close();
                }

                sqlCmd_Init = new SqlCommand("SELECT TOP 1 OrderID FROM [OrdersHistory] ORDER BY OrderID DESC", sqlConn_Init);
                sqlReader_Init = sqlCmd_Init.ExecuteReader();
                if (sqlReader_Init.Read() && ((int)sqlReader_Init["OrderID"]) > LastOrderID)
                {
                    LastOrderID = (int)sqlReader_Init["OrderID"];
                }
                sqlReader_Init.Close();

                ThSync = new Thread(new ThreadStart(Synchronizing));
                ThSync.Name = "ThSynchronizing";
                ThSync.Start();
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
            finally
            {
                if (sqlConn_Init.State != ConnectionState.Closed)
                    sqlConn_Init.Close();
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="mapUserFund"></param>
        /// <param name="mapUserOrders"></param>
        /// <param name="mapUserStocks"></param>
        public void Uninitialize(Dictionary<int, Dictionary<byte, TradingSystem.UserFund>> mapUserFund, Dictionary<int, TradingSystem.UserOrders> mapUserOrders
            , Dictionary<int, List<TradingSystem.UserStocks>> mapUserStocks)
        {
            bool bInTransaction = false;
            try
            {
                bSync = false;
                if (ThSync != null && ThSync.IsAlive)
                {
                    if (listOrdersHistory.Count > 0 || listTradingHistory.Count > 0 || listUserCash.Count > 0 || listUserUsableCash.Count > 0 ||
                        listNewUserFund.Count > 0 || listUserOrders.Count > 0 || listRemoveOrders.Count > 0 || listTradingError.Count > 0 ||
                        listUserStocks.Count > 0 || listFundHistory.Count > 0)
                        uSyncFlag = 15;
                    ThSync.Join(7500);
                    if (ThSync != null && ThSync.IsAlive)
                        ThSync.Abort();
                    Thread.Sleep(500);
                }
                if (mapUserFund == null && mapUserOrders == null && mapUserStocks == null)
                    return;
                uSyncFlag = 0;
                int nKey = 0; byte nSubKey = 0;
                if (sqlConn_Unin.State == ConnectionState.Closed)
                    sqlConn_Unin.Open();
                sqlTrans_Unin = sqlConn_Unin.BeginTransaction();
                bInTransaction = true;

                // save buffer info into database
                if (mapUserFund != null && mapUserFund.Count > 0)
                {
                    foreach (object objKey in mapUserFund.Keys)
                    {
                        if (objKey == null)
                            continue;
                        nKey = Convert.ToInt32(objKey);
                        if (!mapUserFund.ContainsKey(nKey))
                            continue;
                        foreach (object objSubKey in mapUserFund[nKey])
                        {
                            if (objSubKey == null)
                                continue;
                            nSubKey = Convert.ToByte(objSubKey);
                            if (!mapUserFund[nKey].ContainsKey(nSubKey))
                                continue;
                            sqlCmd_Unin = new SqlCommand("UPDATE [UserFund] SET Cash = @Cash," +
                                "UsableCash = @UsableCash, Currency = @Currency WHERE UserID = @UserID", sqlConn_Unin, sqlTrans_Unin);
                            sqlCmd_Unin.Parameters.Add("@Cash", SqlDbType.Money); sqlCmd_Unin.Parameters["@Cash"].Value = mapUserFund[nKey][nSubKey].Cash;
                            sqlCmd_Unin.Parameters.Add("@UsableCash", SqlDbType.Money); sqlCmd_Unin.Parameters["@UsableCash"].Value = mapUserFund[nKey][nSubKey].UsableCash;
                            sqlCmd_Unin.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Currency"].Value = (byte)mapUserFund[nKey][nSubKey].Curr;
                            sqlCmd_Unin.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Unin.Parameters["@UserID"].Value = mapUserFund[nKey][nSubKey].UserID;
                            sqlCmd_Unin.ExecuteNonQuery();
                        }
                    }
                }

                if (mapUserOrders != null && mapUserOrders.Count > 0)
                {
                    foreach (object objKey in mapUserOrders.Keys)
                    {
                        if (objKey == null)
                            continue;
                        nKey = Convert.ToInt32(objKey);
                        if (!mapUserOrders.ContainsKey(nKey))
                            continue;
                        sqlCmd_Unin = new SqlCommand("SELECT OrderID FROM [UserOrders] WHERE OrderID = @OrderID", sqlConn_Unin, sqlTrans_Unin);
                        sqlCmd_Unin.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Unin.Parameters["@OrderID"].Value = mapUserOrders[nKey].OrderID;
                        sqlReader_Unin = sqlCmd_Unin.ExecuteReader();
                        if (sqlReader_Unin.Read())
                        {
                            sqlReader_Unin.Close();
                            sqlCmd_Unin = new SqlCommand("UPDATE [UserOrders] SET OrderType = @OrderType,OrderStatus = @OrderStatus,OrderSide = @OrderSide," +
                                "StockCode = @StockCode,Market = @Market,OrderVolume = @OrderVolume,OrderPrice = @OrderPrice,TradePrice = @TradePrice,Currency = @Currency," +
                                "OrderDate = @OrderDate,UpdatedDate = @UpdatedDate,ExpiredDate = @ExpiredDate WHERE (OrderID = @OrderID) AND (UserID = @UserID)", sqlConn_Unin, sqlTrans_Unin);
                            sqlCmd_Unin.Parameters.Add("@OrderType", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@OrderType"].Value = (byte)mapUserOrders[nKey].OrdType;
                            sqlCmd_Unin.Parameters.Add("@OrderStatus", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@OrderStatus"].Value = (byte)mapUserOrders[nKey].OrdStatus;
                            if (mapUserOrders[nKey].Side)
                            {
                                sqlCmd_Unin.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Unin.Parameters["@OrderSide"].Value = (byte)1;
                            }
                            else
                            {
                                sqlCmd_Unin.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Unin.Parameters["@OrderSide"].Value = (byte)0;
                            }
                            sqlCmd_Unin.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Unin.Parameters["@StockCode"].Value = mapUserOrders[nKey].StockCode.Trim();
                            sqlCmd_Unin.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Market"].Value = (byte)mapUserOrders[nKey].Market;
                            sqlCmd_Unin.Parameters.Add("@OrderVolume", SqlDbType.Money); sqlCmd_Unin.Parameters["@OrderVolume"].Value = mapUserOrders[nKey].OrderVolume;
                            sqlCmd_Unin.Parameters.Add("@OrderPrice", SqlDbType.Money); sqlCmd_Unin.Parameters["@OrderPrice"].Value = mapUserOrders[nKey].OrderPrice;
                            sqlCmd_Unin.Parameters.Add("@TradePrice", SqlDbType.Money); sqlCmd_Unin.Parameters["@TradePrice"].Value = mapUserOrders[nKey].TradePrice;
                            sqlCmd_Unin.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Currency"].Value = (byte)mapUserOrders[nKey].Curr;
                            sqlCmd_Unin.Parameters.Add("@OrderDate", SqlDbType.DateTime); sqlCmd_Unin.Parameters["@OrderDate"].Value = mapUserOrders[nKey].OrderDate;
                            sqlCmd_Unin.Parameters.Add("@UpdatedDate", SqlDbType.DateTime); sqlCmd_Unin.Parameters["@UpdatedDate"].Value = mapUserOrders[nKey].UpdatedDate;
                            sqlCmd_Unin.Parameters.Add("@ExpiredDate", SqlDbType.DateTime); sqlCmd_Unin.Parameters["@ExpiredDate"].Value = mapUserOrders[nKey].ExpiredDate;
                            sqlCmd_Unin.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Unin.Parameters["@OrderID"].Value = mapUserOrders[nKey].OrderID;
                            sqlCmd_Unin.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Unin.Parameters["@UserID"].Value = mapUserOrders[nKey].UserID;
                            sqlCmd_Unin.ExecuteNonQuery();
                        }
                        else
                        {
                            sqlReader_Unin.Close();
                            sqlCmd_Unin = new SqlCommand("INSERT INTO [UserOrders] (OrderID,UserID,OrderType,OrderStatus,OrderSide,StockCode," +
                                "Market,OrderVolume,OrderPrice,TradePrice,Currency,OrderDate,UpdatedDate,ExpiredDate) " +
                                "VALUES (@OrderID,@UserID,@OrderType,@OrderStatus,@OrderSide,@StockCode,@Market,@OrderVolume," +
                                "@OrderPrice,@TradePrice,@Currency,@OrderDate,@UpdatedDate,@ExpiredDate)", sqlConn_Unin, sqlTrans_Unin);
                            sqlCmd_Unin.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Unin.Parameters["@OrderID"].Value = mapUserOrders[nKey].OrderID;
                            sqlCmd_Unin.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Unin.Parameters["@UserID"].Value = mapUserOrders[nKey].UserID;
                            sqlCmd_Unin.Parameters.Add("@OrderType", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@OrderType"].Value = (byte)mapUserOrders[nKey].OrdType;
                            sqlCmd_Unin.Parameters.Add("@OrderStatus", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@OrderStatus"].Value = (byte)mapUserOrders[nKey].OrdStatus;
                            if (mapUserOrders[nKey].Side)
                            {
                                sqlCmd_Unin.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Unin.Parameters["@OrderSide"].Value = (byte)1;
                            }
                            else
                            {
                                sqlCmd_Unin.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Unin.Parameters["@OrderSide"].Value = (byte)0;
                            }
                            sqlCmd_Unin.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Unin.Parameters["@StockCode"].Value = mapUserOrders[nKey].StockCode.Trim();
                            sqlCmd_Unin.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Market"].Value = (byte)mapUserOrders[nKey].Market;
                            sqlCmd_Unin.Parameters.Add("@OrderVolume", SqlDbType.Money); sqlCmd_Unin.Parameters["@OrderVolume"].Value = mapUserOrders[nKey].OrderVolume;
                            sqlCmd_Unin.Parameters.Add("@OrderPrice", SqlDbType.Money); sqlCmd_Unin.Parameters["@OrderPrice"].Value = mapUserOrders[nKey].OrderPrice;
                            sqlCmd_Unin.Parameters.Add("@TradePrice", SqlDbType.Money); sqlCmd_Unin.Parameters["@TradePrice"].Value = mapUserOrders[nKey].TradePrice;
                            sqlCmd_Unin.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Currency"].Value = (byte)mapUserOrders[nKey].Curr;
                            sqlCmd_Unin.Parameters.Add("@OrderDate", SqlDbType.DateTime); sqlCmd_Unin.Parameters["@OrderDate"].Value = mapUserOrders[nKey].OrderDate;
                            sqlCmd_Unin.Parameters.Add("@UpdatedDate", SqlDbType.DateTime); sqlCmd_Unin.Parameters["@UpdatedDate"].Value = mapUserOrders[nKey].UpdatedDate;
                            sqlCmd_Unin.Parameters.Add("@ExpiredDate", SqlDbType.DateTime); sqlCmd_Unin.Parameters["@ExpiredDate"].Value = mapUserOrders[nKey].ExpiredDate;
                            sqlCmd_Unin.ExecuteNonQuery();
                        }
                    }
                }

                if (mapUserStocks != null && mapUserStocks.Count > 0)
                {
                    foreach (object objKey in mapUserStocks.Keys)
                    {
                        if (objKey == null)
                            continue;
                        nKey = Convert.ToInt32(objKey);
                        if (!mapUserStocks.ContainsKey(nKey))
                            continue;
                        if (mapUserStocks[nKey] == null || mapUserStocks[nKey].Count <= 0)
                            continue;

                        foreach (TradingSystem.UserStocks userStock in mapUserStocks[nKey])
                        {
                            sqlCmd_Unin = new SqlCommand("SELECT UserID, StockCode FROM [UserStocks] WHERE (UserID = @UserID) " +
                                "AND (StockCode = @StockCode) AND (Market = @Market)", sqlConn_Unin, sqlTrans_Unin);
                            sqlCmd_Unin.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Unin.Parameters["@UserID"].Value = userStock.UserID;
                            sqlCmd_Unin.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Unin.Parameters["@StockCode"].Value = userStock.StockCode.Trim();
                            sqlCmd_Unin.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Market"].Value = (byte)userStock.Market;
                            sqlReader_Unin = sqlCmd_Unin.ExecuteReader();
                            if (sqlReader_Unin.Read())
                            {
                                sqlReader_Unin.Close();
                                
                                sqlCmd_Unin = new SqlCommand("UPDATE [UserStocks] SET Volume = @Volume, AveragePrice = @AveragePrice, Currency = @Currency WHERE " +
                                    "(UserID = @UserID) AND (StockCode = @StockCode) AND (Market = @Market) AND (Sellable = @Sellable)", sqlConn_Unin, sqlTrans_Unin);
                                sqlCmd_Unin.Parameters.Add("@Volume", SqlDbType.Int); sqlCmd_Unin.Parameters["@Volume"].Value = userStock.Volume;
                                sqlCmd_Unin.Parameters.Add("@AveragePrice", SqlDbType.Money); sqlCmd_Unin.Parameters["@AveragePrice"].Value = userStock.AveragePrice;
                                sqlCmd_Unin.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Currency"].Value = (byte)userStock.Curr;
                                sqlCmd_Unin.Parameters.Add("@Sellable", SqlDbType.TinyInt);
                                if (userStock.Sellable)
                                    sqlCmd_Unin.Parameters["@Sellable"].Value = 1;
                                else
                                    sqlCmd_Unin.Parameters["@Sellable"].Value = 0;
                                sqlCmd_Unin.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Unin.Parameters["@UserID"].Value = userStock.UserID;
                                sqlCmd_Unin.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Unin.Parameters["@StockCode"].Value = userStock.StockCode.Trim();
                                sqlCmd_Unin.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Market"].Value = (byte)userStock.Market;
                                sqlCmd_Unin.ExecuteNonQuery();
                            }
                            else
                            {
                                sqlReader_Unin.Close();
                                sqlCmd_Unin = new SqlCommand("INSERT INTO [UserStocks] (UserID,StockCode,Market,Volume,AveragePrice,Currency,Sellable) " +
                                    "VALUES (@UserID,@StockCode,@Market,@Volume,@AveragePrice,@Currency,@Sellable)", sqlConn_Unin, sqlTrans_Unin);
                                sqlCmd_Unin.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Unin.Parameters["@UserID"].Value = userStock.UserID;
                                sqlCmd_Unin.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Unin.Parameters["@StockCode"].Value = userStock.StockCode.Trim();
                                sqlCmd_Unin.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Market"].Value = (byte)userStock.Market;
                                sqlCmd_Unin.Parameters.Add("@Volume", SqlDbType.Int); sqlCmd_Unin.Parameters["@Volume"].Value = userStock.Volume;
                                sqlCmd_Unin.Parameters.Add("@AveragePrice", SqlDbType.Money); sqlCmd_Unin.Parameters["@AveragePrice"].Value = userStock.AveragePrice;
                                sqlCmd_Unin.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Unin.Parameters["@Currency"].Value = (byte)userStock.Curr;
                                sqlCmd_Unin.Parameters.Add("@Sellable", SqlDbType.TinyInt);
                                if (userStock.Sellable)
                                    sqlCmd_Unin.Parameters["@Sellable"].Value = 1;
                                else
                                    sqlCmd_Unin.Parameters["@Sellable"].Value = 0;
                                sqlCmd_Unin.ExecuteNonQuery();
                            }
                        }
                    }
                }
                sqlTrans_Unin.Commit();
            }
            catch (Exception err)
            {
                Common.DBLog(err, ReplaceSqlPara(sqlCmd_Unin), false);
                Common.Log(err);
                if (sqlReader_Unin != null && !sqlReader_Unin.IsClosed)
                    sqlReader_Unin.Close();
                if (bInTransaction && sqlTrans_Unin != null && sqlTrans_Unin.Connection != null && sqlTrans_Unin.Connection.State == ConnectionState.Open)
                    sqlTrans_Unin.Rollback();
            }
            finally
            {
                if (sqlConn_Unin.State != ConnectionState.Closed)
                    sqlConn_Unin.Close();
            }
        }

        /// <summary>
        /// 新订单追加入队列
        /// </summary>
        /// <param name="userOrder"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public bool OrderAppended(TradingSystem.UserOrders userOrder, int UserID)
        {
            try
            {
                if (UserID != userOrder.UserID)
                {
                    Common.Log("Illegal Append [UserFund.UserID=" + UserID + ";UserOrder.UserID=" + userOrder.UserID + "] @ OrderAppended.");
                    return false;
                }

                bool bExist = false;
                switch (userOrder.OrdStatus)
                {
                    case TradingSystem.OrderStatus.Waiting:
                        {
                            bExist = false;
                            lock (listUserOrders)
                            {
                                for (int i = 0; i < listUserOrders.Count; i++)
                                {
                                    if (listUserOrders[i].OrderID == userOrder.OrderID)
                                    {
                                        bExist = true;
                                        listUserOrders[i] = userOrder;
                                        break;
                                    }
                                }
                                if (!bExist)
                                {
                                    listUserOrders.Add(userOrder);
                                }
                            }
                        }
                        break;
                    default:
                        return false;
                }
                if (Common.stkBuffer != null)
                {
                    RemotingInterface.RI_Order stiOrder = new RemotingInterface.RI_Order();
                    stiOrder.Clear();
                    stiOrder.ExpiredDate = userOrder.ExpiredDate;
                    stiOrder.OrderDate = userOrder.OrderDate;
                    stiOrder.OrderID = userOrder.OrderID;
                    stiOrder.OrderPrice = Common.ConvertPrice(userOrder.OrderPrice);
                    stiOrder.Curr = (RemotingInterface.RI_Currency)userOrder.Curr;
                    stiOrder.OrderStatus = (RemotingInterface.RI_Status)userOrder.OrdStatus;
                    stiOrder.OrderType = (RemotingInterface.RI_Type)userOrder.OrdType;
                    stiOrder.OrderVolume = userOrder.OrderVolume;
                    stiOrder.Side = userOrder.Side;
                    stiOrder.StockCode = userOrder.StockCode.Trim();
                    stiOrder.StockMarket = (RemotingInterface.RI_Market)userOrder.Market;
                    stiOrder.UpdatedDate = userOrder.UpdatedDate;
                    Common.stkBuffer.SetUserOrders(userOrder.UserID, stiOrder);
                }
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        /// <summary>
        /// 订单状态发生改变
        /// </summary>
        /// <param name="userFund"></param>
        /// <param name="userOrder"></param>
        /// <param name="userStock"></param>
        /// <returns></returns>
        public bool OrderChanged(TradingSystem.UserFund userFund, TradingSystem.UserOrders userOrder, TradingSystem.UserStocks userStock)
        {
            try
            {
                if (userFund.UserID != userOrder.UserID)
                {
                    Common.Log("Illegal Trading [UserFund.UserID=" + userFund.UserID + ";UserOrder.UserID=" + userOrder.UserID + "] @ OrderChanged.");
                    return false;
                }
                if (userOrder.OrdStatus == TradingSystem.OrderStatus.Finished 
                    && userStock.UserID != userOrder.UserID)
                {
                    Common.Log("Illegal Trading [UserStock.UserID=" + userStock.UserID + ";UserOrder.UserID=" + userOrder.UserID + "] @ OrderChanged.");
                    return false;
                }

                bool bExist = false;
                switch (userOrder.OrdStatus)
                {
                    case TradingSystem.OrderStatus.Finished:
                        {
                            lock (listUserOrders)
                            {
                                for (int i = 0; i < listUserOrders.Count; i++)
                                {
                                    if (listUserOrders[i].OrderID == userOrder.OrderID)
                                    {
                                        listUserOrders.RemoveAt(i);
                                        break;
                                    }
                                }
                            }

                            bExist = false;
                            lock (listOrdersHistory)
                            {
                                for (int i = 0; i < listOrdersHistory.Count; i++)
                                {
                                    if (listOrdersHistory[i].OrderID == userOrder.OrderID)
                                    {
                                        bExist = true;
                                        listOrdersHistory[i] = userOrder;
                                        break;
                                    }
                                }
                                if (!bExist)
                                    listOrdersHistory.Add(userOrder);
                            }
                            lock (listRemoveOrders)
                            {
                                listRemoveOrders.Add(userOrder.OrderID);
                            }
                            if (userOrder.OrdType == TradingSystem.OrderType.ImmediateOrder
                                || userOrder.OrdType == TradingSystem.OrderType.LimitedOrder)
                            {
                                bExist = false;
                                lock (listUserStocks)
                                {
                                    for (int i = 0; i < listUserStocks.Count; i++)
                                    {
                                        if (listUserStocks[i].UserID == userStock.UserID
                                            && listUserStocks[i].StockCode != null
                                            && string.Compare(userStock.StockCode.Trim(),
                                            listUserStocks[i].StockCode.Trim()) == 0
                                            && userStock.Market == listUserStocks[i].Market)
                                        {
                                            bExist = true;
                                            listUserStocks[i] = userStock;
                                            break;
                                        }
                                    }
                                    if (!bExist)
                                        listUserStocks.Add(userStock);
                                }

                                TradingHistory TradeItem = new TradingHistory();
                                TradeItem.Initialize();
                                TradeItem.OrderID = userOrder.OrderID;
                                TradeItem.Side = userOrder.Side;
                                TradeItem.StockCode = userOrder.StockCode;
                                TradeItem.Market = userOrder.Market;
                                TradeItem.TradeVolume = userOrder.OrderVolume;
                                TradeItem.TradeDate = userOrder.UpdatedDate;
                                if (userOrder.TradePrice > 0)
                                    TradeItem.TradePrice = userOrder.TradePrice;
                                else
                                {
                                    Common.Log("Illegal Trading Price [TradePrice=" + userOrder.TradePrice.ToString("f4") + "].");
                                    TradeItem.TradePrice = userOrder.OrderPrice;
                                }
                                TradeItem.Curr = userOrder.Curr;
                                TradeItem.UserID = userOrder.UserID;
                                lock (listTradingHistory)
                                {
                                    listTradingHistory.Add(TradeItem);
                                }
                            }

                            if (Common.stkBuffer != null)
                            {
                                RemotingInterface.RI_Order stiOrder = new RemotingInterface.RI_Order();
                                stiOrder.Clear();
                                stiOrder.ExpiredDate = userOrder.ExpiredDate;
                                stiOrder.OrderDate = userOrder.OrderDate;
                                stiOrder.OrderID = userOrder.OrderID;
                                stiOrder.OrderPrice = Common.ConvertPrice(userOrder.OrderPrice);
                                stiOrder.TradePrice = Common.ConvertPrice(userOrder.TradePrice);
                                stiOrder.OrderStatus = (RemotingInterface.RI_Status)userOrder.OrdStatus;
                                stiOrder.OrderType = (RemotingInterface.RI_Type)userOrder.OrdType;
                                stiOrder.OrderVolume = userOrder.OrderVolume;
                                stiOrder.Side = userOrder.Side;
                                stiOrder.StockCode = userOrder.StockCode.Trim();
                                stiOrder.StockMarket = (RemotingInterface.RI_Market)userOrder.Market;
                                stiOrder.UpdatedDate = userOrder.UpdatedDate;
                                stiOrder.Curr = (RemotingInterface.RI_Currency)userOrder.Curr;
                                Common.stkBuffer.SetUserOrders(userOrder.UserID, stiOrder);

                                if (userOrder.OrdType == TradingSystem.OrderType.ImmediateOrder
                                    || userOrder.OrdType == TradingSystem.OrderType.LimitedOrder)
                                {
                                    RemotingInterface.RI_Stock stiStock = new RemotingInterface.RI_Stock();
                                    stiStock.Clear();
                                    stiStock.AveragePrice = Common.ConvertPrice(userStock.AveragePrice);
                                    stiStock.Curr = (RemotingInterface.RI_Currency)userStock.Curr;
                                    stiStock.StockCode = userStock.StockCode.Trim();
                                    stiStock.StockMarket = (RemotingInterface.RI_Market)userStock.Market;
                                    stiStock.Volume = userOrder.OrderVolume;
                                    if (userStock.Sellable)
                                        stiStock.SellableVolume = userOrder.OrderVolume;
                                    else
                                        stiStock.SellableVolume = 0;
                                    if (userOrder.Side)
                                        Common.stkBuffer.AddUserStocks(userStock.UserID, stiStock);
                                    else
                                        Common.stkBuffer.SubUserStocks(userStock.UserID, stiStock);

                                    RemotingInterface.RI_Trading stiTrading = new RemotingInterface.RI_Trading();
                                    stiTrading.Clear();
                                    stiTrading.Side = userOrder.Side;
                                    stiTrading.StockCode = userOrder.StockCode.Trim();
                                    stiTrading.StockMarket = (RemotingInterface.RI_Market)userOrder.Market;
                                    stiTrading.TradeVolume = userOrder.OrderVolume;
                                    stiTrading.TradeDate = userOrder.UpdatedDate;
                                    stiTrading.Curr = (RemotingInterface.RI_Currency)userOrder.Curr;
                                    if (userOrder.TradePrice > 0)
                                        stiTrading.TradePrice = Common.ConvertPrice(userOrder.TradePrice);
                                    else
                                        stiTrading.TradePrice = Common.ConvertPrice(userOrder.OrderPrice);
                                    Common.stkBuffer.SetUserTradings(userOrder.UserID, stiTrading);
                                }
                            }
                        }
                        break;
                    case TradingSystem.OrderStatus.Cancelled:
                    case TradingSystem.OrderStatus.Failure:
                    case TradingSystem.OrderStatus.Unknown:
                        {
                            bExist = false;
                            lock (listUserOrders)
                            {
                                for (int i = 0; i < listUserOrders.Count; i++)
                                {
                                    if (listUserOrders[i].OrderID == userOrder.OrderID)
                                    {
                                        listUserOrders.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                            lock (listOrdersHistory)
                            {
                                for (int i = 0; i < listOrdersHistory.Count; i++)
                                {
                                    if (listOrdersHistory[i].OrderID == userOrder.OrderID)
                                    {
                                        bExist = true;
                                        listOrdersHistory[i] = userOrder;
                                        break;
                                    }
                                }
                                if (!bExist)
                                    listOrdersHistory.Add(userOrder);
                            }
                            lock (listRemoveOrders)
                            {
                                listRemoveOrders.Add(userOrder.OrderID);
                            }

                            if (Common.stkBuffer != null)
                            {
                                RemotingInterface.RI_Order stiOrder = new RemotingInterface.RI_Order();
                                stiOrder.Clear();
                                stiOrder.ExpiredDate = userOrder.ExpiredDate;
                                stiOrder.OrderDate = userOrder.OrderDate;
                                stiOrder.OrderID = userOrder.OrderID;
                                stiOrder.OrderPrice = Common.ConvertPrice(userOrder.OrderPrice);
                                stiOrder.OrderStatus = (RemotingInterface.RI_Status)userOrder.OrdStatus;
                                stiOrder.OrderType = (RemotingInterface.RI_Type)userOrder.OrdType;
                                stiOrder.OrderVolume = userOrder.OrderVolume;
                                stiOrder.Side = userOrder.Side;
                                stiOrder.StockCode = userOrder.StockCode.Trim();
                                stiOrder.StockMarket = (RemotingInterface.RI_Market)userOrder.Market;
                                stiOrder.UpdatedDate = userOrder.UpdatedDate;
                                stiOrder.Curr = (RemotingInterface.RI_Currency)userOrder.Curr;
                                Common.stkBuffer.SetUserOrders(userOrder.UserID, stiOrder);
                            }
                        }
                        break;
                    case TradingSystem.OrderStatus.Cancelling:
                    case TradingSystem.OrderStatus.Waiting:
                        {
                            bExist = false;
                            lock (listUserOrders)
                            {
                                for (int i = 0; i < listUserOrders.Count; i++)
                                {
                                    if (listUserOrders[i].OrderID == userOrder.OrderID)
                                    {
                                        bExist = true;
                                        listUserOrders[i] = userOrder;
                                        break;
                                    }
                                }
                                if (!bExist)
                                {
                                    listUserOrders.Add(userOrder);
                                }
                            }

                            if (Common.stkBuffer != null)
                            {
                                RemotingInterface.RI_Order stiOrder = new RemotingInterface.RI_Order();
                                stiOrder.Clear();
                                stiOrder.ExpiredDate = userOrder.ExpiredDate;
                                stiOrder.OrderDate = userOrder.OrderDate;
                                stiOrder.OrderID = userOrder.OrderID;
                                stiOrder.OrderPrice = Common.ConvertPrice(userOrder.OrderPrice);
                                stiOrder.OrderStatus = (RemotingInterface.RI_Status)userOrder.OrdStatus;
                                stiOrder.OrderType = (RemotingInterface.RI_Type)userOrder.OrdType;
                                stiOrder.OrderVolume = userOrder.OrderVolume;
                                stiOrder.Side = userOrder.Side;
                                stiOrder.StockCode = userOrder.StockCode.Trim();
                                stiOrder.StockMarket = (RemotingInterface.RI_Market)userOrder.Market;
                                stiOrder.UpdatedDate = userOrder.UpdatedDate;
                                stiOrder.Curr = (RemotingInterface.RI_Currency)userOrder.Curr;
                                Common.stkBuffer.SetUserOrders(userOrder.UserID, stiOrder);
                            }
                        }
                        break;
                    default:
                        return false;
                }

                bExist = false;
                lock (listUserCash)
                {
                    for (int i = 0; i < listUserCash.Count; i++)
                    {
                        if (listUserCash[i].UserID == userFund.UserID
                            && listUserCash[i].Curr == userFund.Curr)
                        {
                            bExist = true;
                            listUserCash[i] = userFund;
                            break;
                        }
                    }
                    if (!bExist)
                    {
                        listUserCash.Add(userFund);
                    }
                }
                if (Common.stkBuffer != null)
                {
                    RemotingInterface.RI_Fund stiFund = new RemotingInterface.RI_Fund();
                    stiFund.Clear();
                    stiFund.Cash = userFund.Cash;
                    stiFund.UsableCash = userFund.UsableCash;
                    stiFund.Wealth = Common.ConvertPrice(userFund.Wealth);
                    stiFund.Curr = (RemotingInterface.RI_Currency)userFund.Curr;
                    Common.stkBuffer.SetUserFund(userFund.UserID, stiFund);
                }

                try
                {
                    Common.OrderNotifier.UserOrders_Handled(Common.WebService_PlayID, userOrder.UserID, (byte)userOrder.OrdStatus);
                }
                catch 
                {
                    Common.Log("OrderNotifier.RI_UserOrders_Handled() Failed. [" + userOrder.UserID + "/" + userOrder.OrderID + "]");
                }
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        /// <summary>
        /// 更新用户现金记录
        /// </summary>
        /// <param name="userFund"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public bool FundUpdate(TradingSystem.UserFund userFund, int UserID)
        {
            try
            {
                double defaultFund = 0;
                switch(userFund.Curr)
                {
                    case TradingSystem.Currency.AUD:
                        defaultFund = Common.stkTrading.defaultAUD;
                        break;
                    case TradingSystem.Currency.CAD:
                        defaultFund = Common.stkTrading.defaultCAD;
                        break;
                    case TradingSystem.Currency.CHF:
                        defaultFund = Common.stkTrading.defaultCHF;
                        break;
                    case TradingSystem.Currency.EUR:
                        defaultFund = Common.stkTrading.defaultEUR;
                        break;
                    case TradingSystem.Currency.GBP:
                        defaultFund = Common.stkTrading.defaultGBP;
                        break;
                    case TradingSystem.Currency.HKD:
                        defaultFund = Common.stkTrading.defaultHKD;
                        break;
                    case TradingSystem.Currency.JPY:
                        defaultFund = Common.stkTrading.defaultJPY;
                        break;
                    case TradingSystem.Currency.NZD:
                        defaultFund = Common.stkTrading.defaultNZD;
                        break;
                    case TradingSystem.Currency.USD:
                        defaultFund = Common.stkTrading.defaultUSD;
                        break;
                    default:
                        defaultFund = Common.stkTrading.defaultRMB;
                        break;
                }
                if (userFund.UserID != UserID)
                {
                    Common.Log("Illegal Fund Updating [UserFund.UserID=" + userFund.UserID + ";UserOrder.UserID=" + UserID + "] @ FundUpdate.");
                    return false;
                }
                else if (Common.ComparePrice(userFund.Cash + (defaultFund * 0.001), userFund.UsableCash) < 0)
                {
                    Common.Log("Illegal Fund Updating [UserFund.UserID=" + userFund.UserID + ";UserFund.Currency=" + userFund.Curr.ToString().Trim() +
                        ";UserFund.Cash=" + userFund.Cash + ";UserFund.UsableCash=" + userFund.UsableCash + "] @ FundUpdate.");
                    return false;
                }
                else if (Common.ComparePrice(userFund.Cash, userFund.UsableCash) < 0)
                {
                    userFund.UsableCash = userFund.Cash;
                }

                if (Common.stkBuffer != null)
                {
                    RemotingInterface.RI_Fund stiFund = new RemotingInterface.RI_Fund();
                    stiFund.Clear();
                    stiFund.Cash = userFund.Cash;
                    stiFund.UsableCash = userFund.UsableCash;
                    stiFund.Wealth = Common.ConvertPrice(userFund.Wealth);
                    stiFund.Curr = (RemotingInterface.RI_Currency)userFund.Curr;
                    Common.stkBuffer.SetUserFund(userFund.UserID, stiFund);
                }
                lock (listUserUsableCash)
                {
                    for (int i = 0; i < listUserUsableCash.Count; i++)
                    {
                        if (listUserUsableCash[i].UserID == userFund.UserID
                            && listUserUsableCash[i].Curr == userFund.Curr)
                        {
                            listUserUsableCash[i] = userFund;
                            return true;
                        }
                    }
                    listUserUsableCash.Add(userFund);
                }
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        /// <summary>
        /// 追加资金流水记录
        /// </summary>
        /// <param name="fundHistory"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public bool FundChanged(FundHistory fundHistory, int UserID)
        {
            try
            {
                double defaultFund = 0;
                switch (fundHistory.Curr)
                {
                    case TradingSystem.Currency.AUD:
                        defaultFund = Common.stkTrading.defaultAUD;
                        break;
                    case TradingSystem.Currency.CAD:
                        defaultFund = Common.stkTrading.defaultCAD;
                        break;
                    case TradingSystem.Currency.CHF:
                        defaultFund = Common.stkTrading.defaultCHF;
                        break;
                    case TradingSystem.Currency.EUR:
                        defaultFund = Common.stkTrading.defaultEUR;
                        break;
                    case TradingSystem.Currency.GBP:
                        defaultFund = Common.stkTrading.defaultGBP;
                        break;
                    case TradingSystem.Currency.HKD:
                        defaultFund = Common.stkTrading.defaultHKD;
                        break;
                    case TradingSystem.Currency.JPY:
                        defaultFund = Common.stkTrading.defaultJPY;
                        break;
                    case TradingSystem.Currency.NZD:
                        defaultFund = Common.stkTrading.defaultNZD;
                        break;
                    case TradingSystem.Currency.USD:
                        defaultFund = Common.stkTrading.defaultUSD;
                        break;
                    default:
                        defaultFund = Common.stkTrading.defaultRMB;
                        break;
                }
                if (fundHistory.UserID != UserID)
                {
                    Common.Log("Illegal Fund Changing [FundHistory.UserID=" + fundHistory.UserID + ";UserOrder.UserID=" + UserID + "] @ FundChanged.");
                    return false;
                }
                else if (Common.ComparePrice(fundHistory.OriginalCash + fundHistory.ChangedCash, (-0.001 * defaultFund)) < 0)
                {
                    Common.Log("Illegal Fund Changing [FundHistory.UserID=" + fundHistory.UserID + ";UserFund.Currency=" + fundHistory.Curr.ToString().Trim() +
                        ";FundHistory.OriginalCash=" + fundHistory.OriginalCash + ";FundHistory.ChangedCash=" + fundHistory.ChangedCash + "] @ FundChanged.");
                    return false;
                }
                else if (Common.ComparePrice(fundHistory.OriginalCash + fundHistory.ChangedCash, 0) < 0)
                {
                    fundHistory.ChangedCash = (-1 * fundHistory.OriginalCash);
                }
                if (fundHistory.ChangedCash < 0.001 && fundHistory.ChangedCash > -0.001)
                    return false;
                fundHistory.ChangedTime = DateTime.Now;

                if (Common.stkBuffer != null)
                {
                    RemotingInterface.RI_FundChanges stiFundChanges = new RemotingInterface.RI_FundChanges();
                    stiFundChanges.ChangedCash = fundHistory.ChangedCash;
                    stiFundChanges.OrderID = fundHistory.OrderID;
                    stiFundChanges.OriginalCash = fundHistory.OriginalCash;
                    stiFundChanges.Curr = (RemotingInterface.RI_Currency)fundHistory.Curr;
                    stiFundChanges.ChangedDate = fundHistory.ChangedTime;
                    Common.stkBuffer.SetUserFundChanges(fundHistory.UserID, stiFundChanges);
                }
                lock (listFundHistory)
                {
                    listFundHistory.Add(fundHistory);
                }
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        /// <summary>
        /// 记录错误信息
        /// </summary>
        /// <param name="userOrder"></param>
        /// <param name="strDescription"></param>
        /// <returns></returns>
        public bool RecordError(TradingSystem.UserOrders userOrder, string strDescription)
        {
            // 当交易失败时，记录失败原因
            try
            {
                if (strDescription == null)
                    return false;

                switch (userOrder.OrdStatus)
                {
                    case TradingSystem.OrderStatus.Failure:
                        {
                            TradingError TrdErr = new TradingError();
                            TrdErr.Initialize();
                            TrdErr.OrderID = userOrder.OrderID;
                            TrdErr.Description = strDescription;
                            TrdErr.ErrDate = userOrder.UpdatedDate;
                            lock (listTradingError)
                            {
                                listTradingError.Add(TrdErr);
                            }
                        }
                        break;
                    default:
                        return false;
                }
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        
        /// <summary>
        /// 清除数据
        /// </summary>
        /// <returns></returns>
        private bool ClearData()
        {
            bool bInTransaction = false;
            SqlConnection sqlConn_Clr = null;
            SqlTransaction sqlTrans_Clr = null;
            SqlCommand sqlCmd_Clr = null;
            try
            {
               sqlConn_Clr = new SqlConnection(Common.strConn);
               sqlTrans_Clr = sqlConn_Clr.BeginTransaction();
                sqlConn_Clr.Open();
                
                bInTransaction = true;

                sqlCmd_Clr = new SqlCommand("UPDATE [UserOrders] SET OrderStatus = " + (byte)TradingSystem.OrderStatus.Failure
                    + ", UpdatedDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'"
                    + " WHERE OrderID NOT IN (SELECT OrderID FROM [OrdersHistory]) AND (OrderStatus = " + (byte)TradingSystem.OrderStatus.Waiting
                    + " OR OrderStatus = " + (byte)TradingSystem.OrderStatus.Cancelling + ")", sqlConn_Clr, sqlTrans_Clr);
                sqlCmd_Clr.ExecuteNonQuery();

                sqlCmd_Clr = new SqlCommand("INSERT INTO [OrdersHistory] SELECT * FROM [UserOrders]"
                    + " WHERE OrderID NOT IN (SELECT OrderID FROM [OrdersHistory])", sqlConn_Clr, sqlTrans_Clr);
                sqlCmd_Clr.ExecuteNonQuery();

                sqlCmd_Clr = new SqlCommand("DELETE FROM [UserOrders]", sqlConn_Clr, sqlTrans_Clr);
                sqlCmd_Clr.ExecuteNonQuery();

                sqlCmd_Clr = new SqlCommand("UPDATE [UserFund] Set UsableCash = Cash", sqlConn_Clr, sqlTrans_Clr);
                sqlCmd_Clr.ExecuteNonQuery();

                sqlTrans_Clr.Commit();
                return true;
            }
            catch (Exception err)
            {
                Common.DBLog(err, ReplaceSqlPara(sqlCmd_Clr), false);
                Common.Log(err);
                if (sqlReader_Clr != null && !sqlReader_Clr.IsClosed)
                    sqlReader_Clr.Close();
                if (bInTransaction && sqlTrans_Clr != null && sqlTrans_Clr.Connection != null && sqlTrans_Clr.Connection.State == ConnectionState.Open)
                    sqlTrans_Clr.Rollback();
                return false;
            }
            finally
            {
                if (sqlCmd_Clr != null)
                    sqlCmd_Clr.Dispose();
                if (sqlTrans_Clr != null)
                    sqlTrans_Clr.Dispose();
                if (sqlConn_Clr != null)
                    sqlConn_Clr.Dispose();
            }
        }

        /// <summary>
        /// 写入数据库
        /// </summary>
        /// <returns></returns>
        private bool MergeDB()
        {
            bool bInTransaction = false;
            string strCurrSql = "";
            try
            {
                if (sqlConn_Mrg.State == ConnectionState.Closed)
                    sqlConn_Mrg.Open();
                sqlTrans_Mrg = sqlConn_Mrg.BeginTransaction();
                bInTransaction = true;

                List<TradingSystem.UserStocks> listUserStocks = new List<TradingSystem.UserStocks>();
                if (DateTime.Now.TimeOfDay >= Common.BeginAMTS &&
                    DateTime.Now.TimeOfDay <= Common.EndPMTS)
                {
                    sqlCmd_Mrg = new SqlCommand("SELECT UserID, StockCode, Market, Currency, SUM(AveragePrice * Volume) AS Val, SUM(Volume) AS Vol, " +
                       "Sellable FROM [UserStocks] WHERE (UserID IN (SELECT UserID FROM [UserFund])) " +
                       "AND (Volume > 0) GROUP BY UserID, StockCode, Market, Currency, Sellable", sqlConn_Mrg, sqlTrans_Mrg);
                    sqlReader_Mrg = sqlCmd_Mrg.ExecuteReader();
                    while (sqlReader_Mrg.Read())
                    {
                        TradingSystem.UserStocks userStock = new TradingSystem.UserStocks();
                        userStock.Initialize();
                        userStock.UserID = (int)sqlReader_Mrg["UserID"];
                        userStock.StockCode = sqlReader_Mrg["StockCode"].ToString().Trim();
                        userStock.Market = (TradingSystem.StockMarket)Convert.ToByte(sqlReader_Mrg["Market"].ToString().Trim());
                        userStock.Curr = (TradingSystem.Currency)Convert.ToByte(sqlReader_Mrg["Currency"].ToString().Trim());
                        userStock.Volume = (int)sqlReader_Mrg["Vol"];
                        if (userStock.Volume > 0)
                            userStock.AveragePrice = Common.ConvertPrice(double.Parse(sqlReader_Mrg["Val"].ToString().Trim()) / userStock.Volume);
                        else
                            userStock.AveragePrice = 0;
                        if (sqlReader_Mrg["Sellable"].ToString().Trim() == "1")
                            userStock.Sellable = true;
                        else
                            userStock.Sellable = false;
                        listUserStocks.Add(userStock);
                    }
                }
                else
                {
                    sqlCmd_Mrg = new SqlCommand("SELECT UserID, StockCode, Market, Currency, SUM(AveragePrice * Volume) AS Val, SUM(Volume) AS Vol " +
                        "FROM [UserStocks] WHERE (UserID IN (SELECT UserID FROM [UserFund])) " +
                        "AND (Volume > 0) GROUP BY UserID, StockCode, Market, Currency", sqlConn_Mrg, sqlTrans_Mrg);
                    sqlReader_Mrg = sqlCmd_Mrg.ExecuteReader();
                    while (sqlReader_Mrg.Read())
                    {
                        TradingSystem.UserStocks userStock = new TradingSystem.UserStocks();
                        userStock.Initialize();
                        userStock.UserID = (int)sqlReader_Mrg["UserID"];
                        userStock.StockCode = sqlReader_Mrg["StockCode"].ToString().Trim();
                        userStock.Market = (TradingSystem.StockMarket)Convert.ToByte(sqlReader_Mrg["Market"].ToString().Trim());
                        userStock.Curr = (TradingSystem.Currency)Convert.ToByte(sqlReader_Mrg["Currency"].ToString().Trim());
                        userStock.Volume = (int)sqlReader_Mrg["Vol"];
                        if (userStock.Volume > 0)
                            userStock.AveragePrice = Common.ConvertPrice(double.Parse(sqlReader_Mrg["Val"].ToString().Trim()) / userStock.Volume);
                        else
                            userStock.AveragePrice = 0;
                        userStock.Sellable = true;
                        listUserStocks.Add(userStock);
                    }
                }
                sqlReader_Mrg.Close();

                sqlCmd_Mrg = new SqlCommand("DELETE FROM [UserStocks]", sqlConn_Mrg, sqlTrans_Mrg);
                sqlCmd_Mrg.ExecuteNonQuery();

                foreach (TradingSystem.UserStocks data in listUserStocks)
                {
                    strCurrSql = "INSERT INTO [UserStocks] (UserID, StockCode, Market, Volume, AveragePrice, Currency, Sellable) " +
                        "VALUES (@UserID, @StockCode, @Market, @Volume, @AveragePrice, @Currency, @Sellable)";
                    sqlCmd_Mrg = new SqlCommand(strCurrSql, sqlConn_Mrg, sqlTrans_Mrg);
                    sqlCmd_Mrg.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Mrg.Parameters["@UserID"].Value = data.UserID;
                    sqlCmd_Mrg.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Mrg.Parameters["@StockCode"].Value = data.StockCode.Trim();
                    sqlCmd_Mrg.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Mrg.Parameters["@Market"].Value = (byte)data.Market;
                    sqlCmd_Mrg.Parameters.Add("@Volume", SqlDbType.Int); sqlCmd_Mrg.Parameters["@Volume"].Value = data.Volume;
                    sqlCmd_Mrg.Parameters.Add("@AveragePrice", SqlDbType.Money); sqlCmd_Mrg.Parameters["@AveragePrice"].Value = data.AveragePrice;
                    sqlCmd_Mrg.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Mrg.Parameters["@Currency"].Value = (byte)data.Curr;
                    sqlCmd_Mrg.Parameters.Add("@Sellable", SqlDbType.TinyInt);
                    if (data.Sellable)
                        sqlCmd_Mrg.Parameters["@Sellable"].Value = 1;
                    else
                        sqlCmd_Mrg.Parameters["@Sellable"].Value = 0;

                    sqlCmd_Mrg.ExecuteNonQuery();
                }
                sqlTrans_Mrg.Commit();

                Common.Log("The Database Has Been Merged.");
                return true;
            }
            catch (Exception err)
            {
                Common.DBLog(err, ReplaceSqlPara(sqlCmd_Mrg),false);
                Common.Log(err);
                if (sqlReader_Mrg != null && !sqlReader_Mrg.IsClosed)
                    sqlReader_Mrg.Close();
                if (bInTransaction && sqlTrans_Mrg != null && sqlTrans_Mrg.Connection != null && sqlTrans_Mrg.Connection.State == ConnectionState.Open)
                    sqlTrans_Mrg.Rollback();
                return false;
            }
            finally
            {
                if (sqlConn_Mrg.State != ConnectionState.Closed)
                    sqlConn_Mrg.Close();
            }
        }

        /// <summary>
        /// 数据同步
        /// </summary>
        private void Synchronizing()
        {
            try
            {
                bool bInTransaction = false;
                string strCurrSQL = "";
                while (bSync)
                {
                    try
                    {
                        if ((Common.IsWeekend || DateTime.Now.TimeOfDay < Common.BeginAMTS
                            || DateTime.Now.TimeOfDay > Common.EndPMTS.Add(new TimeSpan(0, 10, 0))) && !bDataCleared)
                        {
                            bDataCleared = ClearData();
                            Thread.Sleep(30000);
                            continue;
                        }
                        else if (!Common.IsWeekend && DateTime.Now.TimeOfDay >= Common.BeginAMTS && bDataCleared)
                        {
                            bDataCleared = false;
                        }
                        else if (uSyncFlag < 15)
                        {
                            uSyncFlag++;
                            Thread.Sleep(1000);
                            continue;
                        }

                        uSyncFlag = 0;
                        bInTransaction = false;
                        if (listDBOrdersHistory.Count <= 0 && listDBTradingHistory.Count <= 0 && listDBUserFund.Count <= 0
                            && listDBUserOrders.Count <= 0 && listDBRemoveOrders.Count <= 0 && listDBTradingError.Count <= 0
                            && listDBUserUsableFund.Count <= 0 && listDBUserStocks.Count <= 0 && listDBNewUserFund.Count <= 0
                            && listDBFundHistory.Count <= 0)
                        {
                            lock (listOrdersHistory)
                            {
                                if (listOrdersHistory.Count > 0)
                                {
                                    listDBOrdersHistory = new List<TradingSystem.UserOrders>(listOrdersHistory);
                                    listOrdersHistory.Clear();
                                }
                            }
                            lock (listTradingHistory)
                            {
                                if (listTradingHistory.Count > 0)
                                {
                                    listDBTradingHistory = new List<TradingHistory>(listTradingHistory);
                                    listTradingHistory.Clear();
                                }
                            }
                            lock (listUserCash)
                            {
                                if (listUserCash.Count > 0)
                                {
                                    listDBUserFund = new List<TradingSystem.UserFund>(listUserCash);
                                    listUserCash.Clear();
                                }
                            }
                            lock (listUserUsableCash)
                            {
                                if (listUserUsableCash.Count > 0)
                                {
                                    listDBUserUsableFund = new List<TradingSystem.UserFund>(listUserUsableCash);
                                    listUserUsableCash.Clear();
                                }
                            }
                            lock (listNewUserFund)
                            {
                                if (listNewUserFund.Count > 0)
                                {
                                    listDBNewUserFund = new List<TradingSystem.UserFund>(listNewUserFund);
                                    listNewUserFund.Clear();
                                }
                            }
                            lock (listUserOrders)
                            {
                                if (listUserOrders.Count > 0)
                                {
                                    listDBUserOrders = new List<TradingSystem.UserOrders>(listUserOrders);
                                    listUserOrders.Clear();
                                }
                            }
                            lock (listRemoveOrders)
                            {
                                if (listRemoveOrders.Count > 0)
                                {
                                    listDBRemoveOrders = new List<int>(listRemoveOrders);
                                    listRemoveOrders.Clear();
                                }
                            }
                            lock (listTradingError)
                            {
                                if (listTradingError.Count > 0)
                                {
                                    listDBTradingError = new List<TradingError>(listTradingError);
                                    listTradingError.Clear();
                                }
                            }
                            lock (listUserStocks)
                            {
                                if (listUserStocks.Count > 0)
                                {
                                    listDBUserStocks = new List<TradingSystem.UserStocks>(listUserStocks);
                                    listUserStocks.Clear();
                                }
                            }
                            lock (listFundHistory)
                            {
                                if (listFundHistory.Count > 0)
                                {
                                    listDBFundHistory = new List<FundHistory>(listFundHistory);
                                    listFundHistory.Clear();
                                }
                            }
                        }

                        if (listDBOrdersHistory.Count > 0 || listDBTradingHistory.Count > 0 || listDBUserFund.Count > 0 ||
                            listDBUserOrders.Count > 0 || listDBRemoveOrders.Count > 0 || listDBTradingError.Count > 0
                            || listDBUserUsableFund.Count > 0 || listDBUserStocks.Count > 0 || listDBNewUserFund.Count > 0
                            || listDBFundHistory.Count > 0)
                        {
                            if (sqlConn_Sync.State == ConnectionState.Closed)
                                sqlConn_Sync.Open();
                            sqlTrans_Sync = sqlConn_Sync.BeginTransaction();
                            bInTransaction = true;

                            foreach (TradingSystem.UserFund data in listDBUserFund)
                            {
                                try
                                {
                                    strCurrSQL = "UPDATE [UserFund] SET Cash = @Cash WHERE (UserID = @UserID) AND (Currency = @Currency)";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@Cash", SqlDbType.Money); sqlCmd_Sync.Parameters["@Cash"].Value = data.Cash;
                                    sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                    sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                    strCurrSQL = strCurrSQL.Replace("@Cash", data.Cash.ToString("f3")).Replace("@UserID", data.UserID.ToString()).Replace("@Currency", ((byte)data.Curr).ToString());
                                    sqlCmd_Sync.ExecuteNonQuery();
                                    Common.DBLog(null, strCurrSQL, true);
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (TradingSystem.UserFund data in listDBUserUsableFund)
                            {
                                try
                                {
                                    strCurrSQL = "UPDATE [UserFund] SET UsableCash = @UsableCash WHERE (UserID = @UserID) AND (Currency = @Currency)";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@UsableCash", SqlDbType.Money); sqlCmd_Sync.Parameters["@UsableCash"].Value = data.UsableCash;
                                    sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                    sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                    strCurrSQL = strCurrSQL.Replace("@UsableCash", data.UsableCash.ToString("f3")).Replace("@UserID", data.UserID.ToString()).Replace("@Currency", ((byte)data.Curr).ToString());
                                    sqlCmd_Sync.ExecuteNonQuery();
                                    Common.DBLog(null, strCurrSQL, true);
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (TradingSystem.UserFund data in listDBNewUserFund)
                            {
                                try
                                {
                                    strCurrSQL = "SELECT UserID FROM [UserFund] WHERE (UserID = @UserID) AND (Currency = @Currency)";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                    sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                    strCurrSQL = strCurrSQL.Replace("@UserID", data.UserID.ToString()).Replace("@Currency", ((byte)data.Curr).ToString());
                                    sqlReader_Sync = sqlCmd_Sync.ExecuteReader();
                                    if (sqlReader_Sync.Read())
                                    {
                                        sqlReader_Sync.Close();
                                    }
                                    else
                                    {
                                        sqlReader_Sync.Close();
                                        strCurrSQL = "INSERT INTO [UserFund] (UserID, Cash, UsableCash, Currency) VALUES (@UserID, @Cash, @UsableCash, @Currency)";
                                        sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                        sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                        sqlCmd_Sync.Parameters.Add("@Cash", SqlDbType.Money); sqlCmd_Sync.Parameters["@Cash"].Value = data.Cash;
                                        sqlCmd_Sync.Parameters.Add("@UsableCash", SqlDbType.Money); sqlCmd_Sync.Parameters["@UsableCash"].Value = data.UsableCash;
                                        sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                        strCurrSQL = strCurrSQL.Replace("@Cash", data.Cash.ToString("f3")).Replace("@UsableCash", data.UsableCash.ToString("f3"))
                                            .Replace("@UserID", data.UserID.ToString()).Replace("@Currency", ((byte)data.Curr).ToString());
                                        sqlCmd_Sync.ExecuteNonQuery();
                                        Common.DBLog(null, strCurrSQL, true);
                                    }
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (TradingSystem.UserOrders data in listDBUserOrders)
                            {
                                try
                                {
                                    strCurrSQL = "SELECT OrderID FROM [UserOrders] WHERE OrderID = @OrderID";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                    strCurrSQL = strCurrSQL.Replace("@OrderID", data.OrderID.ToString());
                                    sqlReader_Sync = sqlCmd_Sync.ExecuteReader();
                                    if (sqlReader_Sync.Read())
                                    {
                                        sqlReader_Sync.Close();
                                        strCurrSQL = "UPDATE [UserOrders] SET OrderType = @OrderType,OrderStatus = @OrderStatus,OrderSide = @OrderSide,Currency = @Currency" +
                                            "StockCode = @StockCode,Market = @Market,OrderVolume = @OrderVolume,OrderPrice = @OrderPrice,TradePrice = @TradePrice,OrderDate = @OrderDate," +
                                            "UpdatedDate = @UpdatedDate,ExpiredDate = @ExpiredDate WHERE (OrderID = @OrderID) AND (UserID = @UserID)";
                                        sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                        sqlCmd_Sync.Parameters.Add("@OrderType", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@OrderType"].Value = (byte)data.OrdType;
                                        sqlCmd_Sync.Parameters.Add("@OrderStatus", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@OrderStatus"].Value = (byte)data.OrdStatus;
                                        if (data.Side)
                                        {
                                            sqlCmd_Sync.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Sync.Parameters["@OrderSide"].Value = (byte)1;
                                        }
                                        else
                                        {
                                            sqlCmd_Sync.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Sync.Parameters["@OrderSide"].Value = (byte)0;
                                        }
                                        sqlCmd_Sync.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Sync.Parameters["@StockCode"].Value = data.StockCode.Trim();
                                        sqlCmd_Sync.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Market"].Value = (byte)data.Market;
                                        sqlCmd_Sync.Parameters.Add("@OrderVolume", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderVolume"].Value = data.OrderVolume;
                                        sqlCmd_Sync.Parameters.Add("@OrderPrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@OrderPrice"].Value = data.OrderPrice;
                                        sqlCmd_Sync.Parameters.Add("@TradePrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@TradePrice"].Value = data.TradePrice;
                                        sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                        sqlCmd_Sync.Parameters.Add("@OrderDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@OrderDate"].Value = data.OrderDate;
                                        sqlCmd_Sync.Parameters.Add("@UpdatedDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@UpdatedDate"].Value = data.UpdatedDate;
                                        sqlCmd_Sync.Parameters.Add("@ExpiredDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@ExpiredDate"].Value = data.ExpiredDate;
                                        sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                        sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                        strCurrSQL = strCurrSQL.Replace("@OrderType", ((byte)data.OrdType).ToString()).Replace("@OrderStatus", ((byte)data.OrdStatus).ToString()).Replace("@OrderSide", data.Side.ToString())
                                            .Replace("@StockCode", data.StockCode).Replace("@Market", ((byte)data.Market).ToString()).Replace("@OrderVolume", data.OrderVolume.ToString()).Replace("@OrderPrice", data.OrderPrice.ToString("f3"))
                                            .Replace("@TradePrice", data.TradePrice.ToString("f3")).Replace("@Currency", ((byte)data.Curr).ToString()).Replace("@OrderDate", data.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"))
                                            .Replace("@UpdatedDate", data.UpdatedDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("@ExpiredDate", data.ExpiredDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("@OrderID", data.OrderID.ToString())
                                            .Replace("@UserID", data.UserID.ToString());
                                        sqlCmd_Sync.ExecuteNonQuery();
                                        Common.DBLog(null, strCurrSQL, true);
                                    }
                                    else
                                    {
                                        sqlReader_Sync.Close();
                                        strCurrSQL = "SELECT OrderID FROM [OrdersHistory] WHERE OrderID = @OrderID";
                                        sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                        sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                        strCurrSQL = strCurrSQL.Replace("@OrderID", data.OrderID.ToString());
                                        sqlReader_Sync = sqlCmd_Sync.ExecuteReader();
                                        if (!sqlReader_Sync.Read())
                                        {
                                            sqlReader_Sync.Close();
                                            strCurrSQL = "INSERT INTO [UserOrders] (OrderID,UserID,OrderType,OrderStatus,OrderSide,StockCode," +
                                                "Market,OrderVolume,OrderPrice,TradePrice,Currency,OrderDate,UpdatedDate,ExpiredDate) " +
                                                "VALUES (@OrderID,@UserID,@OrderType,@OrderStatus,@OrderSide,@StockCode,@Market," +
                                                "@OrderVolume,@OrderPrice,@TradePrice,@Currency,@OrderDate,@UpdatedDate,@ExpiredDate)";
                                            sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                            sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                            sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                            sqlCmd_Sync.Parameters.Add("@OrderType", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@OrderType"].Value = (byte)data.OrdType;
                                            sqlCmd_Sync.Parameters.Add("@OrderStatus", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@OrderStatus"].Value = (byte)data.OrdStatus;
                                            if (data.Side)
                                            {
                                                sqlCmd_Sync.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Sync.Parameters["@OrderSide"].Value = (byte)1;
                                            }
                                            else
                                            {
                                                sqlCmd_Sync.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Sync.Parameters["@OrderSide"].Value = (byte)0;
                                            }
                                            sqlCmd_Sync.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Sync.Parameters["@StockCode"].Value = data.StockCode.Trim();
                                            sqlCmd_Sync.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Market"].Value = (byte)data.Market;
                                            sqlCmd_Sync.Parameters.Add("@OrderVolume", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderVolume"].Value = data.OrderVolume;
                                            sqlCmd_Sync.Parameters.Add("@OrderPrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@OrderPrice"].Value = data.OrderPrice;
                                            sqlCmd_Sync.Parameters.Add("@TradePrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@TradePrice"].Value = data.TradePrice;
                                            sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                            sqlCmd_Sync.Parameters.Add("@OrderDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@OrderDate"].Value = data.OrderDate;
                                            sqlCmd_Sync.Parameters.Add("@UpdatedDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@UpdatedDate"].Value = data.UpdatedDate;
                                            sqlCmd_Sync.Parameters.Add("@ExpiredDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@ExpiredDate"].Value = data.ExpiredDate;
                                            strCurrSQL = strCurrSQL.Replace("@OrderType", ((byte)data.OrdType).ToString()).Replace("@OrderStatus", ((byte)data.OrdStatus).ToString()).Replace("@OrderSide", data.Side.ToString())
                                                .Replace("@StockCode", data.StockCode).Replace("@Market", ((byte)data.Market).ToString()).Replace("@OrderVolume", data.OrderVolume.ToString()).Replace("@OrderPrice", data.OrderPrice.ToString("f3"))
                                                .Replace("@TradePrice", data.TradePrice.ToString("f3")).Replace("@Currency", ((byte)data.Curr).ToString()).Replace("@OrderDate", data.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"))
                                                .Replace("@UpdatedDate", data.UpdatedDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("@ExpiredDate", data.ExpiredDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("@OrderID", data.OrderID.ToString())
                                                .Replace("@UserID", data.UserID.ToString());
                                            sqlCmd_Sync.ExecuteNonQuery();
                                            Common.DBLog(null, strCurrSQL, true);
                                        }
                                        else
                                            sqlReader_Sync.Close();
                                    }
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (TradingSystem.UserOrders data in listDBOrdersHistory)
                            {
                                try
                                {
                                    strCurrSQL = "SELECT OrderID FROM [OrdersHistory] WHERE OrderID = @OrderID";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                    strCurrSQL = strCurrSQL.Replace("@OrderID", data.OrderID.ToString());
                                    sqlReader_Sync = sqlCmd_Sync.ExecuteReader();
                                    if (!sqlReader_Sync.Read())
                                    {
                                        sqlReader_Sync.Close();
                                        strCurrSQL = "INSERT INTO [OrdersHistory] (OrderID,UserID,OrderType,OrderStatus,OrderSide,StockCode," +
                                              "Market,OrderVolume,OrderPrice,TradePrice,Currency,OrderDate,UpdatedDate,ExpiredDate) " +
                                              "VALUES (@OrderID,@UserID,@OrderType,@OrderStatus,@OrderSide,@StockCode,@Market," +
                                              "@OrderVolume,@OrderPrice,@TradePrice,@Currency,@OrderDate,@UpdatedDate,@ExpiredDate)";
                                        sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                        sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                        sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                        sqlCmd_Sync.Parameters.Add("@OrderType", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@OrderType"].Value = (byte)data.OrdType;
                                        sqlCmd_Sync.Parameters.Add("@OrderStatus", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@OrderStatus"].Value = (byte)data.OrdStatus;
                                        if (data.Side)
                                        {
                                            sqlCmd_Sync.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Sync.Parameters["@OrderSide"].Value = (byte)1;
                                        }
                                        else
                                        {
                                            sqlCmd_Sync.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Sync.Parameters["@OrderSide"].Value = (byte)0;
                                        }
                                        sqlCmd_Sync.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Sync.Parameters["@StockCode"].Value = data.StockCode.Trim();
                                        sqlCmd_Sync.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Market"].Value = (byte)data.Market;
                                        sqlCmd_Sync.Parameters.Add("@OrderVolume", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderVolume"].Value = data.OrderVolume;
                                        sqlCmd_Sync.Parameters.Add("@OrderPrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@OrderPrice"].Value = data.OrderPrice;
                                        sqlCmd_Sync.Parameters.Add("@TradePrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@TradePrice"].Value = data.TradePrice;
                                        sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                        sqlCmd_Sync.Parameters.Add("@OrderDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@OrderDate"].Value = data.OrderDate;
                                        sqlCmd_Sync.Parameters.Add("@UpdatedDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@UpdatedDate"].Value = data.UpdatedDate;
                                        sqlCmd_Sync.Parameters.Add("@ExpiredDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@ExpiredDate"].Value = data.ExpiredDate;
                                        strCurrSQL = strCurrSQL.Replace("@OrderType", ((byte)data.OrdType).ToString()).Replace("@OrderStatus", ((byte)data.OrdStatus).ToString()).Replace("@OrderSide", data.Side.ToString())
                                            .Replace("@StockCode", data.StockCode).Replace("@Market", ((byte)data.Market).ToString()).Replace("@OrderVolume", data.OrderVolume.ToString()).Replace("@OrderPrice", data.OrderPrice.ToString("f3"))
                                            .Replace("@TradePrice", data.TradePrice.ToString("f3")).Replace("@Currency", ((byte)data.Curr).ToString()).Replace("@OrderDate", data.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"))
                                            .Replace("@UpdatedDate", data.UpdatedDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("@ExpiredDate", data.ExpiredDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("@OrderID", data.OrderID.ToString())
                                            .Replace("@UserID", data.UserID.ToString());
                                        sqlCmd_Sync.ExecuteNonQuery();
                                        Common.DBLog(null, strCurrSQL, true);
                                    }
                                    else
                                        sqlReader_Sync.Close();
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (TradingHistory data in listDBTradingHistory)
                            {
                                try
                                {
                                    strCurrSQL = "INSERT INTO [TradingHistory] (OrderID,UserID," +
                                        "OrderSide,StockCode,Market,TradeVolume,TradePrice,Currency,TradeDate) " +
                                        "VALUES (@OrderID,@UserID,@OrderSide,@StockCode,@Market," +
                                        "@TradeVolume,@TradePrice,@Currency,@TradeDate)";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                    sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                    if (data.Side)
                                    {
                                        sqlCmd_Sync.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Sync.Parameters["@OrderSide"].Value = (byte)1;
                                    }
                                    else
                                    {
                                        sqlCmd_Sync.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Sync.Parameters["@OrderSide"].Value = (byte)0;
                                    }
                                    sqlCmd_Sync.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Sync.Parameters["@StockCode"].Value = data.StockCode.Trim();
                                    sqlCmd_Sync.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Market"].Value = (byte)data.Market;
                                    sqlCmd_Sync.Parameters.Add("@TradeVolume", SqlDbType.Int); sqlCmd_Sync.Parameters["@TradeVolume"].Value = data.TradeVolume;
                                    sqlCmd_Sync.Parameters.Add("@TradePrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@TradePrice"].Value = data.TradePrice;
                                    sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                    sqlCmd_Sync.Parameters.Add("@TradeDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@TradeDate"].Value = data.TradeDate;
                                    strCurrSQL = strCurrSQL.Replace("@OrderSide", data.Side.ToString())
                                        .Replace("@StockCode", data.StockCode).Replace("@Market", ((byte)data.Market).ToString()).Replace("@TradeVolume", data.TradeVolume.ToString())
                                        .Replace("@TradePrice", data.TradePrice.ToString("f3")).Replace("@Currency", ((byte)data.Curr).ToString())
                                        .Replace("@TradeDate", data.TradeDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("@OrderID", data.OrderID.ToString())
                                        .Replace("@UserID", data.UserID.ToString());
                                    sqlCmd_Sync.ExecuteNonQuery();
                                    Common.DBLog(null, strCurrSQL, true);
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (int data in listDBRemoveOrders)
                            {
                                try
                                {
                                    strCurrSQL = "DELETE [UserOrders] WHERE OrderID = @OrderID";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data;
                                    strCurrSQL = strCurrSQL.Replace("@OrderID", data.ToString());
                                    sqlCmd_Sync.ExecuteNonQuery();
                                    Common.DBLog(null, strCurrSQL, true);
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (TradingError data in listDBTradingError)
                            {
                                try
                                {
                                    strCurrSQL = "INSERT INTO [TradingError] (OrderID,Description,ErrorDate) " +
                                        "VALUES (@OrderID,@Description,@ErrorDate)";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                    sqlCmd_Sync.Parameters.Add("@Description", SqlDbType.NVarChar, 128); sqlCmd_Sync.Parameters["@Description"].Value = data.Description;
                                    sqlCmd_Sync.Parameters.Add("@ErrorDate", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@ErrorDate"].Value = data.ErrDate;
                                    strCurrSQL = strCurrSQL.Replace("@ErrorDate", data.ErrDate.ToString("yyyy-MM-dd HH:mm:ss"))
                                        .Replace("@OrderID", data.OrderID.ToString()).Replace("@Description", data.Description.Trim());
                                    sqlCmd_Sync.ExecuteNonQuery();
                                    Common.DBLog(null, strCurrSQL, true);
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (TradingSystem.UserStocks data in listDBUserStocks)
                            {
                                try
                                {
                                    strCurrSQL = "SELECT UserID, StockCode FROM [UserStocks] WHERE (UserID = @UserID) AND " +
                                        "(StockCode = @StockCode) AND (Market = @Market) AND (Sellable = @Sellable)";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                    sqlCmd_Sync.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Sync.Parameters["@StockCode"].Value = data.StockCode.Trim();
                                    sqlCmd_Sync.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Market"].Value = (byte)data.Market;
                                    sqlCmd_Sync.Parameters.Add("@Sellable", SqlDbType.TinyInt);
                                    if (data.Sellable)
                                        sqlCmd_Sync.Parameters["@Sellable"].Value = (byte)1;
                                    else
                                        sqlCmd_Sync.Parameters["@Sellable"].Value = (byte)0;
                                    strCurrSQL = strCurrSQL.Replace("@Sellable", (data.Sellable).ToString())
                                        .Replace("@StockCode", data.StockCode).Replace("@Market", ((byte)data.Market).ToString())
                                        .Replace("@UserID", data.UserID.ToString());
                                    sqlReader_Sync = sqlCmd_Sync.ExecuteReader();
                                    if (sqlReader_Sync.Read())
                                    {
                                        sqlReader_Sync.Close();
                                        if (data.Volume > 0)
                                        {
                                            strCurrSQL = "UPDATE [UserStocks] SET Volume = @Volume, AveragePrice = @AveragePrice, Currency = @Currency WHERE " +
                                                "(UserID = @UserID) AND (StockCode = @StockCode) AND (Market = @Market) AND (Sellable = @Sellable)";
                                            sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                            sqlCmd_Sync.Parameters.Add("@Volume", SqlDbType.Int); sqlCmd_Sync.Parameters["@Volume"].Value = data.Volume;
                                            sqlCmd_Sync.Parameters.Add("@AveragePrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@AveragePrice"].Value = data.AveragePrice;
                                            sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                            sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                            sqlCmd_Sync.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Sync.Parameters["@StockCode"].Value = data.StockCode.Trim();
                                            sqlCmd_Sync.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Market"].Value = (byte)data.Market;
                                            sqlCmd_Sync.Parameters.Add("@Sellable", SqlDbType.TinyInt);
                                            if (data.Sellable)
                                                sqlCmd_Sync.Parameters["@Sellable"].Value = (byte)1;
                                            else
                                                sqlCmd_Sync.Parameters["@Sellable"].Value = (byte)0;
                                        }
                                        else
                                        {
                                            strCurrSQL = "UPDATE [UserStocks] SET Volume = 0, AveragePrice = 0 WHERE " +
                                                "(UserID = @UserID) AND (StockCode = @StockCode) AND (Market = @Market) AND (Sellable = @Sellable)";
                                            sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                            sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                            sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                            sqlCmd_Sync.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Sync.Parameters["@StockCode"].Value = data.StockCode.Trim();
                                            sqlCmd_Sync.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Market"].Value = (byte)data.Market;
                                            sqlCmd_Sync.Parameters.Add("@Sellable", SqlDbType.TinyInt);
                                            if (data.Sellable)
                                                sqlCmd_Sync.Parameters["@Sellable"].Value = (byte)1;
                                            else
                                                sqlCmd_Sync.Parameters["@Sellable"].Value = (byte)0;
                                        }
                                        strCurrSQL = strCurrSQL.Replace("@Volume", data.Volume.ToString()).Replace("@AveragePrice", data.AveragePrice.ToString("f3"))
                                            .Replace("@StockCode", data.StockCode).Replace("@Market", ((byte)data.Market).ToString()).Replace("@Sellable", (data.Sellable).ToString())
                                            .Replace("@Currency", ((byte)data.Curr).ToString()).Replace("@UserID", data.UserID.ToString());
                                        sqlCmd_Sync.ExecuteNonQuery();
                                        Common.DBLog(null, strCurrSQL, true);
                                    }
                                    else
                                    {
                                        sqlReader_Sync.Close();
                                        strCurrSQL = "INSERT INTO [UserStocks] (UserID,StockCode,Market,Volume,AveragePrice,Currency,Sellable) " +
                                            "VALUES (@UserID,@StockCode,@Market,@Volume,@AveragePrice,@Currency,@Sellable)";
                                        sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                        sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                        sqlCmd_Sync.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Sync.Parameters["@StockCode"].Value = data.StockCode.Trim();
                                        sqlCmd_Sync.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Market"].Value = (byte)data.Market;
                                        sqlCmd_Sync.Parameters.Add("@Volume", SqlDbType.Int); sqlCmd_Sync.Parameters["@Volume"].Value = data.Volume;
                                        sqlCmd_Sync.Parameters.Add("@AveragePrice", SqlDbType.Money); sqlCmd_Sync.Parameters["@AveragePrice"].Value = data.AveragePrice;
                                        sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                        sqlCmd_Sync.Parameters.Add("@Sellable", SqlDbType.TinyInt);
                                        if (data.Sellable)
                                            sqlCmd_Sync.Parameters["@Sellable"].Value = (byte)1;
                                        else
                                            sqlCmd_Sync.Parameters["@Sellable"].Value = (byte)0;
                                        strCurrSQL = strCurrSQL.Replace("@Volume", data.Volume.ToString()).Replace("@AveragePrice", data.AveragePrice.ToString("f3"))
                                            .Replace("@StockCode", data.StockCode).Replace("@Market", ((byte)data.Market).ToString()).Replace("@Sellable", (data.Sellable).ToString())
                                            .Replace("@Currency", ((byte)data.Curr).ToString()).Replace("@UserID", data.UserID.ToString());
                                        sqlCmd_Sync.ExecuteNonQuery();
                                        Common.DBLog(null, strCurrSQL, true);
                                    }
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            foreach (FundHistory data in listDBFundHistory)
                            {
                                try
                                {
                                    strCurrSQL = "INSERT INTO [FundHistory] (UserID,OriginalCash,ChangedCash,Currency,ChangedTime,OrderID) " +
                                        "VALUES (@UserID,@OriginalCash,@ChangedCash,@Currency,@ChangedTime,@OrderID)";
                                    sqlCmd_Sync = new SqlCommand(strCurrSQL, sqlConn_Sync, sqlTrans_Sync);
                                    sqlCmd_Sync.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Sync.Parameters["@UserID"].Value = data.UserID;
                                    sqlCmd_Sync.Parameters.Add("@OriginalCash", SqlDbType.Money); sqlCmd_Sync.Parameters["@OriginalCash"].Value = data.OriginalCash;
                                    sqlCmd_Sync.Parameters.Add("@ChangedCash", SqlDbType.Money); sqlCmd_Sync.Parameters["@ChangedCash"].Value = data.ChangedCash;
                                    sqlCmd_Sync.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Sync.Parameters["@Currency"].Value = (byte)data.Curr;
                                    sqlCmd_Sync.Parameters.Add("@ChangedTime", SqlDbType.DateTime); sqlCmd_Sync.Parameters["@ChangedTime"].Value = data.ChangedTime.ToString("yyyy-MM-dd HH:mm:ss");
                                    sqlCmd_Sync.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Sync.Parameters["@OrderID"].Value = data.OrderID;
                                    strCurrSQL = strCurrSQL.Replace("@OriginalCash", data.OriginalCash.ToString("f3")).Replace("@ChangedCash", data.ChangedCash.ToString("f3"))
                                        .Replace("@ChangedTime", data.ChangedTime.ToString("yyyy-MM-dd HH:mm:ss")).Replace("@OrderID", data.OrderID.ToString())
                                        .Replace("@Currency", ((byte)data.Curr).ToString()).Replace("@UserID", data.UserID.ToString());
                                    sqlCmd_Sync.ExecuteNonQuery();
                                    Common.DBLog(null, strCurrSQL, true);
                                }
                                catch (Exception err)
                                {
                                    Common.DBLog(err, strCurrSQL, false);
                                }
                            }

                            listDBOrdersHistory.Clear();
                            listDBTradingHistory.Clear();
                            listDBUserFund.Clear();
                            listDBUserUsableFund.Clear();
                            listDBNewUserFund.Clear();
                            listDBUserOrders.Clear();
                            listDBRemoveOrders.Clear();
                            listDBTradingError.Clear();
                            listDBUserStocks.Clear();
                            listDBFundHistory.Clear();
                            sqlTrans_Sync.Commit();
                            bInTransaction = false;
                        }
                    }
                    catch (Exception err)
                    {
                        Common.Log(err);
                        if (sqlReader_Sync != null && !sqlReader_Sync.IsClosed)
                            sqlReader_Sync.Close();
                        if (bInTransaction && sqlTrans_Sync != null && sqlTrans_Sync.Connection != null && sqlTrans_Sync.Connection.State == ConnectionState.Open)
                        {
                            sqlTrans_Sync.Rollback();
                            bInTransaction = false;
                        }
                    }
                    finally
                    {
                        if (sqlConn_Sync.State != ConnectionState.Closed)
                            sqlConn_Sync.Close();
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                Common.Log("Error: The DBSync Thread Has Crashed !");
                Common.stkTrading.Uninitialize();
            }
        }

        public static string ReplaceSqlPara(SqlCommand cmd)
        {
            String cmdText = cmd.CommandText;
            SqlParameterCollection paras = cmd.Parameters;
            if (paras != null)
            {
                foreach (SqlParameter p in paras)
                {
                    cmdText = cmdText.Replace(p.ParameterName, p.Value.ToString());
                }
            }
            return cmdText;
        }
    }
}
#endif