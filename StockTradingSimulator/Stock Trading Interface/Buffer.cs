#if INTERNEL
using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Stock_Trading_Simulator_Kernel;

namespace Stock_Trading_Simulator_Kernel
{
    /// <summary>
    /// 缓存类
    /// </summary>
    public class StkBuffer
    {
        /// <summary>
        /// 用户资金
        /// </summary>
        private Dictionary<int, Dictionary<byte, RemotingInterface.RI_Fund>> mapRIUserFund = null;
        /// <summary>
        /// 用户订单
        /// </summary>
        private Dictionary<int, Dictionary<int, RemotingInterface.RI_Order>> mapRIUserOrders = null;
        /// <summary>
        /// 用户持股
        /// </summary>
        private Dictionary<int, List<RemotingInterface.RI_Stock>> mapRIUserStocks = null;
        /// <summary>
        /// 用户交易记录
        /// </summary>
        private Dictionary<int, List<RemotingInterface.RI_Trading>> mapRIUserTrades = null;
        /// <summary>
        /// 用户资金流水
        /// </summary>
        private Dictionary<int, List<RemotingInterface.RI_FundChanges>> mapRIUserFundChanges = null;


        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            try
            {
                mapRIUserFund = new Dictionary<int, Dictionary<byte, RemotingInterface.RI_Fund>>();
                mapRIUserOrders = new Dictionary<int, Dictionary<int, RemotingInterface.RI_Order>>();
                mapRIUserStocks = new Dictionary<int, List<RemotingInterface.RI_Stock>>();
                mapRIUserTrades = new Dictionary<int, List<RemotingInterface.RI_Trading>>();
                mapRIUserFundChanges = new Dictionary<int, List<RemotingInterface.RI_FundChanges>>();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 清空缓存,由SHELL在零点调用
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            try
            {
                if (mapRIUserFund == null)
                    mapRIUserFund = new Dictionary<int, Dictionary<byte, RemotingInterface.RI_Fund>>();
                else
                    lock (mapRIUserFund)
                        mapRIUserFund.Clear();

                if (mapRIUserOrders == null)
                    mapRIUserOrders = new Dictionary<int, Dictionary<int, RemotingInterface.RI_Order>>();
                else
                    lock (mapRIUserOrders)
                        mapRIUserOrders.Clear();

                if (mapRIUserStocks == null)
                    mapRIUserStocks = new Dictionary<int, List<RemotingInterface.RI_Stock>>();
                else
                    lock (mapRIUserStocks)
                        mapRIUserStocks.Clear();

                if (mapRIUserTrades == null)
                    mapRIUserTrades = new Dictionary<int, List<RemotingInterface.RI_Trading>>();
                else
                    lock (mapRIUserTrades)
                        mapRIUserTrades.Clear();

                if (mapRIUserFundChanges == null)
                    mapRIUserFundChanges = new Dictionary<int, List<RemotingInterface.RI_FundChanges>>();
                else
                    lock (mapRIUserFundChanges)
                        mapRIUserFundChanges.Clear();

                try
                {
                    Common.OrderNotifier.Clear(Common.WebService_PlayID);
                }
                catch
                {
                    Common.Log("OrderNotifier.RI_Clear() Failed. [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]");
                }
                Common.Log("Interface Buffer Cleared");
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        /// <summary>
        /// 更新用户资金
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserFund"></param>
        /// <returns></returns>
        public bool SetUserFund(int UserID, RemotingInterface.RI_Fund UserFund)
        {
            try
            {
                if (UserID <= 0 || UserFund.Curr == RemotingInterface.RI_Currency.Unknown)
                    return false;
                if (Common.ComparePrice(UserFund.Cash + 0.01, UserFund.UsableCash) < 0)
                    return false;
                UserFund.Cash = Common.ConvertPrice(UserFund.Cash, 2);
                UserFund.UsableCash = Common.ConvertPrice(UserFund.UsableCash, 2);
                lock (mapRIUserFund)
                {
                    if(mapRIUserFund.ContainsKey(UserID))
                    {
                        Dictionary<byte, RemotingInterface.RI_Fund> mapCurr = mapRIUserFund[UserID];
                        mapCurr[(byte)UserFund.Curr] = UserFund;
                        mapRIUserFund[UserID] = mapCurr;
                    }
                    else
                    {
                        Dictionary<byte, RemotingInterface.RI_Fund> mapCurr = new Dictionary<byte, RemotingInterface.RI_Fund>();
                        mapCurr[(byte)UserFund.Curr] = UserFund;
                        mapRIUserFund[UserID] = mapCurr;
                    }
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
        /// 更新用户订单
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserOrder"></param>
        /// <returns></returns>
        public bool SetUserOrders(int UserID, RemotingInterface.RI_Order UserOrder)
        {
            try
            {
                if (UserID <= 0)
                    return false;
                if (UserOrder.OrderDate.Date != DateTime.Now.Date
                    || DateTime.Now.TimeOfDay > Common.EndPMTS)
                    return false;

                TradingSystem.StockMarket sMarket = TradingSystem.StockMarket.Unknown;
                if (UserOrder.StockMarket == RemotingInterface.RI_Market.Shanghai)
                    sMarket = TradingSystem.StockMarket.Shanghai;
                else if (UserOrder.StockMarket == RemotingInterface.RI_Market.Shenzhen)
                    sMarket = TradingSystem.StockMarket.Shenzhen;
                TradingSystem.StockType sType = Common.stkTrading.GetStockType(UserOrder.StockCode, sMarket);
                switch (sType)
                {
                    case TradingSystem.StockType.SH_A:
                    case TradingSystem.StockType.SZ_A:
                    case TradingSystem.StockType.SH_Bond:
                    case TradingSystem.StockType.SZ_Bond:
                        {
                            UserOrder.OrderPrice = Common.ConvertPrice(UserOrder.OrderPrice, 2);
                            UserOrder.TradePrice = Common.ConvertPrice(UserOrder.TradePrice, 2);
                        }
                        break;
                    default:
                        {
                            UserOrder.OrderPrice = Common.ConvertPrice(UserOrder.OrderPrice);
                            UserOrder.TradePrice = Common.ConvertPrice(UserOrder.TradePrice);
                        }
                        break;
                }

                if (UserOrder.UpdatedDate < UserOrder.OrderDate)
                    UserOrder.UpdatedDate = UserOrder.OrderDate;

                lock (mapRIUserOrders)
                {
                    if (mapRIUserOrders.ContainsKey(UserID))
                    {
                        Dictionary<int, RemotingInterface.RI_Order> mapOrders = mapRIUserOrders[UserID];
                        mapOrders[UserOrder.OrderID] = UserOrder;
                        mapRIUserOrders[UserID] = mapOrders;
                        Common.Debug("SetOrderInfoInBuffer: UserID-" + UserID + "; StockCode-" + UserOrder.StockCode + (byte)UserOrder.StockMarket +
                           "; OrderID-" + UserOrder.OrderID + "; Type-" + UserOrder.OrderType.ToString() + "; Status-" + UserOrder.OrderStatus.ToString());
                    }
                    else
                    {
                        Dictionary<int, RemotingInterface.RI_Order> mapOrders = new Dictionary<int, RemotingInterface.RI_Order>();
                        mapOrders[UserOrder.OrderID] = UserOrder;
                        mapRIUserOrders[UserID] = mapOrders;
                        Common.Debug("SetOrderInfoInBuffer: UserID-" + UserID + "; StockCode-" + UserOrder.StockCode + (byte)UserOrder.StockMarket +
                           "; OrderID-" + UserOrder.OrderID + "; Type-" + UserOrder.OrderType.ToString() + "; Status-" + UserOrder.OrderStatus.ToString());
                    }
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
        /// 加入用户持股
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserStock"></param>
        /// <returns></returns>
        public bool AddUserStocks(int UserID, RemotingInterface.RI_Stock UserStock)
        {
            try
            {
                if (UserID <= 0 || UserStock.Volume <= 0 || UserStock.SellableVolume < 0)
                    return false;
                if (UserStock.StockCode == null || UserStock.StockCode.Trim().Length != 6
                    || UserStock.StockMarket == RemotingInterface.RI_Market.Unknown)
                    return false;
                lock (mapRIUserStocks)
                {
                    if (mapRIUserStocks.ContainsKey(UserID))
                    {
                        List<RemotingInterface.RI_Stock> listStocks = mapRIUserStocks[UserID];
                        bool bExist = false;
                        for (int i = 0; i < listStocks.Count; i++)
                        {
                            RemotingInterface.RI_Stock data = listStocks[i];
                            if (string.Compare(data.StockCode.ToUpper().Trim(), UserStock.StockCode.ToUpper().Trim()) == 0
                                && data.StockMarket == UserStock.StockMarket)
                            {
                                data.AveragePrice = Common.ConvertPrice((data.AveragePrice * data.Volume + UserStock.AveragePrice * UserStock.Volume)
                                    / (data.Volume + UserStock.Volume));
                                data.Volume += UserStock.Volume;
                                data.SellableVolume += UserStock.SellableVolume;
                                listStocks[i] = data;
                                bExist = true;
                                Common.Debug("SetStocksInfoInBuffer: UserID-" + UserID + "; StockCode-" + data.StockCode + (byte)data.StockMarket +
                                    "; Volume-" + data.Volume + "; SellableVolume-" + data.SellableVolume + "; Price-" + data.AveragePrice.ToString("f3"));
                            }
                        }
                        if (!bExist)
                        {
                            listStocks.Add(UserStock);
                            Common.Debug("AddStocksInfoInBuffer: UserID-" + UserID + "; StockCode-" + UserStock.StockCode + (byte)UserStock.StockMarket +
                               "; Volume-" + UserStock.Volume + "; SellableVolume-" + UserStock.SellableVolume + "; Price-" + UserStock.AveragePrice.ToString("f3"));
                        }
                        mapRIUserStocks[UserID] = listStocks;
                    }
                    else
                    {
                        List<RemotingInterface.RI_Stock> listStocks = new List<RemotingInterface.RI_Stock>();
                        listStocks.Add(UserStock);
                        Common.Debug("AddStocksInfoInBuffer: UserID-" + UserID + "; StockCode-" + UserStock.StockCode + (byte)UserStock.StockMarket +
                            "; Volume-" + UserStock.Volume + "; SellableVolume-" + UserStock.SellableVolume + "; Price-" + UserStock.AveragePrice.ToString("f3"));
                        mapRIUserStocks[UserID] = listStocks;
                    }
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
        /// 合并用户持股
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserStock"></param>
        /// <returns></returns>
        public bool SubUserStocks(int UserID, RemotingInterface.RI_Stock UserStock)
        {
            try
            {
                if (UserID <= 0 || UserStock.Volume <= 0)
                    return false;
                if (UserStock.StockCode == null || UserStock.StockCode.Trim().Length != 6
                    || UserStock.StockMarket == RemotingInterface.RI_Market.Unknown)
                    return false;
                lock (mapRIUserStocks)
                {
                    if (mapRIUserStocks.ContainsKey(UserID))
                    {
                        List<RemotingInterface.RI_Stock> listStocks = mapRIUserStocks[UserID];
                        bool bRtn = false;
                        for (int i = 0; i < listStocks.Count; i++)
                        {
                            if (listStocks[i].SellableVolume >= UserStock.SellableVolume && listStocks[i].StockCode != null
                                && string.Compare(listStocks[i].StockCode.ToUpper().Trim(), UserStock.StockCode.ToUpper().Trim()) == 0
                                && listStocks[i].StockMarket == UserStock.StockMarket && listStocks[i].Volume >= UserStock.Volume)
                            {
                                RemotingInterface.RI_Stock data = listStocks[i];
                                if (data.Volume <= UserStock.Volume)
                                    data.AveragePrice = 0;
                                else
                                    data.AveragePrice = Common.ConvertPrice((data.AveragePrice * data.Volume - UserStock.AveragePrice * UserStock.Volume)
                                        / (data.Volume - UserStock.Volume));
                                data.Volume -= UserStock.Volume;
                                data.SellableVolume -= UserStock.SellableVolume;
                                Common.Debug("SubStocksInfoInBuffer: UserID-" + UserID + "; StockCode-" + data.StockCode + (byte)data.StockMarket +
                                   "; Volume-" + data.Volume + "; SellableVolume-" + data.SellableVolume + "; Price-" + data.AveragePrice.ToString("f3"));
                                if (data.Volume <= 0)
                                    listStocks.RemoveAt(i--);
                                else
                                    listStocks[i] = data;
                                mapRIUserStocks[UserID] = listStocks;
                                bRtn = true;
                                break;
                            }
                        }
                        return bRtn;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        /// <summary>
        /// 更新用户交易
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserTrading"></param>
        /// <returns></returns>
        public bool SetUserTradings(int UserID, RemotingInterface.RI_Trading UserTrading)
        {
            try
            {
                if (UserID <= 0)
                    return false;
                if (UserTrading.TradeDate.Date != DateTime.Now.Date
                    || DateTime.Now.TimeOfDay > Common.EndPMTS)
                    return false;
                lock (mapRIUserTrades)
                {
                    if (mapRIUserTrades.ContainsKey(UserID))
                    {
                        List<RemotingInterface.RI_Trading> listTrades = mapRIUserTrades[UserID];
                        listTrades.Add(UserTrading);
                        mapRIUserTrades[UserID] = listTrades;
                        Common.Debug("SetTradingInfoInBuffer: UserID-" + UserID + "; StockCode-" + UserTrading.StockCode + (byte)UserTrading.StockMarket +
                            "; Volume-" + UserTrading.TradeVolume + "; Price-" + UserTrading.TradePrice.ToString("f3").Trim() + "; Side-" + UserTrading.Side.ToString());
                    }
                    else
                    {
                        List<RemotingInterface.RI_Trading> listTrades = new List<RemotingInterface.RI_Trading>();
                        listTrades.Add(UserTrading);
                        mapRIUserTrades[UserID] = listTrades;
                        Common.Debug("SetTradingInfoInBuffer: UserID-" + UserID + "; StockCode-" + UserTrading.StockCode + (byte)UserTrading.StockMarket +
                          "; Volume-" + UserTrading.TradeVolume + "; Price-" + UserTrading.TradePrice.ToString("f3").Trim() + "; Side-" + UserTrading.Side.ToString());
                    }
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
        /// 更新用户资金流水
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserFundChanges"></param>
        /// <returns></returns>
        public bool SetUserFundChanges(int UserID, RemotingInterface.RI_FundChanges UserFundChanges)
        {
            try
            {
                if (UserID <= 0)
                    return false;
                if (UserFundChanges.ChangedDate.Date != DateTime.Now.Date
                    || DateTime.Now.TimeOfDay > Common.EndPMTS)
                    return false;
                lock (mapRIUserFundChanges)
                {
                    if (mapRIUserFundChanges.ContainsKey(UserID))
                    {
                        List<RemotingInterface.RI_FundChanges> listFundChanges = mapRIUserFundChanges[UserID];
                        listFundChanges.Add(UserFundChanges);
                        mapRIUserFundChanges[UserID] = listFundChanges;
                        Common.Debug("SetUserFundChangesInBuffer: UserID-" + UserID + "; OriginalCash-" + UserFundChanges.OriginalCash.ToString("f3").Trim() +
                            "; ChangedCash-" + UserFundChanges.ChangedCash.ToString("f3").Trim() + "; OrderID-" + UserFundChanges.OrderID.ToString().Trim());
                    }
                    else
                    {
                        List<RemotingInterface.RI_FundChanges> listFundChanges = new List<RemotingInterface.RI_FundChanges>();
                        listFundChanges.Add(UserFundChanges);
                        mapRIUserFundChanges[UserID] = listFundChanges;
                        Common.Debug("SetUserFundChangesInBuffer: UserID-" + UserID + "; OriginalCash-" + UserFundChanges.OriginalCash.ToString("f3").Trim() +
                            "; ChangedCash-" + UserFundChanges.ChangedCash.ToString("f3").Trim() + "; OrderID-" + UserFundChanges.OrderID.ToString().Trim());
                    }
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
        /// 获取用户资金
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public Dictionary<byte, RemotingInterface.RI_Fund> GetUserFund(int UserID)
        {
            try
            {
                Dictionary<int, Dictionary<byte, RemotingInterface.RI_Fund>> mapTempUserFund = null;
                lock (mapRIUserFund)
                    mapTempUserFund = new Dictionary<int, Dictionary<byte, RemotingInterface.RI_Fund>>(mapRIUserFund);
                if (mapTempUserFund != null && mapTempUserFund.ContainsKey(UserID))
                {
                  //  Common.Debug("GetUserFund :" + UserID + "," + mapTempUserFund[UserID].Count);
                    return mapTempUserFund[UserID];
                }
                else
                {
                  //  Common.Debug("GetUserFund :" + UserID);
                    return null;
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户订单
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public Dictionary<int, RemotingInterface.RI_Order> GetUserOrders(int UserID)
        {
            try
            {
                Dictionary<int, Dictionary<int, RemotingInterface.RI_Order>> mapTempUserOrders = null;
                lock (mapRIUserOrders)
                    mapTempUserOrders = new Dictionary<int, Dictionary<int, RemotingInterface.RI_Order>>(mapRIUserOrders);
                if (mapTempUserOrders != null && mapTempUserOrders.ContainsKey(UserID))
                {
                   // Common.Debug("GetUserOrders :" + UserID + "," + mapTempUserOrders[UserID].Count);
                    return mapTempUserOrders[UserID];
                }
                else
                {
                   // Common.Debug("GetUserOrders :" + UserID);
                    return null;
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户持股
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<RemotingInterface.RI_Stock> GetUserStocks(int UserID)
        {
            try
            {
                Dictionary<int, List<RemotingInterface.RI_Stock>> mapTempUserStocks = null;
                lock (mapRIUserStocks)
                    mapTempUserStocks = new Dictionary<int, List<RemotingInterface.RI_Stock>>(mapRIUserStocks);
                if (mapTempUserStocks != null && mapTempUserStocks.ContainsKey(UserID))
                {
                  //  Common.Debug("GetUserStocks :" + UserID + "," + mapTempUserStocks[UserID].Count);
                    return mapTempUserStocks[UserID];
                }
                else
                {
                  //  Common.Debug("GetUserStocks :" + UserID);
                    return null;
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户交易
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<RemotingInterface.RI_Trading> GetUserTradings(int UserID)
        {
            try
            {
                Dictionary<int, List<RemotingInterface.RI_Trading>> mapTempUserTrades = null;
                lock (mapRIUserTrades)
                    mapTempUserTrades = new Dictionary<int, List<RemotingInterface.RI_Trading>>(mapRIUserTrades);
                if (mapTempUserTrades != null && mapTempUserTrades.ContainsKey(UserID))
                    return mapTempUserTrades[UserID];
                else
                    return null;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户资金流水
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<RemotingInterface.RI_FundChanges> GetUserFundChanges(int UserID)
        {
            try
            {
                Dictionary<int, List<RemotingInterface.RI_FundChanges>> mapTempUserTrades = null;
                lock (mapRIUserFundChanges)
                    mapTempUserTrades = new Dictionary<int, List<RemotingInterface.RI_FundChanges>>(mapRIUserFundChanges);
                if (mapTempUserTrades != null && mapTempUserTrades.ContainsKey(UserID))
                    return mapTempUserTrades[UserID];
                else
                    return null;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取指定数目的订单
        /// </summary>
        /// <param name="MaxNumber"></param>
        /// <returns></returns>
        public List<RemotingInterface.RI_AllOrders> GetAllOrders(int MaxNumber)
        {
            try
            {
                if (MaxNumber <= 0)
                    return null;
                Common.Debug(">>> GetAllOrders() Invoking >>>");
                Dictionary<int, Dictionary<int, RemotingInterface.RI_Order>> mapTempUserOrders = null;
                lock (mapRIUserOrders)
                    mapTempUserOrders = new Dictionary<int, Dictionary<int, RemotingInterface.RI_Order>>(mapRIUserOrders);
                List<RemotingInterface.RI_AllOrders> listOriginal = new List<RemotingInterface.RI_AllOrders>();
                List<RemotingInterface.RI_AllOrders> listTarget = new List<RemotingInterface.RI_AllOrders>();
                foreach (object objUserID in mapTempUserOrders.Keys)
                {
                    if (objUserID == null)
                        continue;
                    int nUserID = Convert.ToInt32(objUserID);
                    if (!mapTempUserOrders.ContainsKey(nUserID))
                        continue;
                    foreach (object objOrderID in mapTempUserOrders[nUserID].Keys)
                    {
                        if (objOrderID == null)
                            continue;
                        int nOrderID = Convert.ToInt32(objOrderID);
                        if (!mapTempUserOrders[nUserID].ContainsKey(nOrderID))
                            continue;
                        RemotingInterface.RI_AllOrders data = new RemotingInterface.RI_AllOrders();
                        data.Clear(); data.UserID = nUserID;
                        if (!data.Import(mapTempUserOrders[nUserID][nOrderID]))
                            continue;
                        listOriginal.Add(data);
                    }
                }

                if (listOriginal.Count > 0)
                {
                    int nCount = listOriginal.Count;
                    while (listTarget.Count < nCount && listOriginal.Count > 0)
                    {
                        RemotingInterface.RI_AllOrders target = new RemotingInterface.RI_AllOrders();
                        target.Clear(); target.UpdatedDate = DateTime.MinValue;
                        for (int i = 0; i < listOriginal.Count; i++)
                        {
                            if (listOriginal[i].UpdatedDate > target.UpdatedDate)
                            {
                                target = listOriginal[i];
                            }
                        }
                        if (listOriginal.Contains(target))
                        {
                            listOriginal.Remove(target);
                            listTarget.Add(target);
                        }

                        if (listTarget.Count >= MaxNumber)
                        {
                            Common.Debug("<<< GetAllOrders() Invoked <<<");
                            return listTarget;
                        }
                    }
                }
                Common.Debug("<<< GetAllOrders() Invoked <<<");
                return listTarget;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return null;
            }
        }

        /// <summary>
        /// 获取用户数
        /// </summary>
        /// <returns></returns>
        public int GetUsersCount()
        {
            try
            {
                if (mapRIUserFund != null)
                {
                    int nCount = 0;
                    lock (mapRIUserFund)
                    {
                        nCount = mapRIUserFund.Count;
                    }
                    return nCount;
                }
                else
                    return 0;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return -1;
            }
        }
    }
}
#endif