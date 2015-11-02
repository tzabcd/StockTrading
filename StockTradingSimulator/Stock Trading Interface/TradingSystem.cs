#if INTERNEL
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using QDBFAnalyzer.StructuredAnalysis;
using Stock_Trading_Simulator_Kernel;

namespace Stock_Trading_Simulator_Kernel
{
    /// <summary>
    /// 交易系统
    /// </summary>
    public partial class TradingSystem
    {
       
        /// <summary>
        /// 新用户资金列表
        /// </summary>
        public List<UserFund> listNewUserFund = null;
        /// <summary>
        /// 新用户订单列表
        /// </summary>
        public List<UserOrders> listNewUserOrders = null;
        public int nLastOrderID = 0; // 订单分配ID

        #region 默认各币种初始资金
        public double defaultAUD = 0;
        public double defaultCAD = 0;
        public double defaultCHF = 0;
        public double defaultEUR = 0;
        public double defaultGBP = 0;
        public double defaultHKD = 0;
        public double defaultJPY = 0;
        public double defaultNZD = 0;
        public double defaultRMB = 0;
        public double defaultUSD = 0;
        #endregion

        public double defaultBuyTax = 0; //默认买入交易费
        public double defaultSellTax = 0;//默认卖出交易费
        public double defaultSingleStockRate = 1;
        public Dictionary<string, Show2003DBFRecord> mapLastSHQuotation = null; //最近一次获取的上证类股票行情
        public Dictionary<string, SjshqDBFRecord> mapLastSZQuotation = null;//最近一次获取的深证类股票行情

        private Dictionary<int, Dictionary<byte, UserFund>> mapUserFund = null; //资金
        private Dictionary<int, UserOrders> mapUserOrders = null; //订单
        private Dictionary<int, List<UserStocks>> mapUserStocks = null;//持股
        private Thread ThTrading = null;
        private bool bTrading = false;

        /// <summary>
        /// 交易系统构造
        /// </summary>
        public TradingSystem()
        {
            listNewUserFund = new List<UserFund>();
            listNewUserOrders = new List<UserOrders>();
            mapLastSHQuotation = new Dictionary<string, Show2003DBFRecord>();
            mapLastSZQuotation = new Dictionary<string, SjshqDBFRecord>();
        }

        /// <summary>
        /// 交易系统初始化
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            try
            {
                //数据同步系统构造
                Common.DBSync = new Synchronizer(Common.strConn);
                //数据同步系统启动
                if (!Common.DBSync.Initialize(ref mapUserFund, ref mapUserOrders, ref mapUserStocks, ref nLastOrderID)
                    || mapUserFund == null || mapUserOrders == null || mapUserStocks == null || nLastOrderID < 0)
                {
                    Common.Log("Failed to Create the DBSync System.");
                    return false;
                }
                else
                {
                    Common.Log("DBSync System Created");
                }
                nLastOrderID++;//订单ID初始分配
                bTrading = true;
                ThTrading = new Thread(new ThreadStart(Trading));//交易线程运行（支撑运行线程)
                ThTrading.Name = "ThTrading";
                ThTrading.Start();
                Common.Log("Trading System Created");
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                Common.Log("Failed to Create the Trading System.");
                return false;
            }
        }

