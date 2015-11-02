using System;
using System.Collections.Generic;
using System.Text;

namespace Stock_Trading_UserRank_Kernel
{
    /// <summary>
    /// 用户等级
    /// </summary>
    public struct UserLevel
    {
        public int LevelID;
        public double Percentage;
        public string Title;
        public void Initialize()
        {
            try
            {
                LevelID = 0;
                Percentage = 1;
                Title = "";
            }
            catch
            { }
        }
    }

    /// <summary>
    /// 用户排名
    /// </summary>
    public struct UserRank
    {
        public int UserId;
        public int AreaId;
        public string UserName;
        public string UserDataBase;

        //总资产
        public double Wealth;
        public double WealthRMB;
        public double WealthUSD;
        public double WealthHKD;

        //股票市值
        public double StockWealth;

        //收益率
        public double Profit;
        public double DailyProfit;
        public double WeeklyProfit;
        public double MonthlyProfit;

        //持仓比例
        public double RatioRMB;
        public double RatioUSD;
        public double RatioHKD;

        //名次及变化
        public int RankId;
        public int RankChanged;
        public int AreaRankId;
        public int AreaRankChanged;

        //排名日期
        public DateTime RankDate;

        //头衔
        public string Title;
        public string AreaTitle;
        public void Initialize()
        {
            try
            {
                UserId = 0;
                AreaId = 0;
                UserName = "";
                UserDataBase = "";

                Wealth = 0.00;
                WealthRMB = 0.00;
                WealthUSD = 0.00;
                WealthHKD = 0.00;

                StockWealth = 0.00;

                Profit = 0.00;
                DailyProfit = 0.00;
                WeeklyProfit = 0.00;
                MonthlyProfit = 0.00;

                RatioRMB = 0.00;
                RatioUSD = 0.00;
                RatioHKD = 0.00;

                RankId = 0;
                RankChanged = 0;
                AreaRankId = 0;
                AreaRankChanged = 0;

                RankDate = DateTime.MinValue;

                Title = "";
                AreaTitle = "";
            }
            catch
            { }
        }
    }
}
