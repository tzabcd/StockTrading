using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using QDBFAnalyzer.StructuredAnalysis;
using Stock_Trading_Simulator_Kernel;

namespace Stock_Trading_Simulator_Kernel
{
    public partial class TradingSystem
    {
        /// <summary>
        /// 订单类型
        /// </summary>
        public enum OrderType : byte
        {
            None = 0,
            ImmediateOrder = 1,
            LimitedOrder = 2,
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public enum OrderStatus : byte
        {
            Unknown = 0,
            Waiting = 1,
            Cancelling = 2,
            Cancelled = 5,
            Failure = 6,
            Finished = 7,
        }

        /// <summary>
        /// 证券市场
        /// </summary>
        public enum StockMarket : byte
        {
            Unknown = 0,
            Shanghai = 1,
            Shenzhen = 2,
        }

        /// <summary>
        /// 证券类型
        /// </summary>
        public enum StockType : byte
        {
            Unknown = 0,             //未知
            ETF = 1,                 //上市交易型基金
            SH_A = 5,                      //上证A股
            SH_B = 6,                      //上证B股
            SH_OpenedFund = 7,             //上证开放式基金
            SH_ClosedFund = 8,             //上证封闭式基金
            SH_Index = 9,                  //上证指数
            SH_Bond = 10,                  //上证债券
            SH_Warrant = 11,               //上证权证
            SZ_A = 15,               //深证A股
            SZ_B = 16,               //深证B股
            SZ_OpenedFund = 17,      //深证开放式基金
            SZ_ClosedFund = 18,      //深证封闭式基金
            SZ_Index = 19,           //深证指数
            SZ_Bond = 20,            //深证债券
            SZ_Warrant = 21,         //深证权证
        }

        /// <summary>
        /// 币种
        /// </summary>
        public enum Currency : byte
        {
            Unknown = 0,
            AUD = 1,
            CAD = 2,
            CHF = 3,
            EUR = 4,
            GBP = 5,
            HKD = 6,
            JPY = 7,
            NZD = 8,
            RMB = 9,
            USD = 10,
        }

        /// <summary>
        /// 用户资金
        /// </summary>
        public struct UserFund
        {
            public int UserID;
            public double Wealth;
            public double Cash;
            public double UsableCash;
            public Currency Curr;
            public void Initialize()
            {
                try
                {
                    UserID = 0;
                    Wealth = 0;
                    Cash = 0;
                    UsableCash = 0;
                    Curr = Currency.Unknown;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 用户订单
        /// </summary>
        public struct UserOrders
        {
            public int OrderID;
            public int UserID;
            public OrderType OrdType;
            public OrderStatus OrdStatus;
            public bool Side;    // true - buy; false - sell
            public string StockCode;
            public StockMarket Market;
            public int OrderVolume;
            public double OrderPrice;
            public double TradePrice;
            public Currency Curr;
            public DateTime OrderDate;
            public DateTime UpdatedDate;
            public DateTime ExpiredDate;
            public void Initialize()
            {
                try
                {
                    OrderID = 0;
                    UserID = 0;
                    OrdType = OrderType.None;
                    OrdStatus = OrderStatus.Unknown;
                    Side = false;
                    StockCode = "";
                    Market = StockMarket.Unknown;
                    OrderVolume = 0;
                    OrderPrice = 0;
                    TradePrice = 0;
                    Curr = Currency.Unknown;
                    OrderDate = Common.MinDateTime;
                    UpdatedDate = Common.MinDateTime;
                    ExpiredDate = Common.MaxDateTime;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 用户股票
        /// </summary>
        public struct UserStocks
        {
            public int UserID;
            public string StockCode;
            public StockMarket Market;
            public int Volume;
            public double AveragePrice;
            public Currency Curr;
            public bool Sellable;
            public void Initialize()
            {
                try
                {
                    UserID = 0;
                    StockCode = "";
                    Market = StockMarket.Unknown;
                    Volume = 0;
                    AveragePrice = 0;
                    Curr = Currency.Unknown;
                    Sellable = false;
                }
                catch
                { }
            }
        }

    }
}