        /// <summary>
        /// 交易系统停止
        /// </summary>
        public void Uninitialize()
        {
            try
            {
                bTrading = false;
                if (ThTrading != null && ThTrading.IsAlive)
                {
                    ThTrading.Join(4500);
                    if (ThTrading != null && ThTrading.IsAlive)
                        ThTrading.Abort();
                }
                if (Common.DBSync != null)
                {
                    Common.DBSync.Uninitialize(mapUserFund, mapUserOrders, mapUserStocks);
                    Common.Log("DBSync System Terminated");
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
            }
            finally
            {
                Common.Log("Trading System Terminated");
            }
        }

        /// <summary>
        /// 盘后清除操作
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            try
            {
                // 由SHELL在盘后调用
                if (!Common.IsWeekend && Common.IsInStockQuotationTime)
                    return false;

                #region 清空缓存
                lock (listNewUserFund)
                {
                    if (listNewUserFund == null)
                        listNewUserFund = new List<UserFund>();
                    else
                        listNewUserFund.Clear();
                }

                lock (listNewUserOrders)
                {
                    if (listNewUserOrders == null)
                        listNewUserOrders = new List<UserOrders>();
                    else
                        listNewUserOrders.Clear();
                }

                lock (mapUserFund)
                {
                    if (mapUserFund == null)
                        mapUserFund = new Dictionary<int, Dictionary<byte, UserFund>>();
                    else
                        mapUserFund.Clear();
                }

                lock (mapUserOrders)
                {
                    if (mapUserOrders == null)
                        mapUserOrders = new Dictionary<int, UserOrders>();
                    else
                        mapUserOrders.Clear();
                }

                lock (mapUserStocks)
                {
                    if (mapUserStocks == null)
                        mapUserStocks = new Dictionary<int, List<UserStocks>>();
                    else
                        mapUserStocks.Clear();
                }

                lock (mapLastSHQuotation)
                {
                    if (mapLastSHQuotation == null)
                        mapLastSHQuotation = new Dictionary<string, Show2003DBFRecord>();
                    else
                        mapLastSHQuotation.Clear();
                }

                lock (mapLastSZQuotation)
                {
                    if (mapLastSZQuotation == null)
                        mapLastSZQuotation = new Dictionary<string, SjshqDBFRecord>();
                    else
                        mapLastSZQuotation.Clear();
                }
                Common.Log("Trading System Buffer Cleared");
                #endregion

                //数据同步系统重启
                if (!Common.DBSync.Initialize(ref mapUserFund, ref mapUserOrders, ref mapUserStocks, ref nLastOrderID)
                    || mapUserFund == null || mapUserOrders == null || mapUserStocks == null || nLastOrderID < 0)
                {
                    Common.Log("Failed to Reinitialize the DBSync System.");
                    Common.BackupLogs();
                    return false;
                }
                else
                {
                    Common.Log("DBSync System Reinitialized.");
                    Common.BackupLogs();
                    return true;
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        /// <summary>
        /// 获取用户资金
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="mapUsableFund"></param>
        /// <returns></returns>
        public bool GetUserFund(int UserID, out Dictionary<byte, UserFund> mapUsableFund)
        {
            mapUsableFund = new Dictionary<byte, UserFund>();
            try
            {
                if (UserID <= 0 || mapUserFund == null || !mapUserFund.ContainsKey(UserID))
                    return false;
                lock (mapUserFund)
                    mapUsableFund = mapUserFund[UserID];
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 设置用户资金
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="usableFund"></param>
        /// <returns></returns>
        public bool SetUserFund(int UserID, UserFund usableFund)
        {
            try
            {
                if (UserID <= 0 || mapUserFund == null || !mapUserFund.ContainsKey(UserID))
                    return false;
                if (usableFund.UserID != UserID || usableFund.UsableCash < 0 || usableFund.Cash < 0 || usableFund.Curr == Currency.Unknown)
                    return false;
                lock (mapUserFund)
                {
                    Dictionary<byte, UserFund> mapCurrFund = mapUserFund[UserID];
                    mapCurrFund[(byte)usableFund.Curr] = usableFund;
                    mapUserFund[UserID] = mapCurrFund;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取用户订单
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="OrderID"></param>
        /// <param name="userOrder"></param>
        /// <returns></returns>
        public bool GetUserOrder(int UserID, int OrderID, out UserOrders userOrder)
        {
            userOrder = new UserOrders(); userOrder.Initialize();
            try
            {
                if (UserID <= 0 || OrderID <= 0 || mapUserOrders == null
                    || !mapUserOrders.ContainsKey(OrderID))
                    return false;
                if (!mapUserFund.ContainsKey(UserID))
                    return false;
                if (mapUserOrders[OrderID].UserID != UserID)
                    return false;
                userOrder = mapUserOrders[OrderID];
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取消用户订单
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="OrderID"></param>
        /// <returns></returns>
        public bool CancelUserOrder(int UserID, int OrderID)
        {
            try
            {
                if (UserID <= 0 || OrderID <= 0 || mapUserOrders == null)
                    return false;
                if (!mapUserFund.ContainsKey(UserID))
                    return false;
                lock (listNewUserOrders)
                {
                    for (int i = 0; i < listNewUserOrders.Count; i++)
                    {
                        if (listNewUserOrders[i].OrderID == OrderID)
                        {
                            UserOrders data = listNewUserOrders[i];
                            data.OrdStatus = OrderStatus.Cancelling;
                            listNewUserOrders[i] = data;
                            return true;
                        }
                    }
                }
                lock (mapUserOrders)
                {
                    if (!mapUserOrders.ContainsKey(OrderID))
                        return false;
                    if (mapUserOrders[OrderID].UserID != UserID)
                        return false;
                    if (mapUserOrders[OrderID].OrdStatus != OrderStatus.Waiting)
                        return false;
                    UserOrders data = mapUserOrders[OrderID];
                    data.OrdStatus = OrderStatus.Cancelling;
                    data.UpdatedDate = DateTime.Now;
                    mapUserOrders[OrderID] = data;

                    RemotingInterface.RI_Order stiOrder;
                    stiOrder = new RemotingInterface.RI_Order(); stiOrder.Clear();
                    stiOrder.ExpiredDate = data.ExpiredDate;
                    stiOrder.OrderDate = data.OrderDate;
                    stiOrder.OrderID = data.OrderID;
                    stiOrder.OrderPrice = Common.ConvertPrice(data.OrderPrice);
                    stiOrder.TradePrice = Common.ConvertPrice(data.TradePrice);
                    stiOrder.Curr = (RemotingInterface.RI_Currency)data.Curr;
                    stiOrder.OrderStatus = (RemotingInterface.RI_Status)data.OrdStatus;
                    stiOrder.OrderType = (RemotingInterface.RI_Type)data.OrdType;
                    stiOrder.OrderVolume = data.OrderVolume;
                    stiOrder.Side = data.Side;
                    stiOrder.StockCode = data.StockCode.Trim();
                    stiOrder.StockMarket = (RemotingInterface.RI_Market)data.Market;
                    stiOrder.UpdatedDate = data.UpdatedDate;
                    Common.stkBuffer.SetUserOrders(UserID, stiOrder);
                     
                    Common.Debug("Order Status Changed [" + data.OrdStatus.ToString() + "] [UserID=" + data.UserID + "] [OrderID=" + data.OrderID + "].");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取指定股票的可卖股数
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <returns></returns>
        public int GetSellableStockVolume(int UserID, string StockCode, StockMarket Market)
        {
            try
            {
                if (UserID <= 0 || StockCode == null || StockCode.Trim().Length != 6 || Market == StockMarket.Unknown)
                    return -1;
                if (!mapUserFund.ContainsKey(UserID))
                    return -1;
                int nVolume = 0;
                lock (mapUserStocks)
                {
                    if (!mapUserStocks.ContainsKey(UserID))
                        return -1;
                    foreach (UserStocks stk in mapUserStocks[UserID])
                    {
                        if (string.Compare(StockCode.Trim(),
                            stk.StockCode.Trim()) == 0
                            && Market == stk.Market
                            && stk.Sellable)
                        {
                            nVolume += stk.Volume;
                        }
                    }
                }
                lock (mapUserOrders)
                {
                    int nKey = 0;
                    foreach (object objKey in mapUserOrders.Keys)
                    {
                        if (objKey == null)
                            continue;
                        nKey = Convert.ToInt32(objKey);
                        if (mapUserOrders[nKey].UserID == UserID
                            && string.Compare(StockCode.Trim(),
                            mapUserOrders[nKey].StockCode.Trim()) == 0
                            && Market == mapUserOrders[nKey].Market
                            && (mapUserOrders[nKey].OrdStatus == OrderStatus.Waiting
                            || mapUserOrders[nKey].OrdStatus == OrderStatus.Cancelling)
                            && !mapUserOrders[nKey].Side)
                        {
                            nVolume -= mapUserOrders[nKey].OrderVolume;
                        }
                    }
                }
                return nVolume;
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// 获取用户订单数
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public short GetUserOrdersCount(int UserID)
        {
            try
            {
                if (UserID <= 0 || mapUserOrders == null)
                    return -1;
                if (!mapUserFund.ContainsKey(UserID))
                    return -2;
                short sCount = 0;
                int OrderID = 0;
                lock (mapUserOrders)
                {
                    foreach (object objKey in mapUserOrders.Keys)
                    {
                        if (objKey == null)
                            continue;
                        OrderID = Convert.ToInt32(objKey);
                        if (!mapUserOrders.ContainsKey(OrderID))
                            continue;
                        if (mapUserOrders[OrderID].UserID == UserID)
                            sCount++;
                    }
                }
                return sCount;
            }
            catch
            {
                return -3;
            }
        }
        /// <summary>
        /// 获取各种状态下的订单数
        /// </summary>
        /// <param name="ImmediateCount"></param>
        /// <param name="LimitedCount"></param>
        /// <param name="CancelCount"></param>
        /// <returns></returns>
        public bool GetTotalWaitingOrdersCount(out int ImmediateCount, out int LimitedCount, out int CancelCount)
        {
            try
            {
                int OrderID = 0;
                ImmediateCount = 0;
                LimitedCount = 0;
                CancelCount = 0;
                lock (mapUserOrders)
                {
                    if (mapUserOrders == null)
                        return false;
                    foreach (object objKey in mapUserOrders.Keys)
                    {
                        if (objKey == null)
                            continue;
                        OrderID = Convert.ToInt32(objKey);
                        if (!mapUserOrders.ContainsKey(OrderID))
                            continue;
                        if (mapUserOrders[OrderID].OrdStatus == OrderStatus.Waiting)
                        {
                            if (mapUserOrders[OrderID].OrdType == OrderType.ImmediateOrder)
                                ImmediateCount++;
                            else if (mapUserOrders[OrderID].OrdType == OrderType.LimitedOrder)
                                LimitedCount++;
                        }
                        else if (mapUserOrders[OrderID].OrdStatus == OrderStatus.Cancelling)
                            CancelCount++;
                    }
                }
                return true;
            }
            catch
            {
                ImmediateCount = 0;
                LimitedCount = 0;
                CancelCount = 0;
                return false;
            }
        }
        /// <summary>
        /// 更新用户股票
        /// </summary>
        /// <param name="mapUpdate"></param>
        /// <returns></returns>
        public bool UpdateUserStocks(Dictionary<int, List<UserStocks>> mapUpdate)
        {
            try
            {
                if (mapUpdate == null || mapUpdate.Count <= 0)
                    return false;
                lock (mapUserStocks)
                {
                    mapUserStocks.Clear();
                    mapUserStocks = new Dictionary<int, List<UserStocks>>(mapUpdate);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断是否能购买
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <param name="Curr"></param>
        /// <param name="WannaBuy"></param>
        /// <returns></returns>
        public bool CanBuy(int UserID, string StockCode, StockMarket Market, Currency Curr, double WannaBuy)
        {
            try
            {
                if (UserID <= 0 || StockCode == null || StockCode.Trim().Length != 6 || Market == StockMarket.Unknown)
                    return false;
                if (!mapUserFund.ContainsKey(UserID))
                    return false;
                if (!mapUserFund[UserID].ContainsKey((byte)Curr))
                    return false;
                double dValue = 0;
                lock (mapUserOrders)
                {
                    foreach (object objKey in mapUserOrders.Keys)
                    {
                        if (objKey == null)
                            continue;
                        int nOrderID = Convert.ToInt32(objKey);
                        if (!mapUserOrders.ContainsKey(nOrderID))
                            continue;
                        if (mapUserOrders[nOrderID].UserID == UserID
                            && string.Compare(StockCode.Trim(),
                            mapUserOrders[nOrderID].StockCode.Trim()) == 0
                            && Market == mapUserOrders[nOrderID].Market && mapUserOrders[nOrderID].Side
                            && (mapUserOrders[nOrderID].OrdStatus == OrderStatus.Cancelling
                            || mapUserOrders[nOrderID].OrdStatus == OrderStatus.Waiting)
                            && Curr == mapUserOrders[nOrderID].Curr)
                            dValue += (mapUserOrders[nOrderID].OrderPrice * mapUserOrders[nOrderID].OrderVolume * (1 + defaultBuyTax));
                    }
                }

                Show2003DBFRecord SHRate = new Show2003DBFRecord(); SHRate.Clear();
                SjshqDBFRecord SZRate = new SjshqDBFRecord(); SZRate.Clear();
                double PreClose = 0;
                if (Common.stkQuotation.GetStkRate(StockCode.Trim(), Market, out SHRate, out SZRate))
                {
                    switch (Market)
                    {
                        case StockMarket.Shanghai:
                            PreClose = SHRate.PreClosePrice;
                            break;
                        case StockMarket.Shenzhen:
                            PreClose = SZRate.PreClosePrice;
                            break;
                    }
                }

                lock (mapUserStocks)
                {
                    if (mapUserStocks.ContainsKey(UserID))
                    {
                        foreach (UserStocks data in mapUserStocks[UserID])
                        {
                            if (data.UserID == UserID
                                && string.Compare(StockCode.Trim(),
                                data.StockCode.Trim()) == 0
                                && Market == data.Market
                                && Curr == data.Curr)
                            {
                                if (PreClose >= 0.001)
                                    dValue += (PreClose * data.Volume);
                                else
                                    dValue += (data.AveragePrice * data.Volume);
                            }
                        }
                    }
                }
                double dMax = mapUserFund[UserID][(byte)Curr].Wealth;
                if (defaultSingleStockRate > 0)
                    dMax *= defaultSingleStockRate;
                if (dValue + WannaBuy > dMax)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取股票类型
        /// </summary>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <returns></returns>
        public StockType GetStockType(string StockCode, StockMarket Market)
        {
            try
            {
                // 长度是否有效
                if (StockCode == null || StockCode.Trim().Length != 6)
                    return StockType.Unknown;
                StockCode = StockCode.Trim();
                if (StockCode == "000000")
                    return StockType.Unknown;
                if (Market == StockMarket.Shanghai && (StockCode == "510180" ||
                    StockCode == "510050" || StockCode == "510880"))
                    return StockType.ETF;
                if (Market == StockMarket.Shenzhen && (
                    StockCode == "159901" || StockCode == "159902"))
                    return StockType.ETF;

                // 区别代码类型
                switch (Market)
                {
                    case StockMarket.Shenzhen:
                        if (StockCode.StartsWith("00"))
                            return StockType.SZ_A;
                        else if (StockCode.StartsWith("184"))
                            return StockType.SZ_ClosedFund;
                        else if (StockCode.StartsWith("17") || StockCode.StartsWith("18") ||
                           StockCode.StartsWith("1599"))
                            return StockType.SZ_OpenedFund;
                        else if (StockCode.StartsWith("20"))
                            return StockType.SZ_B;
                        else if (StockCode.StartsWith("39"))
                            return StockType.SZ_Index;
                        else if (StockCode.StartsWith("031"))
                            return StockType.SZ_Warrant;
                        else
                            return StockType.SZ_Bond;

                    case StockMarket.Shanghai:
                        if (StockCode.StartsWith("500"))
                            return StockType.SH_ClosedFund;
                        else if (StockCode.StartsWith("510")
                             || StockCode.StartsWith("519"))
                            return StockType.SH_OpenedFund;
                        else if (StockCode.StartsWith("6"))
                            return StockType.SH_A;
                        else if (StockCode.StartsWith("900"))
                            return StockType.SH_B;
                        else if (StockCode.StartsWith("000"))
                            return StockType.SH_Index;
                        else if (StockCode.StartsWith("580"))
                            return StockType.SH_Warrant;
                        else
                            return StockType.SH_Bond;

                    default:	// 非法代码
                        return StockType.Unknown;
                }
            }
            catch
            {
                return StockType.Unknown;
            }
        }

        /// <summary>
        /// 交易运转中心
        /// </summary>
        private void Trading()
        {
            try
            {
                ushort uFlag = 0;
                List<int> listUserOrdersKeys = new List<int>();
                //资金，订单，持股皆不为空时，系统正常启动
                while (bTrading && mapUserFund != null && mapUserOrders != null && mapUserStocks != null)
                {
                    try
                    {
                        if (!Management.Work || Common.IsWeekend)
                        {
                            Thread.Sleep(30000);
                            continue;
                        }
                        else
                        {
                            //处于接口开放时段
                            if (Common.IsInInterfaceQuotationTime)
                            {
                                //统计缓存中的即时定单，限价定单，撤销定单的委托数
                                int ICount = 0, LCount = 0, CCount = 0;
                                if (++uFlag % 500 == 0 && GetTotalWaitingOrdersCount(out ICount, out LCount, out CCount))
                                {
                                    Common.Log("--- Orders in Buffer (Total: " + (ICount + LCount + CCount) + ") ---" + Environment.NewLine +
                                        "[ImmediateOrder=" + ICount + "] [LimitedOrder=" + LCount + "] [CancelOrder=" + CCount + "]");
                                    uFlag = 0;
                                }
                            }
                            //不在交易时段，中午时间直接忽视
                            if (DateTime.Now.TimeOfDay < Common.BeginAMTS
                                || DateTime.Now.TimeOfDay > Common.EndPMTS.Add(new TimeSpan(0, 5, 0)))
                            {
                                Thread.Sleep(30000);
                                continue;
                            }
                        }
                        //处理订单信息
                        if (Common.stkQuotation != null && mapUserOrders.Count > 0 && Common.stkQuotation.GetSnapShot())
                        {
                            int nOrderID = -1;
                            listUserOrdersKeys.Clear();
                            Show2003DBFRecord SHRate = new Show2003DBFRecord(); SHRate.Clear();
                            SjshqDBFRecord SZRate = new SjshqDBFRecord(); SZRate.Clear();
                            foreach (object objKey in mapUserOrders.Keys)
                            {
                                if (objKey == null)
                                    continue;
                                nOrderID = Convert.ToInt32(objKey);
                                if (nOrderID < 0 || !mapUserOrders.ContainsKey(nOrderID))
                                    continue;
                                listUserOrdersKeys.Add(nOrderID);//加入订单ID到订单处理队列
                            }
                            //对加入到处理订单队列的订单进行处理
                            foreach (int OrderKey in listUserOrdersKeys)
                            {
                                try
                                {
                                    if (!mapUserOrders.ContainsKey(OrderKey))
                                        continue;
                                    UserOrders userOrder = mapUserOrders[OrderKey];

                                    if (!mapUserFund.ContainsKey(userOrder.UserID))
                                        continue;
                                    Dictionary<byte, UserFund> mapCurrFund = mapUserFund[userOrder.UserID];

                                    if (!mapCurrFund.ContainsKey((byte)userOrder.Curr))
                                        continue;
                                    UserFund userFund = mapCurrFund[(byte)userOrder.Curr];

                                    if (userOrder.StockCode == null
                                        || userOrder.StockCode.Trim().Length <= 0
                                        || userOrder.Market == StockMarket.Unknown
                                        || userOrder.OrdStatus == OrderStatus.Unknown)
                                    {
                                        mapUserOrders.Remove(OrderKey);
                                    }
                                    else if (userOrder.OrdStatus != OrderStatus.Waiting
                                        && userOrder.OrdStatus != OrderStatus.Cancelling)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (userOrder.ExpiredDate < DateTime.Now
                                            && (userOrder.OrdStatus == OrderStatus.Waiting
                                            || userOrder.OrdStatus == OrderStatus.Cancelling))
                                        {
                                            #region 该订单已过期
                                            userOrder.OrdStatus = OrderStatus.Failure;
                                            userOrder.UpdatedDate = DateTime.Now;
                                            Common.DBSync.RecordError(userOrder, "过期订单：" + userOrder.ExpiredDate.ToString("yyyy-MM-dd HH:mm:ss").Trim());
                                            if (userOrder.OrdType == TradingSystem.OrderType.LimitedOrder)
                                            {
                                                userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + Common.stkTrading.defaultBuyTax));
                                                if (!Common.DBSync.FundUpdate(userFund, userOrder.UserID))
                                                    continue;
                                                SetUserFund(userFund.UserID, userFund);
                                            }
                                            if (Common.DBSync.OrderChanged(mapCurrFund[(byte)userFund.Curr], userOrder, new UserStocks()))
                                            {
                                                lock (mapUserFund)
                                                    mapUserFund[userOrder.UserID] = mapCurrFund;
                                                lock (mapUserOrders)
                                                    mapUserOrders[OrderKey] = userOrder;
                                                Common.Debug("Order Status Changed [" + userOrder.OrdStatus.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                            }
                                            #endregion
                                        }
                                        else if (userOrder.OrdStatus == OrderStatus.Cancelling)
                                        {
                                            #region 取消交易
                                            if (userOrder.OrdType == OrderType.LimitedOrder)
                                            {
                                                userOrder.OrdStatus = OrderStatus.Cancelled;
                                                userOrder.UpdatedDate = DateTime.Now;
                                                if (Common.DBSync.OrderChanged(mapCurrFund[(byte)userFund.Curr], userOrder, new UserStocks()))
                                                {
                                                    mapUserOrders[userOrder.OrderID] = userOrder;
                                                    if (userOrder.Side)
                                                    {
                                                        UserFund usableFund = mapCurrFund[(byte)userFund.Curr];
                                                        usableFund.UsableCash += Common.ConvertPrice((userOrder.OrderVolume * userOrder.OrderPrice) * (1 + defaultBuyTax));
                                                        SetUserFund(usableFund.UserID, usableFund);
                                                        Common.DBSync.FundUpdate(usableFund, userOrder.UserID);
                                                    }
                                                    Common.Debug("Order Status Changed [" + userOrder.OrdStatus.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                                }
                                                else
                                                {
                                                    userOrder.OrdStatus = OrderStatus.Failure;
                                                    userOrder.UpdatedDate = DateTime.Now;
                                                    lock (mapUserOrders)
                                                        mapUserOrders[userOrder.OrderID] = userOrder;
                                                    Common.DBSync.RecordError(userOrder, "撤单失败：用户ID-" + userOrder.UserID + "；订单ID-" + userOrder.OrderID);
                                                }
                                            }
                                            else
                                            {
                                                userOrder.OrdStatus = OrderStatus.Failure;
                                                userOrder.UpdatedDate = DateTime.Now;
                                                lock (mapUserOrders)
                                                    mapUserOrders[userOrder.OrderID] = userOrder;
                                                Common.DBSync.RecordError(userOrder, "无效撤单：用户ID-" + userOrder.UserID + "；订单ID-" + userOrder.OrderID);
                                            }
                                            #endregion
                                        }
                                        else if (userOrder.OrdStatus == OrderStatus.Waiting &&
                                          !Common.stkQuotation.GetStkRate(userOrder.StockCode, userOrder.Market, out SHRate, out SZRate))
                                        {
                                            #region 无法获得行情
                                            userOrder.OrdStatus = OrderStatus.Failure;
                                            userOrder.UpdatedDate = DateTime.Now;
                                            Common.DBSync.RecordError(userOrder, "无法获得行情：" + userOrder.StockCode.Trim() + "-" + userOrder.Market.ToString().Trim());
                                            if (userOrder.OrdType == TradingSystem.OrderType.LimitedOrder)
                                            {
                                                userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + Common.stkTrading.defaultBuyTax));
                                                SetUserFund(userFund.UserID, userFund);
                                                if (!Common.DBSync.FundUpdate(userFund, userOrder.UserID))
                                                    continue;
                                            }
                                            if (Common.DBSync.OrderChanged(mapCurrFund[(byte)userFund.Curr], userOrder, new UserStocks()))
                                            {
                                                lock (mapUserFund)
                                                    mapUserFund[userOrder.UserID] = mapCurrFund;
                                                lock (mapUserOrders)
                                                    mapUserOrders[OrderKey] = userOrder;
                                                Common.Debug("Order Status Changed [" + userOrder.OrdStatus.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                            }
                                            #endregion
                                        }
                                        else if (userOrder.OrdStatus == OrderStatus.Waiting &&
                                            (userOrder.Market == StockMarket.Shanghai && (SHRate.LatestPrice < 0.001 || SHRate.OpenPrice < 0.001))
                                            || (userOrder.Market == StockMarket.Shenzhen && (SZRate.LatestPrice < 0.001 || SZRate.OpenPrice < 0.001)))
                                        {
                                            #region 该股票停牌
                                            userOrder.OrdStatus = OrderStatus.Failure;
                                            userOrder.UpdatedDate = DateTime.Now;
                                            Common.DBSync.RecordError(userOrder, "停牌股票：" + userOrder.StockCode.Trim() + "-" + userOrder.Market.ToString().Trim());
                                            if (userOrder.OrdType == TradingSystem.OrderType.LimitedOrder)
                                            {
                                                userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + Common.stkTrading.defaultBuyTax));
                                                SetUserFund(userFund.UserID, userFund);
                                                if (!Common.DBSync.FundUpdate(userFund, userOrder.UserID))
                                                    continue;
                                            }
                                            if (Common.DBSync.OrderChanged(mapCurrFund[(byte)userFund.Curr], userOrder, new UserStocks()))
                                            {
                                                lock (mapUserFund)
                                                    mapUserFund[userOrder.UserID] = mapCurrFund;
                                                lock (mapUserOrders)
                                                    mapUserOrders[OrderKey] = userOrder;
                                                Common.Debug("Order Status Changed [" + userOrder.OrdStatus.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            if (userOrder.OrdStatus == OrderStatus.Waiting)
                                            {
                                                double dBargainAmount = 0;
                                                List<UserStocks> listUserStocks = new List<UserStocks>();
                                                if (mapUserStocks.ContainsKey(userOrder.UserID))
                                                    listUserStocks = mapUserStocks[userOrder.UserID];
                                                else
                                                    mapUserStocks[userOrder.UserID] = new List<UserStocks>();
                                                UserStocks userStock = new UserStocks(); userStock.Initialize();
                                                bool bExist = false;
                                                for (int i = 0; i < listUserStocks.Count; i++)
                                                {
                                                    if (listUserStocks[i].UserID == userOrder.UserID
                                                        && string.Compare(userOrder.StockCode.Trim()
                                                        , listUserStocks[i].StockCode.Trim()) == 0
                                                        && userOrder.Market == listUserStocks[i].Market
                                                        && ((!userOrder.Side && listUserStocks[i].Sellable)
                                                        || (userOrder.Side && !listUserStocks[i].Sellable)))
                                                    {
                                                        bExist = true;
                                                        userStock = listUserStocks[i];
                                                        break;
                                                    }
                                                }
                                                if (!bExist)
                                                {
                                                    userStock.UserID = userOrder.UserID;
                                                    userStock.StockCode = userOrder.StockCode.Trim();
                                                    userStock.Market = userOrder.Market;
                                                    userStock.Sellable = false;
                                                }

                                                switch (userOrder.OrdType)
                                                {
                                                    #region  市价交易
                                                    case OrderType.ImmediateOrder:
                                                        {
                                                            Common.Debug("--- [ImmediateOrder] Received. ---");
                                                            if (userOrder.Side)
                                                            {
                                                                // 买入
                                                                if (userOrder.Market == StockMarket.Shanghai)
                                                                {
                                                                    if (SHRate.SellingVal1 < 0.001 || Common.ComparePrice(SHRate.HighestPrice, SHRate.LowestPrice) <= 0)
                                                                    {
                                                                        userOrder.OrderPrice = SHRate.SellingVal1;
                                                                        if (Common.ComparePrice(userFund.UsableCash, (SHRate.SellingVal1 * userOrder.OrderVolume) * (1 + defaultBuyTax)) < 0)
                                                                        {
                                                                            userOrder.OrdStatus = OrderStatus.Failure;
                                                                            userOrder.UpdatedDate = DateTime.Now;
                                                                        }
                                                                        else
                                                                        {
                                                                            userOrder.OrdType = OrderType.LimitedOrder;
                                                                            Common.DBSync.OrderChanged(userFund, userOrder, new UserStocks());
                                                                            userFund.UsableCash -= Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + defaultBuyTax) + 0.0099);
                                                                            Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                            mapCurrFund[(byte)userFund.Curr] = userFund;
                                                                            mapUserFund[userOrder.UserID] = mapCurrFund;
                                                                            mapUserOrders[OrderKey] = userOrder;
                                                                            Common.Debug("Order Type Changed [" + userOrder.OrdType.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        userOrder.OrderPrice = SHRate.SellingVal1;
                                                                        if (Common.ComparePrice(userFund.UsableCash, (SHRate.SellingVal1 * userOrder.OrderVolume) * (1 + defaultBuyTax)) < 0)
                                                                        {
                                                                            userOrder.OrdStatus = OrderStatus.Failure;
                                                                            userOrder.UpdatedDate = DateTime.Now;
                                                                        }
                                                                        else if (BuySH(ref SHRate, ref userFund, ref userOrder, ref userStock, out dBargainAmount))
                                                                        {
                                                                            UpdateUserStock(ref listUserStocks, userStock);
                                                                            userFund.UsableCash -= Common.ConvertPrice((SHRate.SellingVal1 * userOrder.OrderVolume) * (1 + defaultBuyTax) + 0.0099);
                                                                            Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                        }
                                                                    }
                                                                }
                                                                else if (userOrder.Market == StockMarket.Shenzhen)
                                                                {
                                                                    if (SZRate.SellingVal1 < 0.001 || Common.ComparePrice(SZRate.HighestPrice, SZRate.LowestPrice) <= 0)
                                                                    {
                                                                        userOrder.OrderPrice = SZRate.SellingVal1;
                                                                        if (Common.ComparePrice(userFund.UsableCash, (SZRate.SellingVal1 * userOrder.OrderVolume) * (1 + defaultBuyTax)) < 0)
                                                                        {
                                                                            userOrder.OrdStatus = OrderStatus.Failure;
                                                                            userOrder.UpdatedDate = DateTime.Now;
                                                                        }
                                                                        else
                                                                        {
                                                                            userOrder.OrdType = OrderType.LimitedOrder;
                                                                            Common.DBSync.OrderChanged(userFund, userOrder, new UserStocks());
                                                                            userFund.UsableCash -= Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + defaultBuyTax) + 0.0099);
                                                                            Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                            mapCurrFund[(byte)userFund.Curr] = userFund;
                                                                            mapUserFund[userOrder.UserID] = mapCurrFund;
                                                                            mapUserOrders[OrderKey] = userOrder;
                                                                            Common.Debug("Order Type Changed [" + userOrder.OrdType.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        userOrder.OrderPrice = SHRate.SellingVal1;
                                                                        if (Common.ComparePrice(userFund.UsableCash, (SHRate.SellingVal1 * userOrder.OrderVolume) * (1 + defaultBuyTax)) < 0)
                                                                        {
                                                                            userOrder.OrdStatus = OrderStatus.Failure;
                                                                            userOrder.UpdatedDate = DateTime.Now;
                                                                        }
                                                                        else if (BuySZ(ref SZRate, ref userFund, ref userOrder, ref userStock, out dBargainAmount))
                                                                        {
                                                                            UpdateUserStock(ref listUserStocks, userStock);
                                                                            userFund.UsableCash -= Common.ConvertPrice((SZRate.SellingVal1 * userOrder.OrderVolume) * (1 + defaultBuyTax) + 0.0099);
                                                                            Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // 卖出
                                                                if (userOrder.Market == StockMarket.Shanghai)
                                                                {
                                                                    if (SHRate.BuyingVal1 < 0.001 || Common.ComparePrice(SHRate.HighestPrice, SHRate.LowestPrice) <= 0)
                                                                    {
                                                                        userOrder.OrderPrice = SHRate.BuyingVal1;
                                                                        userOrder.OrdType = OrderType.LimitedOrder;
                                                                        Common.DBSync.OrderChanged(userFund, userOrder, new UserStocks());
                                                                        mapUserOrders[OrderKey] = userOrder;
                                                                        Common.Debug("Order Type Changed [" + userOrder.OrdType.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                                                    }
                                                                    else if (SellSH(ref SHRate, ref userFund, ref userOrder, ref userStock, out dBargainAmount))
                                                                    {
                                                                        userOrder.OrderPrice = SHRate.BuyingVal1;
                                                                        UpdateUserStock(ref listUserStocks, userStock);
                                                                        Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                    }
                                                                }
                                                                else if (userOrder.Market == StockMarket.Shenzhen)
                                                                {
                                                                    if (SZRate.BuyingVal1 < 0.001 || Common.ComparePrice(SZRate.HighestPrice, SZRate.LowestPrice) <= 0)
                                                                    {
                                                                        userOrder.OrderPrice = SZRate.BuyingVal1;
                                                                        userOrder.OrdType = OrderType.LimitedOrder;
                                                                        Common.DBSync.OrderChanged(userFund, userOrder, new UserStocks());
                                                                        mapUserOrders[OrderKey] = userOrder;
                                                                        Common.Debug("Order Type Changed [" + userOrder.OrdType.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                                                    }
                                                                    else if (SellSZ(ref SZRate, ref userFund, ref userOrder, ref userStock, out dBargainAmount))
                                                                    {
                                                                        userOrder.OrderPrice = SZRate.BuyingVal1;
                                                                        UpdateUserStock(ref listUserStocks, userStock);
                                                                        Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    #endregion
                                                    #region 限价交易
                                                    case OrderType.LimitedOrder:
                                                        {
                                                            if (userOrder.Side)
                                                            {
                                                                // 买入
                                                                if (userOrder.Market == StockMarket.Shanghai && SHRate.SellingVal1 >= 0.001
                                                                    && Common.ComparePrice(SHRate.HighestPrice, SHRate.LowestPrice) > 0
                                                                    && Common.ComparePrice(SHRate.SellingVal1, userOrder.OrderPrice) <= 0)
                                                                {
                                                                    if (IsOrderValid(ref SHRate, ref userFund, ref userOrder) &&
                                                                        BuySH(ref SHRate, ref userFund, ref userOrder, ref userStock, out dBargainAmount))
                                                                        UpdateUserStock(ref listUserStocks, userStock);
                                                                    Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                }
                                                                else if (userOrder.Market == StockMarket.Shenzhen && SZRate.SellingVal1 >= 0.001
                                                                    && Common.ComparePrice(SZRate.HighestPrice, SZRate.LowestPrice) > 0
                                                                    && Common.ComparePrice(SZRate.SellingVal1, userOrder.OrderPrice) <= 0)
                                                                {
                                                                    if (IsOrderValid(ref SZRate, ref userFund, ref userOrder) &&
                                                                        BuySZ(ref SZRate, ref userFund, ref userOrder, ref userStock, out dBargainAmount))
                                                                        UpdateUserStock(ref listUserStocks, userStock);
                                                                    Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // 卖出
                                                                if (userOrder.Market == StockMarket.Shanghai && SHRate.BuyingVal1 >= 0.001
                                                                    && Common.ComparePrice(SHRate.HighestPrice, SHRate.LowestPrice) > 0
                                                                    && Common.ComparePrice(SHRate.BuyingVal1, userOrder.OrderPrice) >= 0)
                                                                {
                                                                    if (IsOrderValid(ref SHRate, ref userFund, ref userOrder) &&
                                                                        SellSH(ref SHRate, ref userFund, ref userOrder, ref userStock, out dBargainAmount))
                                                                        UpdateUserStock(ref listUserStocks, userStock);
                                                                    Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                }
                                                                else if (userOrder.Market == StockMarket.Shenzhen
                                                                    && SZRate.BuyingVal1 >= 0.001 && Common.ComparePrice(SZRate.HighestPrice, SZRate.LowestPrice) > 0
                                                                    && Common.ComparePrice(SZRate.BuyingVal1, userOrder.OrderPrice) >= 0)
                                                                {
                                                                    if (IsOrderValid(ref SZRate, ref userFund, ref userOrder) &&
                                                                        SellSZ(ref SZRate, ref userFund, ref userOrder, ref userStock, out dBargainAmount))
                                                                        UpdateUserStock(ref listUserStocks, userStock);
                                                                    Common.DBSync.FundUpdate(userFund, userOrder.UserID);
                                                                }
                                                            }
                                                        }
                                                        break;
                                                    #endregion
                                                    default:
                                                        {
                                                            userOrder.OrdStatus = OrderStatus.Failure;
                                                            userOrder.UpdatedDate = DateTime.Now;
                                                            Common.DBSync.RecordError(userOrder, "未知的订单类型：" + userOrder.OrdType.ToString().Trim());
                                                            lock (mapUserOrders)
                                                                mapUserOrders[OrderKey] = userOrder;
                                                        }
                                                        break;
                                                }

                                                if (userOrder.OrdStatus != OrderStatus.Waiting
                                                    && Common.DBSync.OrderChanged(userFund, userOrder, userStock))
                                                {
                                                    SetUserFund(userFund.UserID, userFund);
                                                    lock (mapUserOrders)
                                                        mapUserOrders[OrderKey] = userOrder;
                                                    lock (mapUserStocks)
                                                        mapUserStocks[userOrder.UserID] = listUserStocks;
                                                    Common.Debug("Order Status Changed [" + userOrder.OrdStatus.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                                }
                                            }
                                            else
                                                Common.Log("Illegal Order Status [" + userOrder.OrdStatus.ToString() + "] [UserID=" + userOrder.UserID + "] [OrderID=" + userOrder.OrderID + "].");
                                        }
                                    }
                                }
                                catch (Exception err)
                                {
                                    Common.Log(err);
                                }
                            }
                        }

                        lock (listNewUserFund)
                        {
                            if (listNewUserFund.Count > 0)
                            {
                                for (int i = 0; i < listNewUserFund.Count; i++)
                                {
                                    if (!mapUserFund.ContainsKey(listNewUserFund[i].UserID))
                                    {
                                        mapUserFund[listNewUserFund[i].UserID] = new Dictionary<byte, UserFund>();
                                        Dictionary<byte, UserFund> mapCurrFund = mapUserFund[listNewUserFund[i].UserID];
                                        mapCurrFund[(byte)listNewUserFund[i].Curr] = listNewUserFund[i];
                                        mapUserFund[listNewUserFund[i].UserID] = mapCurrFund;
                                    }
                                    else if (!mapUserFund[listNewUserFund[i].UserID].ContainsKey((byte)listNewUserFund[i].Curr))
                                    {
                                        Dictionary<byte, UserFund> mapCurrFund = mapUserFund[listNewUserFund[i].UserID];
                                        mapCurrFund[(byte)listNewUserFund[i].Curr] = listNewUserFund[i];
                                        mapUserFund[listNewUserFund[i].UserID] = mapCurrFund;
                                    }
                                }
                                listNewUserFund.Clear();
                            }
                        }

                        lock (listNewUserOrders)
                        {
                            if (listNewUserOrders.Count > 0)
                            {
                                for (int i = 0; i < listNewUserOrders.Count; i++)
                                {
                                    if (!mapUserOrders.ContainsKey(listNewUserOrders[i].OrderID))
                                        mapUserOrders[listNewUserOrders[i].OrderID] = listNewUserOrders[i];
                                }
                                listNewUserOrders.Clear();
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        Common.Log(err);
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                if (Common.DBSync != null)
                    Common.DBSync.Uninitialize(mapUserFund, mapUserOrders, mapUserStocks);
                Common.Log("Error: The Trading Thread Has Crashed !");
            }
        }

        /// <summary>
        /// 更新用户股票
        /// </summary>
        /// <param name="listUserStocks"></param>
        /// <param name="userStock"></param>
        /// <returns></returns>
        private bool UpdateUserStock(ref List<UserStocks> listUserStocks, UserStocks userStock)
        {
            try
            {
                if (listUserStocks == null)
                    return false;
                bool bExist = false;
                for (int i = 0; i < listUserStocks.Count; i++)
                {
                    if (listUserStocks[i].UserID == userStock.UserID
                        && string.Compare(userStock.StockCode.Trim()
                        , listUserStocks[i].StockCode.Trim()) == 0
                        && userStock.Market == listUserStocks[i].Market
                        && userStock.Sellable == listUserStocks[i].Sellable)
                    {
                        bExist = true;
                        if (userStock.Volume > 0)
                            listUserStocks[i] = userStock;
                        else
                            listUserStocks.RemoveAt(i--);
                        break;
                    }
                }
                if (!bExist && userStock.Volume > 0)
                    listUserStocks.Add(userStock);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 购买上证类股票
        /// </summary>
        /// <param name="SHRate"></param>
        /// <param name="userFund"></param>
        /// <param name="userOrder"></param>
        /// <param name="userStock"></param>
        /// <param name="dBoughtAmount"></param>
        /// <returns></returns>
        private bool BuySH(ref Show2003DBFRecord SHRate, ref UserFund userFund, ref UserOrders userOrder, ref UserStocks userStock, out double dBoughtAmount)
        {
            try
            {
                dBoughtAmount = 0;
                if (userOrder.UserID != userFund.UserID || userStock.UserID != userFund.UserID)
                    return false;
                else if (!userOrder.Side || userOrder.OrdStatus != OrderStatus.Waiting || userFund.UserID != userOrder.UserID)
                    return false;
                else if (userOrder.OrderVolume < 0)
                    return false;
                else if (SHRate.SellingVal1 < 0.001 || SHRate.LatestPrice < 0.001)
                    return false;
                UserFund tmpFund = userFund;
                UserOrders tmpOrder = userOrder;
                UserStocks tmpStock = userStock;
                dBoughtAmount = Common.ConvertPrice((tmpOrder.OrderVolume * SHRate.SellingVal1) * (1 + defaultBuyTax));
                if (Common.ComparePrice(userFund.Cash, dBoughtAmount) >= 0)
                {
                    tmpFund.UsableCash += Common.ConvertPrice((tmpOrder.OrderVolume * tmpOrder.OrderPrice) * (1 + defaultBuyTax));
                    tmpFund.UsableCash -= Common.ConvertPrice(dBoughtAmount);

                    Synchronizer.FundHistory fundHistory = new Synchronizer.FundHistory(); fundHistory.Initialize();
                    fundHistory.UserID = userFund.UserID; fundHistory.OrderID = userOrder.OrderID;
                    fundHistory.OriginalCash = Common.ConvertPrice(userFund.Cash);
                    tmpFund.Cash -= Common.ConvertPrice(dBoughtAmount);
                    fundHistory.ChangedCash = Common.ConvertPrice(tmpFund.Cash - fundHistory.OriginalCash);
                    fundHistory.Curr = tmpOrder.Curr;
                    Common.DBSync.FundChanged(fundHistory, userFund.UserID);

                    if (tmpStock.Volume + tmpOrder.OrderVolume > 0)
                        tmpStock.AveragePrice = Common.ConvertPrice(
                            ((tmpStock.AveragePrice * tmpStock.Volume) + (SHRate.SellingVal1 * tmpOrder.OrderVolume))
                            / (tmpStock.Volume + tmpOrder.OrderVolume) * (1 + defaultBuyTax));
                    else
                        tmpStock.AveragePrice = 0;
                    tmpStock.Volume += tmpOrder.OrderVolume;
                    tmpStock.Curr = tmpOrder.Curr;

                    if (GetStockType(tmpOrder.StockCode, StockMarket.Shanghai) == StockType.SH_Warrant)
                        tmpStock.Sellable = true;
                    else
                        tmpStock.Sellable = false;
                    if (tmpFund.UsableCash < 0)
                        tmpFund.UsableCash = 0;
                }
                else
                {
                    tmpOrder.UpdatedDate = DateTime.Now;
                    tmpOrder.OrdStatus = OrderStatus.Failure;
                    Common.DBSync.RecordError(tmpOrder, "(SH)余额不足：Currency-" + userFund.Curr.ToString().Trim() + "/Cash-" +
                        tmpFund.Cash.ToString("f3").Trim() + "/Cost-" + dBoughtAmount.ToString("f3").Trim());
                }

                if (tmpOrder.OrdStatus != OrderStatus.Failure)
                {
                    tmpOrder.TradePrice = SHRate.SellingVal1;
                    if (tmpOrder.OrdType == OrderType.ImmediateOrder)
                        tmpOrder.OrderPrice = 0;
                    tmpOrder.UpdatedDate = DateTime.Now;
                    tmpOrder.OrdStatus = OrderStatus.Finished;
                    userFund = tmpFund;
                    userStock = tmpStock;
                }
                userOrder = tmpOrder;
                if (userOrder.OrdStatus == OrderStatus.Finished)
                    return true;
                else
                    return false;
            }
            catch
            {
                dBoughtAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// 购买深证类股票
        /// </summary>
        /// <param name="SZRate"></param>
        /// <param name="userFund"></param>
        /// <param name="userOrder"></param>
        /// <param name="userStock"></param>
        /// <param name="dBoughtAmount"></param>
        /// <returns></returns>
        private bool BuySZ(ref SjshqDBFRecord SZRate, ref UserFund userFund, ref UserOrders userOrder, ref UserStocks userStock, out double dBoughtAmount)
        {
            try
            {
                dBoughtAmount = 0;
                if (userOrder.UserID != userFund.UserID || userStock.UserID != userFund.UserID)
                    return false;
                else if (!userOrder.Side || userOrder.OrdStatus != OrderStatus.Waiting || userFund.UserID != userOrder.UserID)
                    return false;
                else if (userOrder.OrderVolume < 0)
                    return false;
                else if (SZRate.SellingVal1 < 0.001 || SZRate.LatestPrice < 0.001)
                    return false;
                UserFund tmpFund = userFund;
                UserOrders tmpOrder = userOrder;
                UserStocks tmpStock = userStock;
                dBoughtAmount = Common.ConvertPrice((tmpOrder.OrderVolume * SZRate.SellingVal1) * (1 + defaultBuyTax));
                if (Common.ComparePrice(userFund.Cash, dBoughtAmount) >= 0)
                {
                    tmpFund.UsableCash += Common.ConvertPrice((tmpOrder.OrderVolume * tmpOrder.OrderPrice) * (1 + defaultBuyTax));
                    tmpFund.UsableCash -= Common.ConvertPrice(dBoughtAmount);

                    Synchronizer.FundHistory fundHistory = new Synchronizer.FundHistory(); fundHistory.Initialize();
                    fundHistory.UserID = userFund.UserID; fundHistory.OrderID = userOrder.OrderID;
                    fundHistory.OriginalCash = Common.ConvertPrice(userFund.Cash);
                    tmpFund.Cash -= Common.ConvertPrice(dBoughtAmount);
                    fundHistory.ChangedCash = Common.ConvertPrice(tmpFund.Cash - fundHistory.OriginalCash);
                    fundHistory.Curr = tmpOrder.Curr;
                    Common.DBSync.FundChanged(fundHistory, userFund.UserID);

                    if (tmpStock.Volume + tmpOrder.OrderVolume > 0)
                        tmpStock.AveragePrice = Common.ConvertPrice(
                            ((tmpStock.AveragePrice * tmpStock.Volume) + (SZRate.SellingVal1 * tmpOrder.OrderVolume))
                            / (tmpStock.Volume + tmpOrder.OrderVolume) * (1 + defaultBuyTax));
                    else
                        tmpStock.AveragePrice = 0;
                    tmpStock.Volume += tmpOrder.OrderVolume;
                    tmpStock.Curr = tmpOrder.Curr;

                    if (GetStockType(tmpOrder.StockCode, StockMarket.Shenzhen) == StockType.SZ_Warrant)
                        tmpStock.Sellable = true;
                    else
                        tmpStock.Sellable = false;
                    if (tmpFund.UsableCash < 0)
                        tmpFund.UsableCash = 0;
                }
                else
                {
                    tmpOrder.UpdatedDate = DateTime.Now;
                    tmpOrder.OrdStatus = OrderStatus.Failure;
                    Common.DBSync.RecordError(tmpOrder, "(SZ)余额不足：Currency-" + userFund.Curr.ToString().Trim() + "/Cash-" +
                        userFund.Cash.ToString("f3").Trim() + "/Cost-" + dBoughtAmount.ToString("f3").Trim());
                }

                if (tmpOrder.OrdStatus != OrderStatus.Failure)
                {
                    tmpOrder.TradePrice = Common.ConvertPrice(SZRate.SellingVal1);
                    if (tmpOrder.OrdType == OrderType.ImmediateOrder)
                        tmpOrder.OrderPrice = 0;
                    tmpOrder.UpdatedDate = DateTime.Now;
                    tmpOrder.OrdStatus = OrderStatus.Finished;
                    userFund = tmpFund;
                    userStock = tmpStock;
                }
                userOrder = tmpOrder;
                if (userOrder.OrdStatus == OrderStatus.Finished)
                    return true;
                else
                    return false;
            }
            catch
            {
                dBoughtAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// 卖出上证类股票
        /// </summary>
        /// <param name="SHRate"></param>
        /// <param name="userFund"></param>
        /// <param name="userOrder"></param>
        /// <param name="userStock"></param>
        /// <param name="dSoldAmount"></param>
        /// <returns></returns>
        private bool SellSH(ref Show2003DBFRecord SHRate, ref UserFund userFund, ref UserOrders userOrder, ref UserStocks userStock, out double dSoldAmount)
        {
            try
            {
                dSoldAmount = 0;
                if (userOrder.UserID != userFund.UserID || userStock.UserID != userFund.UserID)
                    return false;
                else if (userOrder.Side || userOrder.OrdStatus != OrderStatus.Waiting || userFund.UserID != userOrder.UserID)
                    return false;
                else if (userOrder.OrderVolume < 0)
                    return false;
                else if (SHRate.BuyingVal1 < 0.001 || SHRate.LatestPrice < 0.001)
                    return false;
                UserFund tmpFund = userFund;
                UserOrders tmpOrder = userOrder;
                UserStocks tmpStock = userStock;
                dSoldAmount = Common.ConvertPrice((tmpOrder.OrderVolume * SHRate.BuyingVal1) * (1 - defaultSellTax));
                if (tmpStock.Volume >= tmpOrder.OrderVolume)
                {
                    tmpStock.Volume -= tmpOrder.OrderVolume;

                    Synchronizer.FundHistory fundHistory = new Synchronizer.FundHistory(); fundHistory.Initialize();
                    fundHistory.UserID = userFund.UserID; fundHistory.OrderID = userOrder.OrderID;
                    fundHistory.OriginalCash = Common.ConvertPrice(userFund.Cash);
                    tmpFund.Cash += Common.ConvertPrice(dSoldAmount);
                    fundHistory.ChangedCash = Common.ConvertPrice(tmpFund.Cash - fundHistory.OriginalCash);
                    fundHistory.Curr = tmpOrder.Curr;
                    Common.DBSync.FundChanged(fundHistory, userFund.UserID);

                    tmpFund.UsableCash += Common.ConvertPrice(dSoldAmount);
                    if (tmpStock.Volume > 0)
                        tmpStock.AveragePrice = Common.ConvertPrice(
                            ((tmpStock.AveragePrice * tmpStock.Volume) + (SHRate.BuyingVal1 * tmpOrder.OrderVolume))
                            / (tmpStock.Volume + tmpOrder.OrderVolume) * (1 + defaultSellTax));
                    else
                        tmpStock.AveragePrice = 0;
                    if (tmpFund.Cash < 0)
                        tmpFund.Cash = 0;
                    if (tmpFund.UsableCash < 0)
                        tmpFund.UsableCash = 0;
                }
                else
                {
                    tmpOrder.UpdatedDate = DateTime.Now;
                    tmpOrder.OrdStatus = OrderStatus.Failure;
                    Common.DBSync.RecordError(tmpOrder, "(SH)余股不足：Volume-" +
                        tmpStock.Volume.ToString().Trim() + "/Cost-" + tmpOrder.OrderVolume.ToString().Trim());
                }

                if (tmpOrder.OrdStatus != OrderStatus.Failure)
                {
                    tmpOrder.TradePrice = Common.ConvertPrice(SHRate.BuyingVal1);
                    if (tmpOrder.OrdType == OrderType.ImmediateOrder)
                        tmpOrder.OrderPrice = 0;
                    tmpOrder.UpdatedDate = DateTime.Now;
                    tmpOrder.OrdStatus = OrderStatus.Finished;
                    userFund = tmpFund;
                    userStock = tmpStock;
                }
                userOrder = tmpOrder;
                if (userOrder.OrdStatus == OrderStatus.Finished)
                    return true;
                else
                    return false;
            }
            catch
            {
                dSoldAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// 卖出深证类股票
        /// </summary>
        /// <param name="SZRate"></param>
        /// <param name="userFund"></param>
        /// <param name="userOrder"></param>
        /// <param name="userStock"></param>
        /// <param name="dSoldAmount"></param>
        /// <returns></returns>
        private bool SellSZ(ref SjshqDBFRecord SZRate, ref UserFund userFund, ref UserOrders userOrder, ref UserStocks userStock, out double dSoldAmount)
        {
            try
            {
                dSoldAmount = 0;
                if (userOrder.UserID != userFund.UserID || userStock.UserID != userFund.UserID)
                    return false;
                else if (userOrder.Side || userOrder.OrdStatus != OrderStatus.Waiting || userFund.UserID != userOrder.UserID)
                    return false;
                else if (userOrder.OrderVolume < 0)
                    return false;
                else if (SZRate.BuyingVal1 < 0.001 || SZRate.LatestPrice < 0.001)
                    return false;
                UserFund tmpFund = userFund;
                UserOrders tmpOrder = userOrder;
                UserStocks tmpStock = userStock;
                dSoldAmount = Common.ConvertPrice((tmpOrder.OrderVolume * SZRate.BuyingVal1) * (1 - defaultSellTax));
                if (tmpStock.Volume >= tmpOrder.OrderVolume)
                {
                    tmpStock.Volume -= tmpOrder.OrderVolume;

                    Synchronizer.FundHistory fundHistory = new Synchronizer.FundHistory(); fundHistory.Initialize();
                    fundHistory.UserID = userFund.UserID; fundHistory.OrderID = userOrder.OrderID;
                    fundHistory.OriginalCash = Common.ConvertPrice(userFund.Cash);
                    tmpFund.Cash += Common.ConvertPrice(dSoldAmount);
                    fundHistory.ChangedCash = Common.ConvertPrice(tmpFund.Cash - fundHistory.OriginalCash);
                    fundHistory.Curr = tmpOrder.Curr;
                    Common.DBSync.FundChanged(fundHistory, userFund.UserID);

                    tmpFund.UsableCash += Common.ConvertPrice(dSoldAmount);
                    if (tmpStock.Volume > 0)
                        tmpStock.AveragePrice = Common.ConvertPrice(
                            ((tmpStock.AveragePrice * tmpStock.Volume) + (SZRate.BuyingVal1 * tmpOrder.OrderVolume))
                            / (tmpStock.Volume + tmpOrder.OrderVolume) * (1 + defaultSellTax));
                    else
                        tmpStock.AveragePrice = 0;
                    if (tmpFund.Cash < 0)
                        tmpFund.Cash = 0;
                    if (tmpFund.UsableCash < 0)
                        tmpFund.UsableCash = 0;
                }
                else
                {
                    tmpOrder.UpdatedDate = DateTime.Now;
                    tmpOrder.OrdStatus = OrderStatus.Failure;
                    Common.DBSync.RecordError(tmpOrder, "(SZ)余股不足：Volume" +
                        tmpStock.Volume.ToString().Trim() + "/Cost-" + tmpOrder.OrderVolume.ToString().Trim());
                }

                if (tmpOrder.OrdStatus != OrderStatus.Failure)
                {
                    tmpOrder.TradePrice = Common.ConvertPrice(SZRate.BuyingVal1);
                    if (tmpOrder.OrdType == OrderType.ImmediateOrder)
                        tmpOrder.OrderPrice = 0;
                    tmpOrder.UpdatedDate = DateTime.Now;
                    tmpOrder.OrdStatus = OrderStatus.Finished;
                    userFund = tmpFund;
                    userStock = tmpStock;
                }
                userOrder = tmpOrder;
                if (userOrder.OrdStatus == OrderStatus.Finished)
                    return true;
                else
                    return false;
            }
            catch
            {
                dSoldAmount = 0;
                return false;
            }
        }

        /// <summary>
        /// 订单是否有效(上证类股票)
        /// </summary>
        /// <param name="SHRate"></param>
        /// <param name="userFund"></param>
        /// <param name="userOrder"></param>
        /// <returns></returns>
        private bool IsOrderValid(ref Show2003DBFRecord SHRate, ref UserFund userFund, ref UserOrders userOrder)
        {
            try
            {
                if (userOrder.OrdType == OrderType.ImmediateOrder)
                    return true;
                else if (userOrder.ExpiredDate.Date > DateTime.Now.Date)
                    return true;
                else if (SHRate.StockCode == null || SHRate.StockName == null)
                    return false;
                else if (GetStockType(SHRate.StockCode.Trim(), StockMarket.Shanghai) != StockType.SH_A)
                    return true;
                if (userOrder.Side)
                {
                    if (SHRate.PreClosePrice < 0.01)
                    {
                        userOrder.OrdStatus = OrderStatus.Failure;
                        userOrder.UpdatedDate = DateTime.Now;
                        userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + defaultBuyTax));
                        return false;
                    }
                    else if (SHRate.StockName.ToUpper().Contains("ST"))
                    {
                        if (Common.ComparePrice(userOrder.OrderPrice, SHRate.PreClosePrice * 0.95) < 0)
                        {
                            userOrder.OrdStatus = OrderStatus.Failure;
                            userOrder.UpdatedDate = DateTime.Now;
                            userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + defaultBuyTax));
                            return false;
                        }
                        else
                            return true;
                    }
                    else if (!SHRate.StockName.ToUpper().Contains("N"))
                    {
                        if (Common.ComparePrice(userOrder.OrderPrice, SHRate.PreClosePrice * 0.9) < 0)
                        {
                            userOrder.OrdStatus = OrderStatus.Failure;
                            userOrder.UpdatedDate = DateTime.Now;
                            userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + defaultBuyTax));
                            return false;
                        }
                        else
                            return true;
                    }
                }
                else
                {
                    if (SHRate.PreClosePrice < 0.01)
                    {
                        userOrder.OrdStatus = OrderStatus.Failure;
                        userOrder.UpdatedDate = DateTime.Now;
                        return false;
                    }
                    else if (SHRate.StockName.ToUpper().Contains("ST"))
                    {
                        if (Common.ComparePrice(userOrder.OrderPrice, SHRate.PreClosePrice * 1.05) > 0)
                        {
                            userOrder.OrdStatus = OrderStatus.Failure;
                            userOrder.UpdatedDate = DateTime.Now;
                            return false;
                        }
                        else
                            return true;
                    }
                    else if (!SHRate.StockName.ToUpper().Contains("N"))
                    {
                        if (Common.ComparePrice(userOrder.OrderPrice, SHRate.PreClosePrice * 1.1) > 0)
                        {
                            userOrder.OrdStatus = OrderStatus.Failure;
                            userOrder.UpdatedDate = DateTime.Now;
                            return false;
                        }
                        else
                            return true;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 订单是否有效(深证类股票)
        /// </summary>
        /// <param name="SZRate"></param>
        /// <param name="userFund"></param>
        /// <param name="userOrder"></param>
        /// <returns></returns>
        private bool IsOrderValid(ref SjshqDBFRecord SZRate, ref UserFund userFund, ref UserOrders userOrder)
        {
            try
            {
                if (userOrder.OrdType == OrderType.ImmediateOrder)
                    return true;
                else if (userOrder.ExpiredDate.Date > DateTime.Now.Date)
                    return true;
                else if (SZRate.StockCode == null || SZRate.StockName == null)
                    return false;
                else if (GetStockType(SZRate.StockCode.Trim(), StockMarket.Shenzhen) != StockType.SZ_A)
                    return true;
                if (userOrder.Side)
                {
                    if (SZRate.PreClosePrice < 0.01)
                    {
                        userOrder.OrdStatus = OrderStatus.Failure;
                        userOrder.UpdatedDate = DateTime.Now;
                        userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + defaultBuyTax));
                        return false;
                    }
                    else if (SZRate.StockName.ToUpper().Contains("ST"))
                    {
                        if (Common.ComparePrice(userOrder.OrderPrice, SZRate.PreClosePrice * 0.95) < 0)
                        {
                            userOrder.OrdStatus = OrderStatus.Failure;
                            userOrder.UpdatedDate = DateTime.Now;
                            userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + defaultBuyTax));
                            return false;
                        }
                        else
                            return true;
                    }
                    else if (!SZRate.StockName.ToUpper().Contains("N"))
                    {
                        if (Common.ComparePrice(userOrder.OrderPrice, SZRate.PreClosePrice * 0.9) < 0)
                        {
                            userOrder.OrdStatus = OrderStatus.Failure;
                            userOrder.UpdatedDate = DateTime.Now;
                            userFund.UsableCash += Common.ConvertPrice((userOrder.OrderPrice * userOrder.OrderVolume) * (1 + defaultBuyTax));
                            return false;
                        }
                        else
                            return true;
                    }
                }
                else
                {
                    if (SZRate.PreClosePrice < 0.01)
                    {
                        userOrder.OrdStatus = OrderStatus.Failure;
                        userOrder.UpdatedDate = DateTime.Now;
                        return false;
                    }
                    else if (SZRate.StockName.ToUpper().Contains("ST"))
                    {
                        if (Common.ComparePrice(userOrder.OrderPrice, SZRate.PreClosePrice * 1.05) > 0)
                        {
                            userOrder.OrdStatus = OrderStatus.Failure;
                            userOrder.UpdatedDate = DateTime.Now;
                            return false;
                        }
                        else
                            return true;
                    }
                    else if (!SZRate.StockName.ToUpper().Contains("N"))
                    {
                        if (Common.ComparePrice(userOrder.OrderPrice, SZRate.PreClosePrice * 1.1) > 0)
                        {
                            userOrder.OrdStatus = OrderStatus.Failure;
                            userOrder.UpdatedDate = DateTime.Now;
                            return false;
                        }
                        else
                            return true;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endif