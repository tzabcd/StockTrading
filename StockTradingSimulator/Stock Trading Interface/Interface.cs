using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
#if INTERNEL
using QDBFAnalyzer.StructuredAnalysis;
#endif

namespace Stock_Trading_Simulator_Kernel
{
    /// <summary>
    /// 接口 RI_前缀表示RemotingInterface
    /// </summary>
    public partial class RemotingInterface : MarshalByRefObject
    {
        
#if INTERNEL
        
        /// <summary>
        /// 上次请求所有订单数的时间
        /// </summary>
        private DateTime dtLastReqAllOrders = DateTime.MinValue;
        /// <summary>
        /// 上次请求的有用户数的时间
        /// </summary>
        private DateTime dtLastReqAllUsersCount = DateTime.MinValue;

        /// <summary>
        /// 添加新用户
        /// </summary>
        /// <param name="strAdminKey"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public RI_Result RequestNewUser(string strAdminKey, int UserID)
        {
            try
            {
                if (strAdminKey == null || string.Compare(strAdminKey.Trim(),
                    Common.Config("Authorization", "AdminKey").Trim()) != 0)
                {
                    if (strAdminKey != null)
                        Common.Log("The Invoker [" + strAdminKey.Trim() + "] Is Unauthorized. [UserID=" + UserID + "]");
                    else
                        Common.Log("The Invoker [null] Is Unauthorized. [UserID=" + UserID + "]");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_NewUser)
                {
                    Common.Log("The Interface [ProcessNewUser] Is Closed. [UserID=" + UserID + "]");
                    return RI_Result.Closed_Interface;
                }

                if (Common.stkTrading == null)
                    return RI_Result.Internal_Error;
                else if (UserID <= 0)
                    return RI_Result.Illegal_UserID;
                if (Common.stkTrading.listNewUserFund == null)
                    Common.stkTrading.listNewUserFund = new List<TradingSystem.UserFund>();
                TradingSystem.UserFund userFund = new TradingSystem.UserFund();
                lock (Common.stkTrading.listNewUserFund)
                {
                    for (int i = 0; i < Common.stkTrading.listNewUserFund.Count; i++)
                    {
                        if (Common.stkTrading.listNewUserFund[i].UserID == UserID)
                            return RI_Result.Illegal_UserID;
                    }
                    #region InitAUD
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultAUD;
                    userFund.UsableCash = Common.stkTrading.defaultAUD;
                    userFund.Wealth = Common.stkTrading.defaultAUD;
                    userFund.Curr = TradingSystem.Currency.AUD;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitCAD
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultCAD;
                    userFund.UsableCash = Common.stkTrading.defaultCAD;
                    userFund.Wealth = Common.stkTrading.defaultCAD;
                    userFund.Curr = TradingSystem.Currency.CAD;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitCHF
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultCHF;
                    userFund.UsableCash = Common.stkTrading.defaultCHF;
                    userFund.Wealth = Common.stkTrading.defaultCHF;
                    userFund.Curr = TradingSystem.Currency.CHF;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitEUR
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultEUR;
                    userFund.UsableCash = Common.stkTrading.defaultEUR;
                    userFund.Wealth = Common.stkTrading.defaultEUR;
                    userFund.Curr = TradingSystem.Currency.EUR;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitGBP
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultGBP;
                    userFund.UsableCash = Common.stkTrading.defaultGBP;
                    userFund.Wealth = Common.stkTrading.defaultGBP;
                    userFund.Curr = TradingSystem.Currency.GBP;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitHKD
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultHKD;
                    userFund.UsableCash = Common.stkTrading.defaultHKD;
                    userFund.Wealth = Common.stkTrading.defaultHKD;
                    userFund.Curr = TradingSystem.Currency.HKD;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitJPY
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultJPY;
                    userFund.UsableCash = Common.stkTrading.defaultJPY;
                    userFund.Wealth = Common.stkTrading.defaultJPY;
                    userFund.Curr = TradingSystem.Currency.JPY;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitNZD
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultNZD;
                    userFund.UsableCash = Common.stkTrading.defaultNZD;
                    userFund.Wealth = Common.stkTrading.defaultNZD;
                    userFund.Curr = TradingSystem.Currency.NZD;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitRMB
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultRMB;
                    userFund.UsableCash = Common.stkTrading.defaultRMB;
                    userFund.Wealth = Common.stkTrading.defaultRMB;
                    userFund.Curr = TradingSystem.Currency.RMB;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                    #region InitUSD
                    userFund.Initialize();
                    userFund.UserID = UserID;
                    userFund.Cash = Common.stkTrading.defaultUSD;
                    userFund.UsableCash = Common.stkTrading.defaultUSD;
                    userFund.Wealth = Common.stkTrading.defaultUSD;
                    userFund.Curr = TradingSystem.Currency.USD;
                    if (userFund.Cash >= 1 && userFund.UsableCash >= 1)
                    {
                        if (Common.DBSync.AddNewUserFund(userFund))
                            Common.stkTrading.listNewUserFund.Add(userFund);
                        else
                            return RI_Result.Illegal_InitFund;
                    }
                    #endregion
                }

                Common.Debug("New User Added [UserID=" + UserID + "].");
                return RI_Result.Success;
            }
            catch
            {
                return RI_Result.Internal_Error;
            }
        }

