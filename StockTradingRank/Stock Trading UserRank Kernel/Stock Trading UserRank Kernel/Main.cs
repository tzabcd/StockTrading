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
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using QDBFAnalyzer.StructuredAnalysis;
using zns = Stock_Trading_Simulator_Kernel;
 
namespace Stock_Trading_UserRank_Kernel
{
    #region 主要操作类
    public class Common
    {
        private static SqlConnection sqlConn = null;
        private static SqlCommand sqlCmd = null;
        private static SqlDataReader sqlReader = null;
        private static SqlTransaction sqlTrans = null;

        private static string strConn = "";

        private static Thread ThTrading = null;
        private static bool bRanking = false;
        private static bool bInit = true;
        private static bool bWealthDone = false;

        public static zns.RemotingInterface[] znRmtIobj = new zns.RemotingInterface[BaseConfig.mapNotifySrv.Count];
        public static Quotation QuoteSvc = new Quotation();
        public static DateTime CurrentDay = DateTime.Today;

        public static bool Debug()
        {
            Initialize();
            while(true)
            {
                Thread.Sleep(1000);
            }
        }
        //初始化操作
        public static bool Initialize()
        {
            try
            {
                strConn = BaseConfig.ConnStr;
                Common.QuoteSvc.SetQuotaionLocation(BaseConfig.DBFSHPath, BaseConfig.DBFSZPath);

                sqlConn = new SqlConnection(strConn.Trim());

                for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                {
                    znRmtIobj[i] = Activator.GetObject(typeof(zns.RemotingInterface), BaseConfig.mapNotifySrv[i].ri) as zns.RemotingInterface;
                }
                Loger.Debug("<<< Configuration Settings Loaded.");

                bRanking = true;
                bInit = true;
                ThTrading = new Thread(new ThreadStart(Run));
                ThTrading.Name = "ThTrading";
                ThTrading.Start();

                return true;
            }
            catch (Exception err)
            {
                Loger.Debug("Initialize Error:"+err.ToString());
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        //卸载操作
        public static void Uninitialize()
        {
            try
            {
                bRanking = false;
                if (ThTrading != null && ThTrading.IsAlive)
                {
                    ThTrading.Join(4500);
                    if (ThTrading != null && ThTrading.IsAlive)
                        ThTrading.Abort();
                }
                if (sqlConn != null && sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
            catch (Exception err)
            {
                Loger.Debug(err.ToString());
            }
            finally
            {
                Loger.Debug("Ranking System Terminated");
            }
        }

        //循环运行
        public static void Run()
        {
            while (bRanking)
            {
                if (bInit == true
                    || (CurrentDay != DateTime.Today && DateTime.Now.Hour == BaseConfig.RunTimeHour))
                {
                    if (IsWorkDate())
                    {
                        Common.ProcWealth();
                        //if (bWealthDone)
                        //    Common.ProcRank();

                        //JYWS.RemotingRespond rankSync = new global::Stock_Trading_UserRank_Kernel.JYWS.RemotingRespond();
                        //rankSync.Url = BaseConfig.RankNotifySrv;
                        //rankSync.Ranks_HandledAsync(BaseConfig.PlayId);
                    }
                    else
                    {
                        Loger.Debug("节假日无排名，运行正常。");
                    }
                    bInit = false;
                    CurrentDay = DateTime.Today;
                }
                Thread.Sleep(1000 * 60 );
            }
        }

        /// <summary>
        /// 计算当日资产
        /// </summary>
        /// <returns></returns>
        public static bool ProcWealth()
        {
            bool bInTransaction = false;
            try
            {
                Loger.Debug(">>> Processing [Wealth] Started ! <<<");

                if (!Common.QuoteSvc.ReloadQuotation())
                {
                    Loger.Debug("--- Reloading Quotation Failed. ---");
                    return false;
                }
                Show2003DBFRecord SHRecord = new Show2003DBFRecord(); SHRecord.Clear();
                SjshqDBFRecord SZRecord = new SjshqDBFRecord(); SZRecord.Clear();
                Dictionary<int, UserRank> mapUserWealth = new Dictionary<int, UserRank>();

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();
                sqlTrans = sqlConn.BeginTransaction();
                bInTransaction = true;

                //读取指定活动参与交易的用户列表
                sqlCmd = new SqlCommand(@"emtradeplay.dbo.GetTradeUserListByPlayId", sqlConn, sqlTrans);
                sqlCmd.Parameters.Add("@playId", SqlDbType.Int).Value = BaseConfig.PlayId;
                sqlReader = sqlCmd.ExecuteReader();

                int nUser = 0;
                while (sqlReader.Read())
                {
                    int userId = Convert.ToInt32(sqlReader["UserId"]);

                    string userDataBase = sqlReader["UserDataBase"].ToString();
                    if (userDataBase == null || userDataBase == string.Empty)
                    {
                        Loger.Debug("--- warning : userId = " + userId + " DataBase = null ---");
                        continue;
                    }

                    zns.RemotingInterface cRmt = null;

                    foreach (KeyValuePair<int, NotifySrv> s in BaseConfig.mapNotifySrv)
                    {
                        if (userDataBase == s.Value.DataBaseChar)
                        {
                            cRmt = Common.znRmtIobj[s.Key];
                            break;
                        }
                    } 

                    #region 各币种股票总市值
                    double StocksWealthRMB = 0;
                    double StocksWealthUSD = 0;
                    double StocksWealthHKD = 0;
                    double tempOneStockWealth = 0;
                    double StocksWealth = 0;  //所有币种股票总市值

                    List<zns.RemotingInterface.RI_Stock> listStock = new List<Stock_Trading_Simulator_Kernel.RemotingInterface.RI_Stock>();
                    listStock = cRmt.RequestUserStocks(BaseConfig.RmtUserKey, userId);
                    if (listStock != null)
                    {
                        foreach (zns.RemotingInterface.RI_Stock stock in listStock)
                        {
                            tempOneStockWealth = 0;

                            if (stock.StockMarket == zns.RemotingInterface.RI_Market.Shanghai)
                            {
                                if (Common.QuoteSvc.FindQuotation(stock.StockCode, out SHRecord))
                                {
                                    if (SHRecord.LatestPrice < 0.001 || SHRecord.OpenPrice < 0.001)
                                        tempOneStockWealth = ConvertPrice(SHRecord.PreClosePrice) * stock.Volume;
                                    else
                                        tempOneStockWealth = ConvertPrice(SHRecord.LatestPrice) * stock.Volume;
                                }
                            }
                            else if (stock.StockMarket == zns.RemotingInterface.RI_Market.Shenzhen)
                            {
                                if (Common.QuoteSvc.FindQuotation(stock.StockCode, out SZRecord))
                                {
                                    if (SZRecord.LatestPrice < 0.001 || SZRecord.OpenPrice < 0.001)
                                        tempOneStockWealth = ConvertPrice(SZRecord.PreClosePrice) * stock.Volume;
                                    else
                                        tempOneStockWealth = ConvertPrice(SZRecord.LatestPrice) * stock.Volume;
                                }
                            }

                            switch (stock.Curr)
                            {
                                case zns.RemotingInterface.RI_Currency.RMB:
                                    StocksWealthRMB += tempOneStockWealth; break;

                                case zns.RemotingInterface.RI_Currency.USD:
                                    StocksWealthUSD += tempOneStockWealth; break;

                                case zns.RemotingInterface.RI_Currency.HKD:
                                    StocksWealthHKD += tempOneStockWealth; break;
                            }
                        }
                        StocksWealth = StocksWealthRMB + StocksWealthUSD * BaseConfig.RateUSD + StocksWealthHKD * BaseConfig.RateHKD;
                    }
                    #endregion
                    
                    #region 各币种现金
                    double CashRMB = 0;
                    double CashUSD = 0;
                    double CashHKD = 0;
                    
                    Dictionary<byte, zns.RemotingInterface.RI_Fund> mapUserFund = new Dictionary<byte, Stock_Trading_Simulator_Kernel.RemotingInterface.RI_Fund>();
                    mapUserFund = cRmt.RequestUserFund(BaseConfig.RmtUserKey, userId);
                    if (mapUserFund != null)
                    {
                        foreach (KeyValuePair<byte, zns.RemotingInterface.RI_Fund> fund in mapUserFund)
                        {
                            switch ((zns.RemotingInterface.RI_Currency)fund.Key)
                            {
                                case zns.RemotingInterface.RI_Currency.RMB:
                                    CashRMB = fund.Value.Cash; break;

                                case zns.RemotingInterface.RI_Currency.USD:
                                    CashUSD = fund.Value.Cash; break;

                                case zns.RemotingInterface.RI_Currency.HKD:
                                    CashHKD = fund.Value.Cash; break;
                            }
                        }
                    }
                    #endregion

                    #region 各币种现有总资产
                    double WealthRMB = 0;
                    double WealthUSD = 0;
                    double WealthHKD = 0;
                    double Wealth = 0;
                    WealthRMB = StocksWealthRMB + CashRMB;
                    WealthUSD = StocksWealthUSD + CashUSD;
                    WealthHKD = StocksWealthHKD + CashHKD;
                    Wealth = WealthRMB + WealthUSD * BaseConfig.RateUSD + WealthHKD * BaseConfig.RateHKD;
                    #endregion

                    #region 持仓比例
                    double RatioRMB = 0;
                    double RatioUSD = 0;
                    double RatioHKD = 0;
                    if (WealthRMB > 0)
                    {
                        RatioRMB = 1 - (CashRMB / WealthRMB);
                        if (RatioRMB > 1)
                            RatioRMB = 1;
                        else if (RatioRMB < 0)
                            RatioRMB = 0;
                    }
                    else
                    {
                        RatioRMB = 0;
                    }

                    if (WealthUSD > 0)
                    {
                        RatioUSD = 1 - (CashUSD / WealthUSD);
                        if (RatioUSD > 1)
                            RatioUSD = 1;
                        else if (RatioUSD < 0)
                            RatioUSD = 0;
                    }
                    else
                    {
                        RatioUSD = 0;
                    }

                    if (WealthHKD > 0)
                    {
                        RatioHKD = 1 - (CashHKD / WealthHKD);
                        if (RatioHKD > 1)
                            RatioHKD = 1;
                        else if (RatioHKD < 0)
                            RatioHKD = 0;
                    }
                    else
                    {
                        RatioHKD = 0;
                    }
                    #endregion
                     
                    #region 收益率
                    double Profit = 0;
                    double InitCash = BaseConfig.InitCashRMB + BaseConfig.InitCashUSD * BaseConfig.RateUSD
                                   + BaseConfig.InitCashHKD * BaseConfig.RateHKD;
                    Profit = (Wealth - InitCash) / InitCash * 100;
                    if (Wealth == 0)
                        Profit = 0;
                    #endregion

                    UserRank data = new UserRank();
                    data.Initialize();

                    data.UserId = userId;
                    data.AreaId = (int)sqlReader["AreaId"];
                    data.UserName = sqlReader["UserName"].ToString();
                    data.UserDataBase = sqlReader["UserDataBase"].ToString();

                    data.Wealth = Wealth;
                    data.WealthRMB = WealthRMB;
                    data.WealthUSD = WealthUSD;
                    data.WealthHKD = WealthHKD;

                    data.StockWealth = StocksWealth;

                    data.RatioRMB = RatioRMB;
                    data.RatioUSD = RatioUSD;
                    data.RatioHKD = RatioHKD;

                    data.Profit = Profit;
                    data.RankDate = DateTime.Now.Date;
                    mapUserWealth[userId] = data;

                    nUser++;
                }  //while(sqlReader.Read()) end
                sqlReader.Close();
                Loger.Debug("---calculate [StocksWealth] [Cash] [Wealth] [Ratio] [Profit] finished . UserCount = " + nUser + "---");

                #region 根据已更新的资产缓存更新数据库表
                int nUserID = 0;
                sqlCmd = new SqlCommand("DELETE FROM [DailyRank]", sqlConn, sqlTrans);
                sqlCmd.ExecuteNonQuery();
                nUser = 0;
                foreach (var data in mapUserWealth)
                {
                    sqlCmd = new SqlCommand("INSERT INTO [DailyRank] (UserID,AreaId,UserName,UserDataBase,Wealth, WealthRMB, WealthUSD, WealthHKD,StockWealth, " +
                        "Profit, DailyProfit, WeeklyProfit, MonthlyProfit, RatioRMB, RatioUSD, RatioHKD, RatioUnderDays, RankDate) " +

                        "VALUES (@UserID,@AreaId,@UserName,@UserDataBase, @Wealth, @WealthRMB, @WealthUSD, @WealthHKD,@StockWealth," +
                        "@Profit, @DailyProfit, @WeeklyProfit, @MonthlyProfit, @RatioRMB, @RatioUSD, @RatioHKD, @RatioUnderDays, @RankDate)", sqlConn, sqlTrans);
                    //用户信息
                    sqlCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = data.Value.UserId;
                    sqlCmd.Parameters.Add("@AreaId", SqlDbType.Int).Value = data.Value.AreaId;
                    sqlCmd.Parameters.Add("@UserName", SqlDbType.VarChar, 32).Value = data.Value.UserName;
                    sqlCmd.Parameters.Add("@UserDataBase", SqlDbType.VarChar, 16).Value = data.Value.UserDataBase;
                    //资产总值
                    sqlCmd.Parameters.Add("@Wealth", SqlDbType.Money).Value = data.Value.Wealth;
                    sqlCmd.Parameters.Add("@WealthRMB", SqlDbType.Money).Value = data.Value.WealthRMB;
                    sqlCmd.Parameters.Add("@WealthUSD", SqlDbType.Money).Value = data.Value.WealthUSD;
                    sqlCmd.Parameters.Add("@WealthHKD", SqlDbType.Money).Value = data.Value.WealthHKD;
                    //股票市值
                    sqlCmd.Parameters.Add("@StockWealth", SqlDbType.Money).Value = data.Value.StockWealth;
                    //收益率
                    sqlCmd.Parameters.Add("@Profit", SqlDbType.Money).Value = data.Value.Profit;
                    sqlCmd.Parameters.Add("@DailyProfit", SqlDbType.Money).Value = data.Value.DailyProfit;
                    sqlCmd.Parameters.Add("@WeeklyProfit", SqlDbType.Money).Value = data.Value.WeeklyProfit;
                    sqlCmd.Parameters.Add("@MonthlyProfit", SqlDbType.Money).Value = data.Value.MonthlyProfit;
                    //持仓比例
                    sqlCmd.Parameters.Add("@RatioRMB", SqlDbType.Money).Value = data.Value.RatioRMB;
                    sqlCmd.Parameters.Add("@RatioUSD", SqlDbType.Money).Value = data.Value.RatioUSD;
                    sqlCmd.Parameters.Add("@RatioHKD", SqlDbType.Money).Value = data.Value.RatioHKD;
                    //低于持仓标准的天数
                    if (mapUserWealth[nUserID].RatioRMB < BaseConfig.RatioBaseLine) //当日持仓未达标则记录
                    {
                        sqlCmd.Parameters.Add("@RatioUnderDays", SqlDbType.Int).Value = 1;
                    }
                    else
                    {
                        sqlCmd.Parameters.Add("@RatioUnderDays", SqlDbType.Int).Value = 0;
                    }
                    //排名日期
                    sqlCmd.Parameters.Add("@RankDate", SqlDbType.DateTime).Value = data.Value.RankDate.ToString("yyyy-MM-dd");
                    sqlCmd.ExecuteNonQuery();
                    nUser++;
                }
                Loger.Debug("---Update table dailyrank finished . userCount = " + nUser + "---");
                #endregion

                #region 当天排名数据备份到单独表
                sqlCmd = new SqlCommand("CreateHistoryRankByDate", sqlConn, sqlTrans);
                sqlCmd.ExecuteNonQuery();
                #endregion

                sqlTrans.Commit();
                Loger.Debug("<<< Processing [Wealth] Finished ! >>>");
                bWealthDone = true;
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug("ProcWealth error :"+err);
                if (bInTransaction && sqlTrans != null && sqlTrans.Connection != null
                    && sqlTrans.Connection.State == ConnectionState.Open)
                {
                    if (sqlReader != null && !sqlReader.IsClosed)
                        sqlReader.Close();
                    sqlTrans.Rollback();
                }
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Price"></param>
        /// <returns></returns>
        public static double ConvertPrice(double Price)
        {
            try
            {
                long tmp1 = 1000;
                long tmp2 = (long)(Price * tmp1);
                return (double)tmp2 / (double)tmp1;
            }
            catch
            {
                return Price;
            }
        }

        /// <summary>
        /// 判断是否为有行情日，即排除周末和节假日
        /// </summary>
        /// <returns></returns>
        public static bool IsWorkDate()
        {
            bool bWorkDay = true;
            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday
                || DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
            {
                bWorkDay = false;
            }
            if (BaseConfig.mapHoliday.ContainsKey(DateTime.Today))
            {
                if (BaseConfig.mapHoliday[DateTime.Today])
                {
                    bWorkDay = false;
                }
            }
            return bWorkDay;
        } 

    }
    #endregion


}