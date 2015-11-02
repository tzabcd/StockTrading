using System;
using System.Collections.Generic;
using System.Text;

namespace Stock_Trading_Simulator_Kernel
{
    public partial class RemotingInterface 
    {
        /// <summary>
        /// 结果状态
        /// </summary>
        public enum RI_Result : byte
        {
            Success = 0,
            Illegal_UserID = 1,  //非法用户
            Banned_Stock = 2, // 禁止交易的股票
            Out_Of_Quotation_Time = 3,  //不在交易时间
            Illegal_Volume = 4,  //非法笔数
            Illegal_Price = 5,  //非法价格
            Illegal_Cancelling = 6,  //非法取消
            Too_Many_Orders = 7,  //订单过多
            Not_Enough_Stock = 8,  //无足够的股票
            Not_Enough_Cash = 9,  //无足够的资金
            Suspended_Stock = 10,
            Speculation_Behavior = 11,
            Existent_User = 12,
            Illegal_InitFund = 13,
            Illegal_Tax = 14,
            Illegal_SingleStockRate = 15,
            Illegal_OrdersMax = 16,
            Illegal_Quotiety = 17,
            Out_Of_Maintain_Time = 18,
            Illegal_Currency = 19,
            Too_Many_Species = 20,

            Service_Start = 95,
            Service_Stop = 96,
            Null_Interface = 97,
            Closed_Interface = 98,
            Unauthorized = 99,
            Internal_Error = 100,
        }

        /// <summary>
        /// 市场类型
        /// </summary>
        public enum RI_Market : byte
        {
            Unknown = 0,
            Shanghai = 1,
            Shenzhen = 2,
        }

        /// <summary>
        /// 订单类型
        /// </summary>
        public enum RI_Type : byte
        {
            None = 0,
            ImmediateOrder = 1,
            LimitedOrder = 2,
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public enum RI_Status : byte
        {
            Unknown = 0,
            Waiting = 1,
            Cancelling = 2,
            Cancelled = 5,
            Failure = 6,
            Finished = 7,
        }

