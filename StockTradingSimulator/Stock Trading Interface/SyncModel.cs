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
   public partial class Synchronizer
    {

        /// <summary>
        /// 特殊标记
        /// </summary>
        private enum Special_OrderID : short
        {
            Bonus = -1,           // 分红
            ExRights = -2,        // 除权
            Punishment = -3,      // 处罚，扣除现金
        }

        /// <summary>
        /// 成交记录
        /// </summary>
        private struct TradingHistory
        {
            public int TradeID;
            public int OrderID;
            public int UserID;
            public bool Side;
            public string StockCode;
            public TradingSystem.StockMarket Market;
            public int TradeVolume;
            public double TradePrice;
            public TradingSystem.Currency Curr;
            public DateTime TradeDate;
            public void Initialize()
            {
                try
                {
                    TradeID = 0;
                    OrderID = 0;
                    UserID = 0;
                    Side = false;
                    StockCode = "";
                    Market = TradingSystem.StockMarket.Unknown;
                    TradeVolume = 0;
                    TradePrice = 0;
                    Curr = TradingSystem.Currency.Unknown;
                    TradeDate = Common.MinDateTime;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 交易错误信息
        /// </summary>
        private struct TradingError
        {
            public int OrderID;
            public string Description;
            public DateTime ErrDate;
            public void Initialize()
            {
                try
                {
                    OrderID = 0;
                    Description = "";
                    ErrDate = Common.MinDateTime;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 资金流水
        /// </summary>
        public struct FundHistory
        {
            public int UserID;
            public double OriginalCash;
            public double ChangedCash;
            public TradingSystem.Currency Curr;
            public DateTime ChangedTime;
            public int OrderID;
            public void Initialize()
            {
                try
                {
                    UserID = 0;
                    OriginalCash = 0;
                    ChangedCash = 0;
                    Curr = TradingSystem.Currency.Unknown;
                    ChangedTime = Common.MinDateTime;
                    OrderID = 0;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 用户资产
        /// </summary>
        public struct UserWealth
        {
            public double WealthAUD;
            public double WealthCAD;
            public double WealthCHF;
            public double WealthEUR;
            public double WealthGBP;
            public double WealthHKD;
            public double WealthJPY;
            public double WealthNZD;
            public double WealthRMB;
            public double WealthUSD;
            public void Initialize()
            {
                try
                {
                    WealthAUD = 0;
                    WealthCAD = 0;
                    WealthCHF = 0;
                    WealthEUR = 0;
                    WealthGBP = 0;
                    WealthHKD = 0;
                    WealthJPY = 0;
                    WealthNZD = 0;
                    WealthRMB = 0;
                    WealthUSD = 0;
                }
                catch
                { }
            }
        }
    }
}