        /// <summary>
        /// 添加即时订单
        /// </summary>
        /// <param name="strUserKey"></param>
        /// <param name="UserID"></param>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <param name="Volume"></param>
        /// <param name="Side"></param>
        /// <param name="ValidDays"></param>
        /// <param name="OrderID"></param>
        /// <returns></returns>
        public RI_Result RequestImmediateOrder(string strUserKey, int UserID, string StockCode, RI_Market Market, int Volume, bool Side, ushort ValidDays, out int OrderID)
        {
            OrderID = -1;
            try
            {
                if (strUserKey == null || string.Compare(strUserKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0)
                {
                    if (strUserKey != null)
                        Common.Log("The Invoker [" + strUserKey.Trim() + "] Is Unauthorized. [UserID=" + UserID + "]");
                    else
                        Common.Log("The Invoker [null] Is Unauthorized. [UserID=" + UserID + "]");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_ImmediateOrder || !Management.Work)
                {
                    Common.Log("The Interface [ProcessImmediateOrder] Is Closed. [UserID=" + UserID + "]");
                    return RI_Result.Closed_Interface;
                }

                if (!Common.IsInInterfaceQuotationTime)
                    return RI_Result.Out_Of_Quotation_Time;
                else if (Common.stkTrading == null)
                    return RI_Result.Internal_Error;
                else if (UserID <= 0)
                    return RI_Result.Illegal_UserID;

                TradingSystem.StockType stkType = Common.stkTrading.GetStockType(StockCode, (TradingSystem.StockMarket)((byte)Market));
                if (Volume <= 0 || (Side &&
                    (stkType != TradingSystem.StockType.SH_Warrant && stkType != TradingSystem.StockType.SZ_Warrant && Volume % 100 != 0) ||
                    ((stkType == TradingSystem.StockType.SH_Warrant || stkType == TradingSystem.StockType.SZ_Warrant) && Volume % 1000 != 0)))
                    return RI_Result.Illegal_Volume;
                else if (StockCode == null || Market == RI_Market.Unknown)
                    return RI_Result.Banned_Stock;

                StockCode = StockCode.Trim();
                if (IsBanned(stkType))
                    return RI_Result.Banned_Stock;
                TradingSystem.Currency Curr = TradingSystem.Currency.Unknown;
                switch (stkType)
                {
                    case TradingSystem.StockType.SH_B:
                        Curr = TradingSystem.Currency.USD;
                        break;
                    case TradingSystem.StockType.SZ_B:
                        Curr = TradingSystem.Currency.HKD;
                        break;
                    default:
                        Curr = TradingSystem.Currency.RMB;
                        break;
                }
                Show2003DBFRecord SHRec = new Show2003DBFRecord(); SHRec.Clear();
                SjshqDBFRecord SZRec = new SjshqDBFRecord(); SZRec.Clear();
                if (Common.stkQuotation.CheckSuspended(StockCode, (TradingSystem.StockMarket)((byte)Market)))
                {
                    if (Market == RI_Market.Shanghai && SHRec.OpenPrice < 0.001)
                        return RI_Result.Suspended_Stock;
                    else if (Market == RI_Market.Shenzhen && SZRec.OpenPrice < 0.001)
                        return RI_Result.Suspended_Stock;
                }
                else
                    return RI_Result.Banned_Stock;

                short sCurrCount = Common.stkTrading.GetUserOrdersCount(UserID);
                if (sCurrCount == -2)
                    return RI_Result.Illegal_UserID;
                if (sCurrCount < 0 || sCurrCount >= Common.MaxOrders)
                    return RI_Result.Too_Many_Orders;
                Dictionary<byte, TradingSystem.UserFund> mapUsableFund = new Dictionary<byte, TradingSystem.UserFund>();
                if (!Common.stkTrading.GetUserFund(UserID, out mapUsableFund))
                    return RI_Result.Illegal_UserID;
                int UserVolume = Common.stkTrading.GetSellableStockVolume(UserID, StockCode, (TradingSystem.StockMarket)((byte)Market));
                if (!Side && (UserVolume < Volume))
                    return RI_Result.Not_Enough_Stock;
                if (!Side && (Volume % 100 != 0) && (Volume % 100 != UserVolume % 100))
                    return RI_Result.Illegal_Volume;
                if (Side && !Common.stkTrading.CanBuy(UserID, StockCode, (TradingSystem.StockMarket)((byte)Market), Curr, 10 * Volume * (1 + Common.stkTrading.defaultBuyTax)))
                    return RI_Result.Speculation_Behavior;

                TradingSystem.UserOrders userOrder = new TradingSystem.UserOrders();
                userOrder.Initialize();
                userOrder.OrderID = Common.stkTrading.nLastOrderID++;
                userOrder.UserID = UserID;
                userOrder.StockCode = StockCode.Trim();
                userOrder.Market = (TradingSystem.StockMarket)((byte)Market);
                userOrder.OrderVolume = Volume;
                userOrder.Side = Side;
                userOrder.OrderDate = DateTime.Now;
                userOrder.ExpiredDate = userOrder.OrderDate.Date.Add(new TimeSpan(ValidDays,
                    Common.EndPMTS.Hours, Common.EndPMTS.Minutes, Common.EndPMTS.Seconds));
                userOrder.OrdStatus = TradingSystem.OrderStatus.Waiting;
                userOrder.OrdType = TradingSystem.OrderType.ImmediateOrder;
                userOrder.Curr = Curr;
                lock (Common.stkTrading.listNewUserOrders)
                {
                    Common.stkTrading.listNewUserOrders.Add(userOrder);
                    Common.DBSync.OrderAppended(userOrder, UserID);
                }
                Common.Debug("Immediate Order Requested [UserID=" + UserID + "; StockCode=" + StockCode.Trim() + "-" + (byte)Market + "].");
                OrderID = userOrder.OrderID;
                return RI_Result.Success;
            }
            catch(Exception ex)
            {
                Common.Debug("Immediate Order Requested error :" + UserID + "; StockCode=" + StockCode.Trim() + "-" + (byte)Market + "]."+ex.ToString());
                return RI_Result.Internal_Error;
            }
        }

        /// <summary>
        /// 添加限价订单
        /// </summary>
        /// <param name="strUserKey"></param>
        /// <param name="UserID"></param>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <param name="Volume"></param>
        /// <param name="OrderPrice"></param>
        /// <param name="Side"></param>
        /// <param name="ValidDays"></param>
        /// <param name="OrderID"></param>
        /// <returns></returns>
        public RI_Result RequestLimitedOrder(string strUserKey, int UserID, string StockCode, RI_Market Market, int Volume, double OrderPrice, bool Side, ushort ValidDays, out int OrderID)
        {
            OrderID = -1;
            try
            {
                if (strUserKey == null || string.Compare(strUserKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0)
                {
                    if (strUserKey != null)
                        Common.Log("The Invoker [" + strUserKey.Trim() + "] Is Unauthorized. [UserID=" + UserID + "]");
                    else
                        Common.Log("The Invoker [null] Is Unauthorized. [UserID=" + UserID + "]");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_LimitedOrder || !Management.Work)
                {
                    Common.Log("The Interface [ProcessLimitedOrder] Is Closed. [UserID=" + UserID + "]");
                    return RI_Result.Closed_Interface;
                }

                if (!Common.IsInInterfaceQuotationTime)
                    return RI_Result.Out_Of_Quotation_Time;
                else if (Common.stkTrading == null)
                    return RI_Result.Internal_Error;
                else if (UserID <= 0)
                    return RI_Result.Illegal_UserID;
                
                TradingSystem.StockType stkType = Common.stkTrading.GetStockType(StockCode, (TradingSystem.StockMarket)((byte)Market));
                if (Volume <= 0 || (Side &&
                    (stkType != TradingSystem.StockType.SH_Warrant && stkType != TradingSystem.StockType.SZ_Warrant && Volume % 100 != 0) ||
                    ((stkType == TradingSystem.StockType.SH_Warrant || stkType == TradingSystem.StockType.SZ_Warrant) && Volume % 1000 != 0)))
                    return RI_Result.Illegal_Volume;
                else if (OrderPrice < 0.001)
                    return RI_Result.Illegal_Price;
                else if ((stkType == TradingSystem.StockType.SH_A || stkType == TradingSystem.StockType.SZ_A) && OrderPrice < 0.01)
                    return RI_Result.Illegal_Price;
                else if (StockCode == null || Market == RI_Market.Unknown)
                    return RI_Result.Banned_Stock;
                StockCode = StockCode.Trim();
                TradingSystem.Currency Curr = TradingSystem.Currency.Unknown;
                switch (stkType)
                {
                    case TradingSystem.StockType.SH_B:
                        Curr = TradingSystem.Currency.USD;
                        break;
                    case TradingSystem.StockType.SZ_B:
                        Curr = TradingSystem.Currency.HKD;
                        break;
                    default:
                        Curr = TradingSystem.Currency.RMB;
                        break;
                }
                if (IsBanned(stkType))
                {
                    Common.Debug("Banned_Stock:" + UserID + ";" + StockCode + ";" + Market.ToString().Trim() + ";" + stkType.ToString().Trim());
                    return RI_Result.Banned_Stock;
                }
                Show2003DBFRecord SHRec = new Show2003DBFRecord(); SHRec.Clear();
                SjshqDBFRecord SZRec = new SjshqDBFRecord(); SZRec.Clear();
                if (Common.stkQuotation.CheckSuspended(StockCode, (TradingSystem.StockMarket)((byte)Market)))
                {
                    Common.Debug("Suspended_Stock:" + UserID + ";" + StockCode + ";" + Market.ToString());
                    return RI_Result.Suspended_Stock;
                }

                short sCurrCount = Common.stkTrading.GetUserOrdersCount(UserID);
                if (sCurrCount == -2)
                    return RI_Result.Illegal_UserID;
                if (sCurrCount < 0 || sCurrCount >= Common.MaxOrders)
                {
                    Common.Debug("Too_Many_Orders:" + UserID + ";" + sCurrCount + "/" + Common.MaxOrders);
                    return RI_Result.Too_Many_Orders;
                }
                int SellableVolume = Common.stkTrading.GetSellableStockVolume(UserID, StockCode, (TradingSystem.StockMarket)((byte)Market));
                if (!Side && (SellableVolume < Volume))
                {
                    Common.Debug("Not_Enough_Stock:" + UserID + "/" + StockCode.Trim() + "-" +
                        (byte)Market + ";" + SellableVolume + "/" + Volume);
                    return RI_Result.Not_Enough_Stock;
                }
                if (!Side && (Volume % 100 != 0) && (Volume % 100 != SellableVolume % 100))
                    return RI_Result.Illegal_Volume;
                if (Side && !Common.stkTrading.CanBuy(UserID, StockCode, (TradingSystem.StockMarket)((byte)Market), Curr, OrderPrice * Volume * (1 + Common.stkTrading.defaultBuyTax)))
                {
                    Common.Debug("Speculation_Behavior:" + UserID + "/" + StockCode.Trim() + "-"
                          + (byte)Market + "/" + (OrderPrice * Volume * (1 + Common.stkTrading.defaultBuyTax)).ToString("f2"));
                    return RI_Result.Speculation_Behavior;
                }

                TradingSystem.UserOrders userOrder = new TradingSystem.UserOrders();
                userOrder.Initialize();
                userOrder.OrderID = Common.stkTrading.nLastOrderID++;
                userOrder.UserID = UserID;
                userOrder.StockCode = StockCode.Trim();
                userOrder.Market = (TradingSystem.StockMarket)((byte)Market);
                userOrder.Side = Side;
                userOrder.OrderVolume = Volume;
                userOrder.OrderPrice = OrderPrice;
                userOrder.OrderDate = DateTime.Now;
                userOrder.ExpiredDate = userOrder.OrderDate.Date.Add(new TimeSpan(ValidDays,
                    Common.EndPMTS.Hours, Common.EndPMTS.Minutes, Common.EndPMTS.Seconds));
                userOrder.OrdStatus = TradingSystem.OrderStatus.Waiting;
                userOrder.OrdType = TradingSystem.OrderType.LimitedOrder;
                userOrder.Curr = Curr;
                if (Side)
                {
                    lock (Common.stkTrading.listNewUserFund)
                    {
                        TradingSystem.UserFund usableFund = new TradingSystem.UserFund(); usableFund.Initialize();
                        for (int i = 0; i < Common.stkTrading.listNewUserFund.Count; i++)
                        {
                            if (Common.stkTrading.listNewUserFund[i].UserID == UserID
                                && ((stkType == TradingSystem.StockType.SH_B && Common.stkTrading.listNewUserFund[i].Curr == TradingSystem.Currency.USD) ||
                                (stkType == TradingSystem.StockType.SZ_B && Common.stkTrading.listNewUserFund[i].Curr == TradingSystem.Currency.HKD) ||
                                Common.stkTrading.listNewUserFund[i].Curr == TradingSystem.Currency.RMB))
                            {
                                usableFund = Common.stkTrading.listNewUserFund[i];
                                usableFund.UsableCash -= Common.ConvertPrice((userOrder.OrderVolume * userOrder.OrderPrice) * (1 + Common.stkTrading.defaultBuyTax) + 0.0099);
                                if (usableFund.UsableCash < 0)
                                    return RI_Result.Not_Enough_Cash;
                                if (!Common.stkTrading.SetUserFund(UserID, usableFund))
                                    return RI_Result.Illegal_UserID;
                                Common.stkTrading.listNewUserFund[i] = usableFund;
                                Common.DBSync.FundUpdate(usableFund, UserID);
                                lock (Common.stkTrading.listNewUserOrders)
                                {
                                    Common.stkTrading.listNewUserOrders.Add(userOrder);
                                    Common.DBSync.OrderAppended(userOrder, UserID);
                                }
                                Common.Debug("Limited Order Requested [UserID=" + UserID + "; StockCode=" + StockCode.Trim() + "-" + (byte)Market + "].");
                                OrderID = userOrder.OrderID;
                                return RI_Result.Success;
                            }
                        }

                        Dictionary<byte, TradingSystem.UserFund> mapUsableFund = new Dictionary<byte, TradingSystem.UserFund>();
                        if (!Common.stkTrading.GetUserFund(UserID, out mapUsableFund))
                            return RI_Result.Illegal_UserID;
                        switch (stkType)
                        {
                            case TradingSystem.StockType.SH_B:
                                {
                                    if (mapUsableFund.ContainsKey((byte)TradingSystem.Currency.USD))
                                        usableFund = mapUsableFund[(byte)TradingSystem.Currency.USD];
                                    else
                                        return RI_Result.Not_Enough_Cash;
                                }
                                break;
                            case TradingSystem.StockType.SZ_B:
                                {
                                    if (mapUsableFund.ContainsKey((byte)TradingSystem.Currency.HKD))
                                        usableFund = mapUsableFund[(byte)TradingSystem.Currency.HKD];
                                    else
                                        return RI_Result.Not_Enough_Cash;
                                }
                                break;
                            default:
                                {
                                    if (mapUsableFund.ContainsKey((byte)TradingSystem.Currency.RMB))
                                        usableFund = mapUsableFund[(byte)TradingSystem.Currency.RMB];
                                    else
                                        return RI_Result.Not_Enough_Cash;
                                }
                                break;
                        }
                        usableFund.UsableCash -= Common.ConvertPrice((userOrder.OrderVolume * userOrder.OrderPrice) * (1 + Common.stkTrading.defaultBuyTax) + 0.0099);
                        if (usableFund.UsableCash < 0)
                        {
                            Common.Debug("Not_Enough_Cash:" + UserID + "/" + StockCode.Trim() + "-" + (byte)Market + ";" +
                                (usableFund.UsableCash + Common.ConvertPrice((userOrder.OrderVolume * userOrder.OrderPrice) * (1 + Common.stkTrading.defaultBuyTax)))
                                + "/" + ((userOrder.OrderVolume * userOrder.OrderPrice) * (1 + Common.stkTrading.defaultBuyTax)));
                            return RI_Result.Not_Enough_Cash;
                        }
                        if (!Common.stkTrading.SetUserFund(UserID, usableFund))
                            return RI_Result.Illegal_UserID;
                        Common.stkTrading.listNewUserFund.Add(usableFund);
                        Common.DBSync.FundUpdate(usableFund, UserID);
                    }
                }

                lock (Common.stkTrading.listNewUserOrders)
                {
                    Common.stkTrading.listNewUserOrders.Add(userOrder);
                    Common.DBSync.OrderAppended(userOrder, UserID);
                }
                Common.Debug("Limited Order Requested [UserID=" + UserID + "; StockCode=" + StockCode.Trim() + "-" + (byte)Market + "].");
                OrderID = userOrder.OrderID;
                return RI_Result.Success;
            }
            catch
            {
                return RI_Result.Internal_Error;
            }
        }

        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="strUserKey"></param>
        /// <param name="UserID"></param>
        /// <param name="OrderID"></param>
        /// <param name="ValidDays"></param>
        /// <returns></returns>
        public RI_Result RequestCancelOrder(string strUserKey, int UserID, int OrderID, ushort ValidDays)
        {
            try
            {
                if (strUserKey == null || string.Compare(strUserKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0)
                {
                    if (strUserKey != null)
                        Common.Log("The Invoker [" + strUserKey.Trim() + "] Is Unauthorized. [UserID=" + UserID + "]");
                    else
                        Common.Log("The Invoker [null] Is Unauthorized. [UserID=" + UserID + "]");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_CancelOrder || !Management.Work)
                {
                    Common.Log("The Interface [ProcessCancelOrder] Is Closed. [UserID=" + UserID + "]");
                    return RI_Result.Closed_Interface;
                }

                if (!Common.IsInInterfaceQuotationTime)
                    return RI_Result.Out_Of_Quotation_Time;
                else if (Common.stkTrading == null)
                    return RI_Result.Internal_Error;
                else if (UserID <= 0)
                    return RI_Result.Illegal_UserID;
                else if (OrderID <= 0)
                    return RI_Result.Illegal_Cancelling;
                TradingSystem.UserOrders targetOrder = new TradingSystem.UserOrders();
                targetOrder.Initialize();
                if (!Common.stkTrading.GetUserOrder(UserID, OrderID, out targetOrder))
                    return RI_Result.Illegal_Cancelling;
                switch (targetOrder.OrdType)
                {
                    case TradingSystem.OrderType.LimitedOrder:
                        break;
                    default:
                        return RI_Result.Illegal_Cancelling;
                }
                if (targetOrder.OrdStatus != TradingSystem.OrderStatus.Waiting)
                    return RI_Result.Illegal_Cancelling;
                targetOrder.OrdStatus = TradingSystem.OrderStatus.Cancelling;
                if (!Common.stkTrading.CancelUserOrder(UserID, OrderID))
                    return RI_Result.Illegal_Cancelling;

                Common.Debug("Cancel Order Requested [UserID=" + UserID + "; OrderID=" + OrderID + "].");
                return RI_Result.Success;
            }
            catch
            {
                return RI_Result.Internal_Error;
            }
        }

        /// <summary>
        /// 获取用户资金集合
        /// </summary>
        /// <param name="strUserKey"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public Dictionary<byte, RI_Fund> RequestUserFund(string strUserKey, int UserID)
        {
            try
            {
                if (strUserKey == null || string.Compare(strUserKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strUserKey.Trim() + "].");
                    return null;
                }
                else if (!Common.Switch_UserFund)
                {
                    Common.Log("The Interface [RequestUserFund] Is Closed. [UserID=" + UserID + "]");
                    return null;
                }
                else if (UserID <= 0)
                    return null;
                else if (Common.stkBuffer == null)
                {
                    Common.Log("Interface Buffer Is Null.");
                    return null; ;
                }
                return Common.stkBuffer.GetUserFund(UserID);
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户订单集合
        /// </summary>
        /// <param name="strUserKey"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public Dictionary<int, RI_Order> RequestUserOrders(string strUserKey, int UserID)
        {
            try
            {
                if (strUserKey == null || string.Compare(strUserKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strUserKey.Trim() + "].");
                    return null;
                }
                if (UserID <= 0)
                    return null;
                else if (!Common.Switch_UserOrders)
                {
                    Common.Log("The Interface [RequestUserOrders] Is Closed. [UserID=" + UserID + "]");
                    return null;
                }
                else if (Common.stkBuffer == null)
                {
                    Common.Log("Interface Buffer Is Null.");
                    return null;
                }
                return Common.stkBuffer.GetUserOrders(UserID);
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户持股集合
        /// </summary>
        /// <param name="strUserKey"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<RI_Stock> RequestUserStocks(string strUserKey, int UserID)
        {
            try
            {
                if (strUserKey == null || string.Compare(strUserKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strUserKey.Trim() + "].");
                    return null;
                }
                if (UserID <= 0)
                    return null;
                else if (!Common.Switch_UserStocks)
                {
                    Common.Log("The Interface [RequestUserStocks] Is Closed. [UserID=" + UserID + "]");
                    return null;
                }
                else if (Common.stkBuffer == null)
                {
                    Common.Log("Interface Buffer Is Null.");
                    return null;
                }
                return Common.stkBuffer.GetUserStocks(UserID);
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户交易记录
        /// </summary>
        /// <param name="strUserKey"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<RI_Trading> RequestUserTrades(string strUserKey, int UserID)
        {
            try
            {
                if (strUserKey == null || string.Compare(strUserKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strUserKey.Trim() + "].");
                    return null;
                }
                if (UserID <= 0)
                    return null;
                else if (!Common.Switch_UserTrades)
                {
                    Common.Log("The Interface [RequestUserTrades] Is Closed. [UserID=" + UserID + "]");
                    return null;
                }
                else if (Common.stkBuffer == null)
                {
                    Common.Log("Interface Buffer Is Null.");
                    return null;
                }
                return Common.stkBuffer.GetUserTradings(UserID);
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户资金流水记录集合
        /// </summary>
        /// <param name="strUserKey"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<RI_FundChanges> RequestUserFundChanges(string strUserKey, int UserID)
        {
            try
            {
                if (strUserKey == null || string.Compare(strUserKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strUserKey.Trim() + "].");
                    return null;
                }
                if (UserID <= 0)
                    return null;
                else if (!Common.Switch_UserFundChanges)
                {
                    Common.Log("The Interface [RequestUserFundChanges] Is Closed. [UserID=" + UserID + "]");
                    return null;
                }
                else if (Common.stkBuffer == null)
                {
                    Common.Log("Interface Buffer Is Null.");
                    return null;
                }
                return Common.stkBuffer.GetUserFundChanges(UserID);
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取所有订单集合
        /// </summary>
        /// <param name="strAnyKey"></param>
        /// <returns></returns>
        public List<RI_AllOrders> RequestAllOrders(string strAnyKey)
        {
            try
            {
                if (strAnyKey == null || (string.Compare(strAnyKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0
                    && string.Compare(Common.Config("Authorization", "AdminKey").Trim(),
                    strAnyKey.Trim()) != 0))
                {
                    Common.Log("UnAuthorized Remoting Key [" + strAnyKey.Trim() + "].");
                    return null;
                }
                else if (Common.stkBuffer == null)
                {
                    Common.Log("Interface Buffer Is Null.");
                    return null;
                }
                else if (DateTime.Now.Subtract(dtLastReqAllOrders) < new TimeSpan(0, 0, 30))
                {
                    Common.Log("The Invoking Frequency of RequestAllOrders() Is Too High !" + Environment.NewLine +
                         "Last Invoked: " + dtLastReqAllOrders.ToString("yyyy-MM-dd HH:mm:ss"));
                    return null;
                }
                dtLastReqAllOrders = DateTime.Now;
                return Common.stkBuffer.GetAllOrders(100);
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }
#endif
    }
}