        /// <summary>
        /// 货币类型
        /// </summary>
        public enum RI_Currency : byte
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
        /// 资金
        /// </summary>
        [Serializable]
        public struct RI_Fund
        {
            public double Cash;
            public double UsableCash;
            public double Wealth;
            public RI_Currency Curr;
            public void Clear()
            {
                try
                {
                    Cash = 0;
                    UsableCash = 0;
                    Wealth = 0;
                    Curr = RI_Currency.Unknown;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 单个订单
        /// </summary>
        [Serializable]
        public struct RI_Order
        {
            public int OrderID;
            public RemotingInterface.RI_Type OrderType;
            public RemotingInterface.RI_Status OrderStatus;
            public bool Side;    // true - buy; false - sell
            public string StockCode;
            public RemotingInterface.RI_Market StockMarket;
            public int OrderVolume;
            public double OrderPrice;
            public double TradePrice;
            public RI_Currency Curr;
            public DateTime OrderDate;
            public DateTime UpdatedDate;
            public DateTime ExpiredDate;
            public void Clear()
            {
                try
                {
                    OrderID = 0;
                    OrderType = RemotingInterface.RI_Type.None;
                    OrderStatus = RemotingInterface.RI_Status.Unknown;
                    Side = false;
                    StockCode = "";
                    StockMarket = RemotingInterface.RI_Market.Unknown;
                    OrderVolume = 0;
                    OrderPrice = 0;
                    TradePrice = 0;
                    Curr = RI_Currency.Unknown;
                    OrderDate = DateTime.MinValue;
                    UpdatedDate = DateTime.MinValue;
                    ExpiredDate = DateTime.MaxValue;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 指定用户的订单
        /// </summary>
        [Serializable]
        public struct RI_AllOrders
        {
            public int UserID;
            public int OrderID;
            public RemotingInterface.RI_Type OrderType;
            public RemotingInterface.RI_Status OrderStatus;
            public bool Side;    // true - buy; false - sell
            public string StockCode;
            public RemotingInterface.RI_Market StockMarket;
            public int OrderVolume;
            public double OrderPrice;
            public double TradePrice;
            public RI_Currency Curr;
            public DateTime OrderDate;
            public DateTime UpdatedDate;
            public DateTime ExpiredDate;
            public void Clear()
            {
                try
                {
                    UserID = 0; OrderID = 0;
                    OrderType = RemotingInterface.RI_Type.None;
                    OrderStatus = RemotingInterface.RI_Status.Unknown;
                    Side = false;
                    StockCode = "";
                    StockMarket = RemotingInterface.RI_Market.Unknown;
                    OrderVolume = 0;
                    OrderPrice = 0;
                    TradePrice = 0;
                    Curr = RI_Currency.Unknown;
                    OrderDate = DateTime.MinValue;
                    UpdatedDate = DateTime.MinValue;
                    ExpiredDate = DateTime.MaxValue;
                }
                catch
                { }
            }

            /// <summary>
            /// 导入单个订单
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public bool Import(RI_Order data)
            {
                try
                {
                    OrderID = data.OrderID;
                    OrderType = data.OrderType;
                    OrderStatus = data.OrderStatus;
                    Side = data.Side;
                    StockCode = data.StockCode;
                    StockMarket = data.StockMarket;
                    OrderVolume = data.OrderVolume;
                    OrderPrice = data.OrderPrice;
                    TradePrice = data.TradePrice;
                    OrderDate = data.OrderDate;
                    Curr = data.Curr;
                    UpdatedDate = data.UpdatedDate;
                    ExpiredDate = data.ExpiredDate;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 单只股票
        /// </summary>
        [Serializable]
        public struct RI_Stock
        {
            public string StockCode;
            public RemotingInterface.RI_Market StockMarket;
            public int Volume;
            public int SellableVolume;
            public double AveragePrice;
            public RI_Currency Curr;
            public void Clear()
            {
                try
                {
                    StockCode = "";
                    StockMarket = RemotingInterface.RI_Market.Unknown;
                    Volume = 0;
                    SellableVolume = 0;
                    AveragePrice = 0;
                    Curr = RI_Currency.Unknown;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 交易
        /// </summary>
        [Serializable]
        public struct RI_Trading
        {
            public bool Side;
            public string StockCode;
            public RemotingInterface.RI_Market StockMarket;
            public int TradeVolume;
            public double TradePrice;
            public RI_Currency Curr;
            public DateTime TradeDate;
            public void Clear()
            {
                try
                {
                    Side = false;
                    StockCode = "";
                    StockMarket = RemotingInterface.RI_Market.Unknown;
                    TradeVolume = 0;
                    TradePrice = 0;
                    Curr = RI_Currency.Unknown;
                    TradeDate = DateTime.MinValue;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 资金流水
        /// </summary>
        [Serializable]
        public struct RI_FundChanges
        {
            public double OriginalCash;
            public double ChangedCash;
            public RI_Currency Curr;
            public DateTime ChangedDate;
            public int OrderID;
            public void Clear()
            {
                try
                {
                    OriginalCash = 0;
                    ChangedCash = 0;
                    Curr = RI_Currency.Unknown;
                    ChangedDate = DateTime.MinValue;
                    OrderID = 0;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 配置项
        /// </summary>
        [Serializable]
        public struct RI_Configuration
        {
            public double InitAUD;
            public double InitCAD;
            public double InitCHF;
            public double InitEUR;
            public double InitGBP;
            public double InitHKD;
            public double InitJPY;
            public double InitNZD;
            public double InitRMB;
            public double InitUSD;
            public double BuyTax;
            public double SellTax;
            public double SingleStockRate;
            public short MaxOrders;
            public int WSPlayID;
            public List<string> ListSpecies;
            public void Clear()
            {
                try
                {
                    InitAUD = 0;
                    InitCAD = 0;
                    InitCHF = 0;
                    InitEUR = 0;
                    InitGBP = 0;
                    InitHKD = 0;
                    InitJPY = 0;
                    InitNZD = 0;
                    InitRMB = 0;
                    InitUSD = 0;
                    BuyTax = 0;
                    SellTax = 0;
                    SingleStockRate = 0;
                    MaxOrders = 0;
                    WSPlayID = 0;
                    ListSpecies = new List<string>();
                }
                catch
                { }
            }
        }

    }
}
