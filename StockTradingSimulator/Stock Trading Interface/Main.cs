#if INTERNEL
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Net;
using Network_Service_Core;
using Stock_Trading_Simulator_Kernel.Result_Notifer;
using Stock_Trading_Simulator_Kernel.RI_UserWealth;

namespace Stock_Trading_Simulator_Kernel
{
    public class Common
    {
        private static TimeSpan BeginTradingInterface = new TimeSpan();
        private static TimeSpan EndTradingInterface = new TimeSpan();

        public static RemotingRespond OrderNotifier = new RemotingRespond();
        public static StockTradingUserWealth UserWealthSvc = new StockTradingUserWealth();
        public static TradingSystem stkTrading = null;
        public static Synchronizer DBSync = null;
        public static Quotation stkQuotation = null;
        public static StkBuffer stkBuffer = null;
        public static DateTime MaxDateTime = new DateTime(2099, 12, 31, 23, 59, 59);
        public static DateTime MinDateTime = new DateTime(1901, 1, 1, 0, 0, 0);
        public static short MaxOrders = 0;
        public static int WebService_PlayID = 0;      // 返回给订单通知接口的PlayID
        public static TimeSpan BeginAMTS = new TimeSpan();
        public static TimeSpan EndAMTS = new TimeSpan();
        public static TimeSpan BeginPMTS = new TimeSpan();
        public static TimeSpan EndPMTS = new TimeSpan();
        public static TimeSpan ResetTS = new TimeSpan();
        public static List<DateTime> listHolidays = new List<DateTime>();
        public static string strConn = "";
        public static string strQuotationHistory = "";
        public static string strSHQuotation = "";
        public static string strSZQuotation = "";

        #region 开关标志
        public static bool Switch_NewUser = false;
        public static bool Switch_ImmediateOrder = false;
        public static bool Switch_LimitedOrder = false;
        public static bool Switch_CancelOrder = false;
        public static bool Switch_UserFund = false;
        public static bool Switch_UserOrders = false;
        public static bool Switch_UserStocks = false;
        public static bool Switch_UserTrades = false;
        public static bool Switch_UserFundChanges = false;
        public static bool Switch_ServiceStatus = false;
        public static bool Switch_Configuration = false;
        public static bool Switch_Maintain = false;
        public static bool DbgLog = false;
        #endregion

        /// <summary>
        /// 初始化，入口
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            try
            {
                Common.Log(">>> Loading Configuration Settings...");
                //找配置文件
                if (!File.Exists(Common.Config()))
                {
                    Common.Log("Unaccessible Configuration File. [ " + Common.Config() + " ]");
                    return false;
                }
                //加载交易开放时段配置
                if (!TimeSpan.TryParse(Common.Config("System", "BeginTimeAM"), out Common.BeginAMTS)
                    || !TimeSpan.TryParse(Common.Config("System", "EndTimeAM"), out Common.EndAMTS)
                    || !TimeSpan.TryParse(Common.Config("System", "BeginTimePM"), out Common.BeginPMTS)
                    || !TimeSpan.TryParse(Common.Config("System", "EndTimePM"), out Common.EndPMTS)
                    || Common.BeginAMTS >= Common.EndAMTS || Common.EndAMTS >= Common.BeginPMTS
                    || Common.BeginPMTS >= Common.EndPMTS || Common.BeginAMTS.Hours < 1 || Common.EndPMTS.Hours > 22)
                {
                    Common.Log("<<< Illegal Configuration Settings [System:BeginTime/EndTime (01:00:00~22:59:59)].");
                    return false;
                }
                else
                {
                    Common.Log("Stock Quotation Time: " + BeginAMTS.ToString() + " ~ " + EndAMTS.ToString()
                        + " / " + BeginPMTS.ToString() + " ~ " + EndPMTS.ToString());
                }
                //加载接口开放时段配置
                if (!TimeSpan.TryParse(Common.Config("System", "BeginInterface"), out Common.BeginTradingInterface)
                    || !TimeSpan.TryParse(Common.Config("System", "EndInterface"), out Common.EndTradingInterface)
                    || Common.BeginTradingInterface == Common.EndTradingInterface)
                {
                    Common.Log("<<< Illegal Configuration Settings [System:BeginInterface/EndInterface (00:00:00~23:59:59)].");
                    return false;
                }
                else
                {
                    Common.Log("Interface Quotation Time: " + BeginTradingInterface.ToString() + " ~ " + EndTradingInterface.ToString());
                }
                //加载Debug日志开关配置
                string strDBG = Common.Config("System", "DebugLog").ToUpper().Trim();
                if (strDBG == "1" || strDBG == "TRUE" || strDBG == "T" ||
                    strDBG == "YES" || strDBG == "Y" || strDBG == "OPEN" || strDBG == "OPENED")
                    Common.DbgLog = true;
                //加载假日列表配置
                string strHoliday = "";
                DateTime dtHoliday = DateTime.MinValue;
                listHolidays.Clear();
                for (int i = 0; i < 100; i++)
                {
                    strHoliday = Common.Config("System", "Holiday-" + i.ToString("00").Trim());
                    if (strHoliday.Length > 0 && DateTime.TryParse(strHoliday, out dtHoliday))
                        listHolidays.Add(dtHoliday.Date);
                }
                //加载关联webService配置
                if (!int.TryParse(Common.Config("WebService", "WSPlayID"), out WebService_PlayID))
                    WebService_PlayID = 0;
                Common.OrderNotifier.Url = Common.Config("WebService", "OrderNotifier");
                Common.UserWealthSvc.Url = Common.Config("WebService", "UserWealthSvc");
                //加载数据库连接字串配置
                Common.strConn = Common.Config("Database", "Connection").Trim();
                if (Common.strConn.Length <= 0)
                {
                    Common.Log("<<< Illegal Configuration Settings [Database:Connection].");
                    return false;
                }
                //加载上证、深证行情源配置
                Common.strSHQuotation = Common.Config("Quotation", "Shanghai");
                Common.strSZQuotation = Common.Config("Quotation", "Shenzhen");
                if (Common.strSHQuotation.Length <= 0 || !File.Exists(Common.strSHQuotation)
                    || Common.strSZQuotation.Length <= 0 || !File.Exists(Common.strSZQuotation))
                {
                    Common.Log("<<< Illegal Configuration Settings [Quotation:Shanghai/Shenzhen].");
                    return false;
                }
                //加载实时行情文件存放配置
                Common.strQuotationHistory = Common.Config("Quotation", "History");
                if (Common.strQuotationHistory.Length <= 0 || !Directory.Exists(Common.strQuotationHistory))
                {
                    Common.Log("<<< Illegal Configuration Settings [Quotation:History].");
                    return false;
                }
                //加载最大订单数配置
                if (!short.TryParse(Common.Config("Trading", "MaxOrders"), out Common.MaxOrders)
                    || Common.MaxOrders < 16 || Common.MaxOrders > 256)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:MaxOrders (16~256)].");
                    return false;
                }
                //缓存构造初始化
                Common.stkBuffer = new StkBuffer();
                if (Common.stkBuffer.Initialize())
                {
                    Common.Log("Interface Buffer Initialized");
                }
                else
                {
                    Common.Log("Failed to Initialize The Interface Buffer");
                    return false;
                }
                //交易系统构造
                Common.stkTrading = new TradingSystem();
                #region 加载初始资金配置
                double.TryParse(Common.Config("Trading", "InitAUD"), out Common.stkTrading.defaultAUD);
                if (Common.stkTrading.defaultAUD < 0 || Common.stkTrading.defaultAUD > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitAUD (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitCAD"), out Common.stkTrading.defaultCAD);
                if (Common.stkTrading.defaultCAD < 0 || Common.stkTrading.defaultCAD > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitCAD (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitCHF"), out Common.stkTrading.defaultCHF);
                if (Common.stkTrading.defaultCHF < 0 || Common.stkTrading.defaultCHF > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitCHF (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitEUR"), out Common.stkTrading.defaultEUR);
                if (Common.stkTrading.defaultEUR < 0 || Common.stkTrading.defaultEUR > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitEUR (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitGBP"), out Common.stkTrading.defaultGBP);
                if (Common.stkTrading.defaultGBP < 0 || Common.stkTrading.defaultGBP > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitGBP (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitHKD"), out Common.stkTrading.defaultHKD);
                if (Common.stkTrading.defaultHKD < 0 || Common.stkTrading.defaultHKD > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitHKD (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitJPY"), out Common.stkTrading.defaultJPY);
                if (Common.stkTrading.defaultJPY < 0 || Common.stkTrading.defaultJPY > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitJPY (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitNZD"), out Common.stkTrading.defaultNZD);
                if (Common.stkTrading.defaultNZD < 0 || Common.stkTrading.defaultNZD > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitNZD (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitRMB"), out Common.stkTrading.defaultRMB);
                if (Common.stkTrading.defaultRMB < 0 || Common.stkTrading.defaultRMB > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitRMB (0~1,000,000,000)].");
                    return false;
                }
                double.TryParse(Common.Config("Trading", "InitUSD"), out Common.stkTrading.defaultUSD);
                if (Common.stkTrading.defaultUSD < 0 || Common.stkTrading.defaultUSD > (double)1000 * 1000 * 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitUSD (0~1,000,000,000)].");
                    return false;
                }

                if (Common.stkTrading.defaultAUD < 1000 && Common.stkTrading.defaultCAD < 1000 &&
                    Common.stkTrading.defaultCHF < 1000 && Common.stkTrading.defaultEUR < 1000 &&
                    Common.stkTrading.defaultGBP < 1000 && Common.stkTrading.defaultHKD < 1000 &&
                    Common.stkTrading.defaultJPY < 1000 && Common.stkTrading.defaultNZD < 1000 &&
                    Common.stkTrading.defaultRMB < 1000 && Common.stkTrading.defaultUSD < 1000)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:InitFund (1000~1,000,000,000)].");
                    return false;
                }
                #endregion
                //加载买入手续费配置
                double.TryParse(Common.Config("Trading", "BuyTax"), out Common.stkTrading.defaultBuyTax);
                if (Common.stkTrading.defaultBuyTax < 0 || Common.stkTrading.defaultBuyTax > 0.5)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:BuyTax (0~0.5)].");
                    return false;
                }
                //加载卖出手续费配置
                double.TryParse(Common.Config("Trading", "SellTax"), out Common.stkTrading.defaultSellTax);
                if (Common.stkTrading.defaultSellTax < 0 || Common.stkTrading.defaultSellTax > 0.5)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:SellTax (0~0.5)].");
                    return false;
                }
                //加载单股比例上限配置
                double.TryParse(Common.Config("Trading", "SingleRate"), out Common.stkTrading.defaultSingleStockRate);
                if (Common.stkTrading.defaultSingleStockRate < 0.01 || Common.stkTrading.defaultSingleStockRate > 10)
                {
                    Common.Log("<<< Illegal Configuration Settings [Trading:SingleRate (0.01~10)].");
                    return false;
                }
                //加载订单ID配置
                if (!int.TryParse(Common.Config("Trading", "InitOrderID"), out Common.stkTrading.nLastOrderID))
                    Common.stkTrading.nLastOrderID = 0;
                //交易系统启动
                if (!Common.stkTrading.Initialize())
                {
                    return false;
                }
                //行情系统启动
                Common.stkQuotation = new Quotation();
                if (!Common.stkQuotation.Initialize())
                {
                    return false;
                }

                #region 接口开关
                #region 添加型
                //添加新用户开关
                string strReq = Common.Config("Interface", "NewUser").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_NewUser)
                        Common.Log(" *** Interface-NewUser [Closed] *** ");
                    Common.Switch_NewUser = false;
                }
                else
                {
                    if (!Common.Switch_NewUser)
                        Common.Log(" *** Interface-NewUser [Opened] *** ");
                    Common.Switch_NewUser = true;
                }
                //添加即时定单开关
                strReq = Common.Config("Interface", "ImmediateOrder").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_ImmediateOrder)
                        Common.Log(" *** Interface-ImmediateOrder [Closed] *** ");
                    Common.Switch_ImmediateOrder = false;
                }
                else
                {
                    if (!Common.Switch_ImmediateOrder)
                        Common.Log(" *** Interface-ImmediateOrder [Opened] *** ");
                    Common.Switch_ImmediateOrder = true;
                }
                //添加限价定单开关
                strReq = Common.Config("Interface", "LimitedOrder").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_LimitedOrder)
                        Common.Log(" *** Interface-LimitedOrder [Closed] *** ");
                    Common.Switch_LimitedOrder = false;
                }
                else
                {
                    if (!Common.Switch_LimitedOrder)
                        Common.Log(" *** Interface-LimitedOrder [Opened] *** ");
                    Common.Switch_LimitedOrder = true;
                }
                //添加取消定单开关
                strReq = Common.Config("Interface", "CancelOrder").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_CancelOrder)
                        Common.Log(" *** Interface-CancelOrder [Closed] *** ");
                    Common.Switch_CancelOrder = false;
                }
                else
                {
                    if (!Common.Switch_CancelOrder)
                        Common.Log(" *** Interface-CancelOrder [Opened] *** ");
                    Common.Switch_CancelOrder = true;
                }
                #endregion
                #region 请求型
                strReq = Common.Config("Interface", "UserFund").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserFund)
                        Common.Log(" *** Interface-UserFund [Closed] *** ");
                    Common.Switch_UserFund = false;
                }
                else
                {
                    if (!Common.Switch_UserFund)
                        Common.Log(" *** Interface-UserFund [Opened] *** ");
                    Common.Switch_UserFund = true;
                }

                strReq = Common.Config("Interface", "UserOrders").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserOrders)
                        Common.Log(" *** Interface-UserOrders [Closed] *** ");
                    Common.Switch_UserOrders = false;
                }
                else
                {
                    if (!Common.Switch_UserOrders)
                        Common.Log(" *** Interface-UserOrders [Opened] *** ");
                    Common.Switch_UserOrders = true;
                }

                strReq = Common.Config("Interface", "UserStocks").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserStocks)
                        Common.Log(" *** Interface-UserStocks [Closed] *** ");
                    Common.Switch_UserStocks = false;
                }
                else
                {
                    if (!Common.Switch_UserStocks)
                        Common.Log(" *** Interface-UserStocks [Opened] *** ");
                    Common.Switch_UserStocks = true;
                }

                strReq = Common.Config("Interface", "UserTrades").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserTrades)
                        Common.Log(" *** Interface-UserTrades [Closed] *** ");
                    Common.Switch_UserTrades = false;
                }
                else
                {
                    if (!Common.Switch_UserTrades)
                        Common.Log(" *** Interface-UserTrades [Opened] *** ");
                    Common.Switch_UserTrades = true;
                }

                strReq = Common.Config("Interface", "UserFundChanges").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserFundChanges)
                        Common.Log(" *** Interface-UserFundChanges [Closed] *** ");
                    Common.Switch_UserFundChanges = false;
                }
                else
                {
                    if (!Common.Switch_UserFundChanges)
                        Common.Log(" *** Interface-UserFundChanges [Opened] *** ");
                    Common.Switch_UserFundChanges = true;
                }
                #endregion
                #region 管理型
                strReq = Common.Config("Interface", "ServiceStatus").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_ServiceStatus)
                        Common.Log(" *** Interface-ServiceStatus [Closed] *** ");
                    Common.Switch_ServiceStatus = false;
                }
                else
                {
                    if (!Common.Switch_ServiceStatus)
                        Common.Log(" *** Interface-ServiceStatus [Opened] *** ");
                    Common.Switch_ServiceStatus = true;
                }

                strReq = Common.Config("Interface", "ServiceConfiguration").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_Configuration)
                        Common.Log(" *** Interface-ServiceConfiguration [Closed] *** ");
                    Common.Switch_Configuration = false;
                }
                else
                {
                    if (!Common.Switch_Configuration)
                        Common.Log(" *** Interface-ServiceConfiguration [Opened] *** ");
                    Common.Switch_Configuration = true;
                }

                strReq = Common.Config("Interface", "ServiceMaintain").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_Maintain)
                        Common.Log(" *** Interface-ServiceMaintain [Closed] *** ");
                    Common.Switch_Maintain = false;
                }
                else
                {
                    if (!Common.Switch_Maintain)
                        Common.Log(" *** Interface-ServiceMaintain [Opened] *** ");
                    Common.Switch_Maintain = true;
                }
                #endregion
                #endregion
                //加载remoting配置
                RemotingConfiguration.Configure(Process.GetCurrentProcess().MainModule.FileName + ".config", false);
                Common.Log("Interface System Created");

                try
                {
                    OrderNotifier.Clear(WebService_PlayID);
                }
                catch
                { }
                Common.Log("<<< Configuration Settings Loaded.");
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public static void Uninitialize()
        {
            try
            {
                if (Common.stkQuotation != null)
                    Common.stkQuotation.Uninitialize();
                if (Common.stkTrading != null)
                    Common.stkTrading.Uninitialize();
            }
            catch (Exception err)
            {
                Common.Log(err);
            }
        }

        /// <summary>
        /// 比较价格(1:Price1>Price2; -1:Price1<Price2; 0:Price1==Price2;)
        /// </summary>
        /// <param name="Price1"></param>
        /// <param name="Price2"></param>
        /// <returns></returns>
        public static short ComparePrice(double Price1, double Price2)
        {
            try
            {
                long L1 = (long)(Price1 * 1000 + 0.5);
                long L2 = (long)(Price2 * 1000 + 0.5);
                if (L1 > L2)
                    return 1;
                else if (L1 < L2)
                    return -1;
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 比较价格 原数增大 Digit * 10 倍 (1:Price1>Price2; -1:Price1< Price2; 0:Price1==Price2;)
        /// </summary>
        /// <param name="Price1"></param>
        /// <param name="Price2"></param>
        /// <param name="Digit"></param>
        /// <returns></returns>
        public static short ComparePrice(double Price1, double Price2, ushort Digit)
        {
            try
            {
                if (Digit > 5)
                    Digit = 5;
                long tmp = 1;
                for (int i = 0; i < Digit; i++)
                    tmp *= 10;
                long L1 = (long)(Price1 * tmp + 0.5);
                long L2 = (long)(Price2 * tmp + 0.5);
                if (L1 > L2)
                    return 1;
                else if (L1 < L2)
                    return -1;
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 转换价格 
        /// </summary>
        /// <param name="Price"></param>
        /// <returns></returns>
        public static double ConvertPrice(double Price)
        {
            try
            {
                long tmp1 = 1000;
                long tmp2 = (long)(Price * tmp1 + 0.5);
                return (double)tmp2 / (double)tmp1;
            }
            catch
            {
                return Price;
            }
        }

        /// <summary>
        /// 转换价格 
        /// </summary>
        /// <param name="Price"></param>
        /// <param name="Digit"></param>
        /// <returns></returns>
        public static double ConvertPrice(double Price, ushort Digit)
        {
            try
            {
                if (Digit > 5)
                    Digit = 5;
                long tmp1 = 1;
                for (int i = 0; i < Digit; i++)
                    tmp1 *= 10;
                long tmp2 = (long)(Price * tmp1 + 0.5);
                return (double)tmp2 / (double)tmp1;
            }
            catch
            {
                return Price;
            }
        }

        /// <summary>
        /// 是否周末
        /// </summary>
        public static bool IsWeekend
        {
            get
            {
                try
                {
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday
                        || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    else
                    {
                        foreach (DateTime dt in listHolidays)
                        {
                            if (dt.Date == DateTime.Now.Date)
                                return true;
                        }
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 是否为交易时间
        /// </summary>
        public static bool IsInStockQuotationTime
        {
            get
            {
                try
                {
                    if ((DateTime.Now.TimeOfDay >= Common.BeginAMTS
                        && DateTime.Now.TimeOfDay <= Common.EndAMTS)
                        || (DateTime.Now.TimeOfDay >= Common.BeginPMTS
                        && DateTime.Now.TimeOfDay <= Common.EndPMTS))
                        return true;
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 是否为接口开放时段
        /// </summary>
        public static bool IsInInterfaceQuotationTime
        {
            get
            {
                try
                {
                    if (Common.BeginTradingInterface > Common.EndTradingInterface)
                    {
                        if (DateTime.Now.TimeOfDay >= Common.BeginTradingInterface
                            || DateTime.Now.TimeOfDay <= Common.EndTradingInterface)
                            return true;
                        else
                            return false;
                    }
                    else if (Common.BeginTradingInterface < Common.EndTradingInterface)
                    {
                        if (DateTime.Now.TimeOfDay >= Common.BeginTradingInterface
                            && DateTime.Now.TimeOfDay <= Common.EndTradingInterface)
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static Core_ConfigAndLogger CoreConfig = new Core_ConfigAndLogger();
        private static Core_ConfigAndLogger CoreDebug = new Core_ConfigAndLogger("Debug");
        private static Core_ConfigAndLogger DBLogger = new Core_ConfigAndLogger("DBLog");
        private static Core_ConfigAndLogger DBError = new Core_ConfigAndLogger("DBErr");

        /// <summary>
        /// 用户资产状态
        /// </summary>
        /// <param name="mapUserWealth"></param>
        public static void UsersStatus(Dictionary<int, Synchronizer.UserWealth> mapUserWealth)
        {
            try
            {
                if (mapUserWealth == null)
                    return;

                using (StreamWriter SW = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\UsersWealth.log"
                    , false, Encoding.Default))
                {
                    SW.WriteLine("用户资产统计 [" + DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒") + "]" + Environment.NewLine);
                    foreach (object objKey in mapUserWealth.Keys)
                    {
                        if (objKey == null)
                            continue;
                        int nKey = Convert.ToInt32(objKey);
                        if (!mapUserWealth.ContainsKey(nKey))
                            continue;
                        SW.WriteLine("UserID=" + nKey.ToString().Trim());
                        if (mapUserWealth[nKey].WealthAUD >= 0.01)
                            SW.WriteLine("Wealth(AUD)=" + mapUserWealth[nKey].WealthAUD.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthCAD >= 0.01)
                            SW.WriteLine("Wealth(CAD)=" + mapUserWealth[nKey].WealthCAD.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthCHF >= 0.01)
                            SW.WriteLine("Wealth(CHF)=" + mapUserWealth[nKey].WealthCHF.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthEUR >= 0.01)
                            SW.WriteLine("Wealth(EUR)=" + mapUserWealth[nKey].WealthEUR.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthGBP >= 0.01)
                            SW.WriteLine("Wealth(GBP)=" + mapUserWealth[nKey].WealthGBP.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthHKD >= 0.01)
                            SW.WriteLine("Wealth(HKD)=" + mapUserWealth[nKey].WealthHKD.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthJPY >= 0.01)
                            SW.WriteLine("Wealth(JPY)=" + mapUserWealth[nKey].WealthJPY.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthNZD >= 0.01)
                            SW.WriteLine("Wealth(NZD)=" + mapUserWealth[nKey].WealthNZD.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthRMB >= 0.01)
                            SW.WriteLine("Wealth(RMB)=" + mapUserWealth[nKey].WealthRMB.ToString("f2").Trim());
                        if (mapUserWealth[nKey].WealthUSD >= 0.01)
                            SW.WriteLine("Wealth(USD)=" + mapUserWealth[nKey].WealthUSD.ToString("f2").Trim());
                        SW.Write(Environment.NewLine);
                    }
                    SW.Close();
                }
            }
            catch
            { }
        }
        public static void CurrentStatus(string strIP)
        {
            try
            {
                if (strIP == null)
                    strIP = "";
                else
                    strIP = strIP.Trim();
                using (StreamWriter SW = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\CurrentStatus.log"
                    , false, Encoding.Default))
                {
                    if (Management.Work)
                        SW.WriteLine("[Stock Trading Simulator Service] 开启于" + Management.StatusChangedTime.ToString("yyyy年MM月dd日HH时mm分ss秒") + "。");
                    else
                        SW.WriteLine("[Stock Trading Simulator Service] 停止于" + Management.StatusChangedTime.ToString("yyyy年MM月dd日HH时mm分ss秒") + "。");
                    if (stkTrading != null)
                    {
                        SW.WriteLine("参数设置于" + Management.ConfigChangedTime.ToString("yyyy年MM月dd日HH时mm分ss秒") + "：" + Environment.NewLine +
                            "初始资金(AUD)=" + stkTrading.defaultAUD.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(CAD)=" + stkTrading.defaultCAD.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(CHF)=" + stkTrading.defaultCHF.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(EUR)=" + stkTrading.defaultEUR.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(GBP)=" + stkTrading.defaultGBP.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(HKD)=" + stkTrading.defaultHKD.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(JPY)=" + stkTrading.defaultJPY.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(NZD)=" + stkTrading.defaultNZD.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(RMB)=" + stkTrading.defaultRMB.ToString("f2").Trim() + Environment.NewLine +
                            "初始资金(USD)=" + stkTrading.defaultUSD.ToString("f2").Trim() + Environment.NewLine +
                            "买入交易手续费=" + (stkTrading.defaultBuyTax * 100).ToString("f2").Trim() + "%" + Environment.NewLine +
                            "卖出交易手续费=" + (stkTrading.defaultSellTax * 100).ToString("f2").Trim() + "%" + Environment.NewLine +
                            "单股比例上限=" + (stkTrading.defaultSingleStockRate * 100).ToString("f2").Trim() + "%" + Environment.NewLine +
                            "每日订单上限=" + MaxOrders.ToString().Trim() + Environment.NewLine);
                    }
                    SW.WriteLine("Remoting上次请求时间：" +
                        DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒").Trim());
                    SW.WriteLine("Remoting上次请求地址：" + strIP);
                    SW.Close();
                }
            }
            catch
            { }
        }
        public static void Debug(string strContent)
        {
            try
            {
                if (strContent != null)
                {
                    Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + strContent.Trim());
                    if (DbgLog)
                        CoreDebug.Log(strContent.Trim());
                }
            }
            catch { }
        }
        public static string Config()
        {
            try
            {
                return CoreConfig.ConfigFileName.Trim();
            }
            catch
            {
                return "";
            }
        }
        public static string Config(string strItem, string strKey)
        {
            try
            {
                if (strItem == null || strKey == null || strItem.Trim().Length <= 0 || strKey.Trim().Length <= 0)
                    return "";
                return CoreConfig.GetConfig(strItem.Trim(), strKey.Trim()).Trim();
            }
            catch
            {
                return "";
            }
        }
        public static bool UpdConfig(string strItem, string strKey, string strVal)
        {
            try
            {
                if (strItem == null || strKey == null || strVal == null || strItem.Trim().Length <= 0
                    || strKey.Trim().Length <= 0 || strVal.Trim().Length <= 0)
                    return false;
                return CoreConfig.SetConfig(strItem.Trim(), strKey.Trim(), strVal.Trim());
            }
            catch
            {
                return false;
            }
        }
        public static void Log()
        {
            try
            {
                string strVal = null;
                CoreConfig.Log(strVal);
                Console.WriteLine("* * * * [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] * * * *");
            }
            catch
            { }
        }
        public static void Log(string Value)
        {
            try
            {
                if (Value != null)
                {
                    CoreConfig.Log(Value);
                    Console.WriteLine(Value);
                }
            }
            catch
            { }
        }
        public static void Log(Exception Err)
        {
            try
            {
                if (Err != null && Err.Message != null && Err.StackTrace != null)
                {
                    CoreConfig.Log(Err.Message.Trim() + Environment.NewLine + Err.StackTrace.Trim());
                    Console.WriteLine(Err.Message.Trim() + Environment.NewLine + Err.StackTrace.Trim());
                }
            }
            catch
            { }
        }
        public static void DBLog(Exception Err, string SQL, bool Succ)
        {
            try
            {
                if (Succ)
                {
                    if (SQL != null && SQL.Trim().Length > 0)
                    {
                        DBLogger.Log("SQL: " + SQL.Trim());
                    }
                }
                else
                {
                    if (Err != null && Err.Message != null && SQL != null && SQL.Trim().Length > 0)
                    {
                        DBError.Log(Err.Message.Trim() + Environment.NewLine + "SQL: " + SQL.Trim());
                        Console.WriteLine(Err.Message.Trim() + Environment.NewLine + "SQL: " + SQL.Trim());
                    }
                }
            }
            catch
            { }
        }
        public static void BackupLogs()
        {
            try
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory.Trim()))
                {
                    string[] strLogFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory.Trim(),
                        "*.log", SearchOption.TopDirectoryOnly);
                    if (strLogFiles != null && strLogFiles.Length > 0)
                    {
                        foreach (string strLogFile in strLogFiles)
                        {
                            if (strLogFile != null && File.Exists(strLogFile.Trim()))
                            {
                                File.Move(strLogFile, strLogFile + "." + DateTime.Now.ToString("yyyyMMddHHmmss").Trim() + ".bak");
                            }
                        }
                    }
                }
            }
            catch
            { }
        }
    }
}
#endif