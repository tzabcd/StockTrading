using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using QDBFAnalyzer.StructuredAnalysis;
using zns = EastMoney.StocksTrader.RemotingProvider;

namespace CalcDailyRank
{
    public delegate void AsyncEventHandler();
    public delegate void DlgGetWealth(ref Dictionary<int, UserRank> mapUserRank);

    class RankSystem
    {
        private string strConn = "";
        private SqlConnection sqlConn = null;
        private SqlCommand sqlCmd = null;
        private SqlDataReader sqlReader = null;
        private SqlTransaction sqlTrans = null;

        private zns.ITransactionRemotingProvider[] znRmtIobj = new zns.ITransactionRemotingProvider[BaseConfig.mapNotifySrv.Count];

        private Quotation QuoteSvc = new Quotation();

        string BasePath = "";

        private Dictionary<int, UserRank> mapUser = null; 

        private DateTime CurrDay;
        private string strTbCalcDay = "";
 
        private Thread thRank = null;
        private bool bRankTread = false;

        private DateTime dtLastRankDay;
        private bool bRankFirst;

        public bool Initialize()
        {
            strConn = BaseConfig.ConnRank;
            sqlConn = new SqlConnection(strConn.Trim());

            QuoteSvc.SetQuotaionLocation(BaseConfig.DBFSHPath, BaseConfig.DBFSZPath);
 
            dtLastRankDay = DateTime.MinValue;
            bRankFirst = false;

            bRankTread = true;
            thRank = new Thread(new ThreadStart(RankCenter));
            thRank.IsBackground = true;
            thRank.Start();

            return true;
        }

        public bool Uninitialize()
        {
            bRankTread = false;

            return true;
        }

        private void RankCenter()
        {
            while (bRankTread)
            {
                try
                {
                    Loger.State(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : RankCenter is running . ");

                    if (DateTime.Now.TimeOfDay < BaseConfig.RankTime) // 在收盘前
                    {
                        Thread.Sleep(1000 * 60);
                        continue;
                    }
                    if (!DaysUtils.IsQuoteDate(DateTime.Today)) //无行情日
                    {
                        Thread.Sleep(1000 * 60);
                        continue;
                    }
                    if (DateTime.Today.Date == dtLastRankDay.Date && bRankFirst == true) //当天排过名
                    {
                        Thread.Sleep(1000 * 60);
                        continue;
                    }
                    //if (IsDBFTransSuccess()==false)
                    //{
                    //    Loger.Debug(" --- 未获取到最新行情源！--- ");
                    //    Thread.Sleep(1000 * 60);
                    //    continue;
                    //}

                    CurrDay = DateTime.Today;

                    dtLastRankDay = CurrDay;
                    bRankFirst = true;

                    if (!string.IsNullOrEmpty(BaseConfig.strRankDate))
                    {
                        if (!DateTime.TryParse(BaseConfig.strRankDate, out CurrDay))
                        {
                            Loger.Debug(" --- 指定日期格式不正确！采用当天时间 --- ");
                        }
                    }
                    Loger.Debug(" --- CurrDay = " + CurrDay.ToString("yyyy-MM-dd") + " --- ");

                    if (!DaysUtils.IsQuoteDate(CurrDay))
                    {
                        Loger.Debug(" --- 警告 : 当天没有行情，无法进行排名 " + CurrDay + "--- ");
                        continue;
                    }

                    strTbCalcDay = "HistoryRank_" + CurrDay.ToString("yyMMdd");

                    bool bRet = false;

                    try
                    {
                       SpecialStock.SendBonus();
                       SpecialStock.SendAllotment();
                    }
                    catch (Exception ex)
                    {
                        Loger.Debug(ex.ToString());
                    }

                    bRet = CalcWealth();
                    if (bRet == false)
                        continue;

                    bRet = LoadUserHistory();
                    if (bRet == false)
                        continue;

                    bRet = DurationRank();
                    if (bRet == false)
                        continue;

                    //bRet = CleanseUser();
                    //if (bRet == false)
                    //    continue;

                    bRet = DailyRank();
                    if (bRet == false)
                        continue;

                    bRet = DailyProfit();
                    //if (bRet == false)
                    //{
                    //    bRet = WeeklyProfit();
                    //    if (bRet == false)
                    //        continue;

                    //    bRet = MonthlyProfit();
                    //    if (bRet == false)
                    //        continue;
                    //}

                    bRet = UpdateDailyRank();
                    if (bRet == false)
                        continue;

                    bRet = ResponseHandler(BaseConfig.PlayId);
                }
                catch (Exception ex)
                {
                    Loger.Debug("---error :" + ex.ToString());
                    continue;
                }
            }
            Loger.Debug("<<< Rank System has been terminated >>> ");
        }

        #region 获取撮合系统相关数据,预处理

        private bool CalcWealth()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始统计交易用户盘后资产市值 <<<");

                if (!QuoteSvc.ReloadQuotation())
                {
                    Loger.Debug("--- Reloading Quotation Failed. ---");
                    return false;
                }

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();
                sqlTrans = sqlConn.BeginTransaction();
               
                Dictionary<int, UserRank>[] arr_mapDailyRank = new Dictionary<int, UserRank>[BaseConfig.mapNotifySrv.Count];
                DlgGetWealth[] arr_dlgGetWealth = new DlgGetWealth[BaseConfig.mapNotifySrv.Count];
                //初始化remoting调用接口、用户服务器数组、异步委托数组
                for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                {
                    znRmtIobj[i] = Activator.GetObject(typeof(zns.ITransactionRemotingProvider), BaseConfig.mapNotifySrv[i].ri) as zns.ITransactionRemotingProvider;
                    arr_mapDailyRank[i] = new Dictionary<int, UserRank>();
                    arr_dlgGetWealth[i] = new DlgGetWealth(InvokeGetWealth);
                }

                Dictionary<int, Dictionary<int, UserRank>> mapDBDailyRank = new Dictionary<int, Dictionary<int, UserRank>>(); //按服务器区分
                mapDBDailyRank.Clear();

                string strSqlLoadUser=@"SELECT * FROM EMsnsTradePlay.dbo.UserList"+BaseConfig.PlayId;
                Loger.Debug("strSqlLoadUser = " + strSqlLoadUser);
                sqlCmd = new SqlCommand(strSqlLoadUser, sqlConn, sqlTrans);
                sqlCmd.Parameters.Add("@PlayId", SqlDbType.Int).Value = BaseConfig.PlayId;
                sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    int userId = Convert.ToInt32(sqlReader["userId"]);
                    int areaId = Convert.ToInt32(sqlReader["areaId"]);
                    string userName = sqlReader["userName"].ToString();
                    string userDataBase = sqlReader["UserDataBase"].ToString();

                    UserRank currRank = new UserRank();
                    currRank.Initialize();

                    currRank.UserId = userId;
                    currRank.AreaId = areaId;
                    currRank.UserName = userName;
                    currRank.UserDataBase = userDataBase;
 
                    int arrId = (int)char.Parse(userDataBase) - (int)'A';
                    arr_mapDailyRank[arrId][userId] = currRank;
                    
                }  //while(sqlReader.Read()) end
                sqlReader.Close();
               
 
                //按服务器分别异步执行获取资产市值
                IAsyncResult[] ia = new IAsyncResult[BaseConfig.mapNotifySrv.Count];
                for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                {
                    Loger.Debug("---arr[" + i + "] userCount = " + arr_mapDailyRank[i].Count + " ---");
                    ia[i] = arr_dlgGetWealth[i].BeginInvoke(ref arr_mapDailyRank[i], null, null);
                }
                //扫描各异步操作完成状态
                while(true)
                {
                    Thread.Sleep(1000);
                    int nCompletedCount=0;
                    for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                    {
                        if(ia[i].IsCompleted)
                        {
                            nCompletedCount++;
                        }
                    }
                    if (nCompletedCount == BaseConfig.mapNotifySrv.Count)
                        break;
                    else
                        Loger.State(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " : watting asyncResult for GetWealth . nCompletedCount = " + nCompletedCount);
                }
                //合并异步处理后的结果
                Dictionary<int, UserRank> mapDailyRank = new Dictionary<int, UserRank>();
                mapDailyRank.Clear();
                for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                {
                    Dictionary<int, UserRank> t_mapDailyRank = new Dictionary<int, UserRank>(arr_mapDailyRank[i]);
                    foreach (var data in t_mapDailyRank)
                    {
                        mapDailyRank[data.Value.UserId] = data.Value;
                    }
                }

                Loger.Debug("---Calculate [StocksWealth] [Cash] [Wealth] [Ratio] [Profit] finished . ---");
                Loger.Debug("---Calculate userCount = " + mapDailyRank.Count + " ---");

                #region 根据已更新的资产缓存更新数据库表
                sqlCmd = new SqlCommand("DELETE FROM [DailyRank]", sqlConn, sqlTrans);
                sqlCmd.ExecuteNonQuery();

                foreach (var user in mapDailyRank)
                {
                    UserRank data = user.Value;

                    sqlCmd = new SqlCommand("INSERT INTO [DailyRank] (UserID,AreaId,UserName,UserDataBase,Wealth, WealthRMB, WealthUSD, WealthHKD,StockWealth, " +
                        "Profit, DailyProfit, WeeklyProfit, MonthlyProfit, RatioRMB, RatioUSD, RatioHKD, RatioUnderDays, RankDate) " +

                        "VALUES (@UserID,@AreaId,@UserName,@UserDataBase, @Wealth, @WealthRMB, @WealthUSD, @WealthHKD,@StockWealth," +
                        "@Profit, @DailyProfit, @WeeklyProfit, @MonthlyProfit, @RatioRMB, @RatioUSD, @RatioHKD, @RatioUnderDays, @RankDate)", sqlConn, sqlTrans);
                    //用户信息
                    sqlCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = data.UserId;
                    sqlCmd.Parameters.Add("@AreaId", SqlDbType.Int).Value = data.AreaId;
                    sqlCmd.Parameters.Add("@UserName", SqlDbType.VarChar, 32).Value = data.UserName;
                    sqlCmd.Parameters.Add("@UserDataBase", SqlDbType.VarChar, 16).Value = data.UserDataBase;
                    //资产总值
                    sqlCmd.Parameters.Add("@Wealth", SqlDbType.Money).Value = data.Wealth;
                    sqlCmd.Parameters.Add("@WealthRMB", SqlDbType.Money).Value = data.WealthRMB;
                    sqlCmd.Parameters.Add("@WealthUSD", SqlDbType.Money).Value = data.WealthUSD;
                    sqlCmd.Parameters.Add("@WealthHKD", SqlDbType.Money).Value = data.WealthHKD;
                    //股票市值
                    sqlCmd.Parameters.Add("@StockWealth", SqlDbType.Money).Value = data.StockWealth;
                    //收益率
                    sqlCmd.Parameters.Add("@Profit", SqlDbType.Money).Value = data.Profit;
                    sqlCmd.Parameters.Add("@DailyProfit", SqlDbType.Money).Value = data.DailyProfit;
                    sqlCmd.Parameters.Add("@WeeklyProfit", SqlDbType.Money).Value = data.WeeklyProfit;
                    sqlCmd.Parameters.Add("@MonthlyProfit", SqlDbType.Money).Value = data.MonthlyProfit;
                    //持仓比例
                    sqlCmd.Parameters.Add("@RatioRMB", SqlDbType.Money).Value = data.RatioRMB;
                    sqlCmd.Parameters.Add("@RatioUSD", SqlDbType.Money).Value = data.RatioUSD;
                    sqlCmd.Parameters.Add("@RatioHKD", SqlDbType.Money).Value = data.RatioHKD;
                    //持仓合格与否
                    sqlCmd.Parameters.Add("@RatioUnderDays", SqlDbType.Int).Value = data.RatioUnderDays;
                    //排名日期
                    sqlCmd.Parameters.Add("@RankDate", SqlDbType.DateTime).Value = data.RankDate.ToString("yyyy-MM-dd");
                    sqlCmd.ExecuteNonQuery();
                }
                Loger.Debug("---Update table dailyrank finished . ---");
                #endregion

                #region 当天排名数据备份到单独表
                sqlCmd = new SqlCommand("CreateHistoryRankByDate '" + CurrDay.ToString("yyyy-MM-dd") + "'", sqlConn, sqlTrans);
                sqlCmd.ExecuteNonQuery();

                DateTime BaseDayQuoteDay;
                DateTime CalcDay;
                BaseDayQuoteDay = CurrDay.AddDays(-1);
                while (!DaysUtils.IsQuoteDate(BaseDayQuoteDay))
                {
                    BaseDayQuoteDay = BaseDayQuoteDay.AddDays(-1);
                }
                CalcDay = CurrDay;
                string strTbBaseLastDay = "HistoryRank_" + BaseDayQuoteDay.ToString("yyMMdd");
                Loger.Debug(strTbBaseLastDay + " <=========> " + strTbCalcDay);

                //日收益率
//                sqlCmd = new SqlCommand(@"update emtradeplay." + strTbCalcDay + @"
//                                                        set dailyprofit = a.profit - b.profit 
//                                                        from emtradeplay." + strTbCalcDay + @" a,emtradeplay." + strTbBaseLastDay + @" b
//                                                        where a.userid=b.userid", sqlConn, sqlTrans);
//                sqlCmd.ExecuteNonQuery();
//                Loger.Debug("<<< 日收益率计算结束 >>> ");
                #endregion

                sqlTrans.Commit();
                Loger.Debug("<<< 交易用户盘后资产市值统计结束 >>>");
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(ReplaceSqlPara(sqlCmd));
                Loger.Debug(err);
                if (sqlTrans != null && sqlTrans.Connection != null
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

        private void InvokeGetWealth(ref Dictionary<int,UserRank> m_mapDailyRank)
        {
            Show2003DBFRecord SHRecord = new Show2003DBFRecord(); SHRecord.Clear();
            SjshqDBFRecord SZRecord = new SjshqDBFRecord(); SZRecord.Clear();

            Dictionary<int, UserRank> t_mapDailyRank = new Dictionary<int, UserRank>(m_mapDailyRank);
            foreach (var user in t_mapDailyRank)
            {
                UserRank currRank = new UserRank();
                currRank = user.Value;

                int userId = currRank.UserId;
                zns.ITransactionRemotingProvider cRmt = null;

                foreach (var s in BaseConfig.mapNotifySrv)
                {
                    if (currRank.UserDataBase == s.Value.DataBaseChar)
                    {
                        cRmt = znRmtIobj[s.Key];
                        break;
                    }
                }

                //SaveBuffer(cRmt,userId);

                #region 各币种股票总市值
                double StocksWealthRMB = 0;
                double StocksWealthUSD = 0;
                double StocksWealthHKD = 0;
                double tempOneStockWealth = 0;
                double StocksWealth = 0;  //所有币种股票总市值

                List<zns.Interface.RI_Stock> listStock = new List<EastMoney.StocksTrader.RemotingProvider.Interface.RI_Stock>();
                listStock = cRmt.RequestUserStocks(BaseConfig.RmtUserKey, userId);
                if (listStock != null)
                {
                    foreach (zns.Interface.RI_Stock stock in listStock)
                    {
                        tempOneStockWealth = 0;

                        if (stock.StockMarket == zns.Interface.RI_Market.Shanghai)
                        {
                            if (QuoteSvc.FindQuotation(stock.StockCode, out SHRecord))
                            {
                                if (SHRecord.LatestPrice < 0.001 || SHRecord.OpenPrice < 0.001)
                                    tempOneStockWealth = ConvertPrice(SHRecord.PreClosePrice) * stock.Volume;
                                else
                                    tempOneStockWealth = ConvertPrice(SHRecord.LatestPrice) * stock.Volume;
                            }
                        }
                        else if (stock.StockMarket == zns.Interface.RI_Market.Shenzhen)
                        {
                            if (QuoteSvc.FindQuotation(stock.StockCode, out SZRecord))
                            {
                                if (SZRecord.LatestPrice < 0.001 || SZRecord.OpenPrice < 0.001)
                                    tempOneStockWealth = ConvertPrice(SZRecord.PreClosePrice) * stock.Volume;
                                else
                                    tempOneStockWealth = ConvertPrice(SZRecord.LatestPrice) * stock.Volume;
                            }
                        }

                        switch (stock.Curr)
                        {
                            case zns.Interface.RI_Currency.RMB:
                                StocksWealthRMB += tempOneStockWealth; break;

                            case zns.Interface.RI_Currency.USD:
                                StocksWealthUSD += tempOneStockWealth; break;

                            case zns.Interface.RI_Currency.HKD:
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

                Dictionary<byte, zns.Interface.RI_Fund> mapUserFund = new Dictionary<byte, EastMoney.StocksTrader.RemotingProvider.Interface.RI_Fund>();
                mapUserFund = cRmt.RequestUserFund(BaseConfig.RmtUserKey, userId);
                if (mapUserFund != null)
                {
                    foreach (KeyValuePair<byte, zns.Interface.RI_Fund> fund in mapUserFund)
                    {
                        switch ((zns.Interface.RI_Currency)fund.Key)
                        {
                            case zns.Interface.RI_Currency.RMB:
                                CashRMB = fund.Value.Cash; break;

                            case zns.Interface.RI_Currency.USD:
                                CashUSD = fund.Value.Cash; break;

                            case zns.Interface.RI_Currency.HKD:
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

                currRank.Wealth = Wealth;
                currRank.WealthRMB = WealthRMB;
                currRank.WealthUSD = WealthUSD;
                currRank.WealthHKD = WealthHKD;

                currRank.StockWealth = StocksWealth;

                currRank.RatioRMB = RatioRMB;
                currRank.RatioUSD = RatioUSD;
                currRank.RatioHKD = RatioHKD;
                if (currRank.RatioRMB < BaseConfig.RatioBaseLine)
                    currRank.RatioUnderDays = 1;
                else
                    currRank.RatioUnderDays = 0;

                currRank.Profit = Profit;
                currRank.DailyProfit = Profit; //初始时用当日收益,后面会修正
                currRank.RankDate = CurrDay.Date;
                m_mapDailyRank[userId] = currRank;
            }
        }

        private void SaveBuffer(zns.ITransactionRemotingProvider cRmt,int userId)
        {
            try
            {
                //用户订单
                Dictionary<int, zns.Interface.RI_Order> mapUserOrder = new Dictionary<int, zns.Interface.RI_Order>();
                mapUserOrder = cRmt.RequestUserOrders(BaseConfig.RmtUserKey, userId);
                if (mapUserOrder != null)
                    Loger.Serialize(BasePath + "/UserOrder/" + userId + ".dat", mapUserOrder);
                else
                    Loger.Serialize(BasePath + "/UserOrder/" + userId + ".dat", "");
            }
            catch (System.Exception e)
            {
                Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                Loger.Debug(BasePath + "/LostUser.log", userId + " -- mapUserOrder --");
            }
            try
            {
                //用户持股
                List<zns.Interface.RI_Stock> listUserStock = new List<zns.Interface.RI_Stock>();
                listUserStock = cRmt.RequestUserStocks(BaseConfig.RmtUserKey, userId);
                if (listUserStock != null)
                    Loger.Serialize(BasePath + "/UserStock/" + userId + ".dat", listUserStock);
                else
                    Loger.Serialize(BasePath + "/UserStock/" + userId + ".dat", "");
            }
            catch (System.Exception e)
            {
                Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                Loger.Debug(BasePath + "/LostUser.log", userId + " -- listUserStock --");
            }
            try
            {
                //用户交易
                List<zns.Interface.RI_Trading> listUserTrade = new List<zns.Interface.RI_Trading>();
                listUserTrade = cRmt.RequestUserTrades(BaseConfig.RmtUserKey, userId);
                if (listUserTrade != null)
                    Loger.Serialize(BasePath + "/UserTrade/" + userId + ".dat", listUserTrade);
                else
                    Loger.Serialize(BasePath + "/UserTrade/" + userId + ".dat", "");
            }
            catch (System.Exception e)
            {
                Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                Loger.Debug(BasePath + "/LostUser.log", userId + " -- listUserTrade --");
            }
            try
            {
                //用户资金
                Dictionary<byte, zns.Interface.RI_Fund> mapUserFund = new Dictionary<byte, zns.Interface.RI_Fund>();
                mapUserFund = cRmt.RequestUserFund(BaseConfig.RmtUserKey, userId);
                if (mapUserFund != null)
                    Loger.Serialize(BasePath + "/UserFund/" + userId + ".dat", mapUserFund);
                else
                    Loger.Serialize(BasePath + "/UserFund/" + userId + ".dat", "");
            }
            catch (System.Exception e)
            {
                Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                Loger.Debug(BasePath + "/LostUser.log", userId + " -- mapUserFund --");
            }
            try
            {
                //用户资金流水
                List<zns.Interface.RI_FundChanges> listUserFundChange = new List<zns.Interface.RI_FundChanges>();
                listUserFundChange = cRmt.RequestUserFundChanges(BaseConfig.RmtUserKey, userId);
                if (listUserFundChange != null)
                    Loger.Serialize(BasePath + "/UserFundChange/" + userId + ".dat", listUserFundChange);
                else
                    Loger.Serialize(BasePath + "/UserFundChange/" + userId + ".dat", "");
            }
            catch (System.Exception e)
            {
                Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                Loger.Debug(BasePath + "/LostUser.log", userId + " -- listUserFundChange --");
            }
        }

        #endregion

        #region 用户历史累积数据,预处理

        private bool LoadUserHistory()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始加载用户历史信息 <<< ");

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                mapUser = new Dictionary<int, UserRank>();

                //读取指定活动参与交易的用户列表
                sqlCmd = new SqlCommand(@"SELECT  p.PlayId, g.GameId, a.AreaId, u.* 
                                                         FROM EMsnsTradePlay.dbo.UserList"+BaseConfig.PlayId+@" u, EMsnsTradePlay.dbo.Play p, EMsnsTradePlay.dbo.Game g, EMsnsTradePlay.dbo.Area a
                                                         WHERE u.AreaId = a.AreaId
                                                         AND a.GameId=g.GameId
                                                         AND g.PlayId = p.PlayId
                                                         AND u.Validity = 1
                                                         AND u.TradeFlag = 1
                                                         AND p.PlayId = @PlayId", sqlConn);
                sqlCmd.Parameters.Add("@PlayId", SqlDbType.Int).Value = BaseConfig.PlayId;
                sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    UserRank u = new UserRank();
                    u.Initialize();
                    u.UserId = Convert.ToInt32(sqlReader["UserId"]);
                    u.AreaId = Convert.ToInt32(sqlReader["AreaId"]);
                    u.UserName = sqlReader["UserName"].ToString();
                    u.UserDataBase = sqlReader["UserDataBase"].ToString();
                    mapUser[u.UserId] = u;
                }
                sqlReader.Close();
                Loger.Debug(" --- 所有交易用户加载 Count = " + mapUser.Count + " --- ");

                DateTime UserHistoryDay;
                UserHistoryDay = CurrDay;
                if (DateTime.Now.Hour < 15) // 在收盘前
                {
                    UserHistoryDay = UserHistoryDay.AddDays(-1);
                }
                while (true)
                {
                    while (!DaysUtils.IsQuoteDate(UserHistoryDay))
                    {
                        UserHistoryDay = UserHistoryDay.AddDays(-1);
                    }
                    string strTbUserHistoryDay = "HistoryRank_" + UserHistoryDay.ToString("yyMMdd");

                    sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbUserHistoryDay + "' AND  type = 'U'", sqlConn);
                    if (sqlCmd.ExecuteScalar() == null)
                        break;

                    Loger.Debug(" --- 提取历史表 [" + strTbUserHistoryDay + "] --- ");

                    sqlCmd = new SqlCommand("SELECT * FROM " + strTbUserHistoryDay + " ORDER BY Wealth DESC ", sqlConn);
                    sqlReader = sqlCmd.ExecuteReader();
                    while (sqlReader.Read())
                    {
                        int nUserId = Convert.ToInt32(sqlReader["UserId"]);
                        if (!mapUser.ContainsKey(nUserId))
                            continue;

                        UserRank uRank = new UserRank();
                        uRank.Initialize();
                        uRank = mapUser[nUserId];
                        uRank.RankDate = Convert.ToDateTime(sqlReader["RankDate"]);
                        uRank.RatioRMB = Convert.ToDouble(sqlReader["RatioRMB"]);
                        uRank.RatioUnderDays = Convert.ToByte(sqlReader["RatioUnderDays"]);
                        uRank.DailyProfit = Convert.ToDouble(sqlReader["DailyProfit"]);

                        string strRatioFlag = "0";
                        if (uRank.RatioUnderDays == 0)
                        {
                            strRatioFlag = "1";
                        }
                        string strProfitFlag = "0";
                        if (uRank.DailyProfit > 0.00d) //连续盈利天数标记(0表示负/1表示正)
                        {
                            strProfitFlag = "1";
                        }
                        uRank.strRatioFlag += strRatioFlag;
                        uRank.strProfitFlag += strProfitFlag;
                        mapUser[uRank.UserId] = uRank;
                    }
                    sqlReader.Close();
                    UserHistoryDay = UserHistoryDay.AddDays(-1);
                }

                Loger.Debug("<<< 历史数据加载结束 >>> ");
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(ReplaceSqlPara(sqlCmd));
                Loger.Debug(err);
                if (sqlReader != null && !sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        private bool CleanseUser()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始清理有效排名用户 <<< ");

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                sqlCmd = new SqlCommand(@"UpdateUserRatioUnderDays");
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.Add("@PlayId", SqlDbType.Int).Value = BaseConfig.PlayId;
                sqlCmd.Connection = sqlConn;
                sqlCmd.ExecuteNonQuery();

                Loger.Debug("<<< 有效排名清理结束 >>>");
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                if (sqlReader != null && !sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        private bool DurationRank()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始计算持仓不合格天数及持续盈利天数名次 <<< ");

                Dictionary<int, UserRank> tempMapUser = new Dictionary<int, UserRank>(mapUser);
               // int nArrests = 1000;
                int nCount = 0;
                foreach (var data in tempMapUser)
                {
                    nCount++;
                    //if (nCount > nArrests)
                    //    break;

                    string strRatioFlag = data.Value.strRatioFlag;
                    int nRatioUnderDays = 0;
                    nRatioUnderDays = Regex.Matches(strRatioFlag, "[0]").Count;

                    string strProfitFlag = data.Value.strProfitFlag;
                    int nDurationDays = 0;
                    for (int i = strProfitFlag.Length; i > 0; i--)
                    {
                        bool bMatch = Regex.IsMatch(strProfitFlag, "[1]{" + i + "}");
                        if (bMatch)
                        {
                            nDurationDays = i;
                            break;
                        }
                    }
                    UserRank u = new UserRank();
                    u.Initialize();
                    u = mapUser[data.Key];
                    u.RatioUnderDays = nRatioUnderDays; //更新低于持仓标准的天数
                    u.DurationDays = nDurationDays; //更新持续盈利天数
                    mapUser[data.Key] = u;
                    Loger.Debug(@"D:\Services\Stock Trading UserRank #3\log\durationDayRank"+CurrDay.ToString("yyMMdd")+".log",
                        "userId = " + u.UserId
                        + "\t\t" + "R = " + strRatioFlag + "\t\t" + "R-Days = " + nRatioUnderDays
                        + "\t\t" + "P = " + strProfitFlag + "\t\t" + "P+Day = " + u.DurationDays);
                }
                Loger.Debug("<<< 持仓不合格天数及持续盈利天数名次计算结束 UserCount = " + tempMapUser.Count + " >>>");
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                if (sqlReader != null && !sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        #endregion

        #region 当日排名及收益率历史对比排名

        private bool DailyRank()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始计算名次 <<< ");

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbCalcDay + "' AND  type = 'U'", sqlConn);
                if (sqlCmd.ExecuteScalar() == null)
                {
                    Loger.Debug("---  警告 : 当日排名数据未生成，请先获取排名数据！[" + strTbCalcDay + "]---");
                    return false;
                }

                Dictionary<int, UserRank> mapUserRank = new Dictionary<int, UserRank>();
                Dictionary<int, int> AreaRankId = new Dictionary<int, int>();

                int nUser = 0;
                int nRank = 0;
                //AND (a.ratiounderdays + b.ratiounderdays) <2 
                string strSelectSql = @"SELECT * FROM emtradeplay." + strTbCalcDay + " ORDER by wealth desc ";
                sqlCmd = new SqlCommand(strSelectSql, sqlConn);
                sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    int nUserId = Convert.ToInt32(sqlReader["UserId"]);
                    if (!mapUser.ContainsKey(nUserId))
                        continue;

                    if (mapUser[nUserId].RatioUnderDays >= BaseConfig.RatioCheckDay)
                    {
                        continue;
                    }
                    nUser++;
                    nRank++;
                    UserRank data = new UserRank();
                    data.Initialize();
                    data.UserId = (int)sqlReader["UserID"];
                    data.AreaId = (int)sqlReader["AreaId"];
                    data.RankId = nRank;

                    #region 计算区域排名
                    if (!AreaRankId.ContainsKey(data.AreaId))
                        AreaRankId[data.AreaId] = 1;
                    else
                        AreaRankId[data.AreaId]++;

                    data.AreaRankId = AreaRankId[data.AreaId];
                    #endregion

                    mapUserRank[data.UserId] = data;
                }
                sqlReader.Close();
                Loger.Debug("<<< 区域排名结束 >>>");

                sqlCmd = new SqlCommand("UPDATE emtradeplay." + strTbCalcDay + " SET RankID=null, AreaRankId=null, RankChanged = null, AreaRankChanged = null, Title = '无效排名用户'  ", sqlConn);
                sqlCmd.ExecuteNonQuery();
                foreach (var data in mapUserRank)
                {
                    int nUserId=data.Value.UserId;
                    if (!mapUser.ContainsKey(nUserId))
                    {
                        continue;
                    }
                    sqlCmd = new SqlCommand("UPDATE emtradeplay." + strTbCalcDay
                        + " SET RankID=@RankID, AreaRankId=@AreaRankId, DurationDays=@DurationDays,  Title = '' WHERE UserId=@UserID", sqlConn);
                    sqlCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = data.Value.UserId;
                    sqlCmd.Parameters.Add("@RankID", SqlDbType.Int).Value = data.Value.RankId;
                    sqlCmd.Parameters.Add("@AreaRankId", SqlDbType.Int).Value = data.Value.AreaRankId;
                    sqlCmd.Parameters.Add("@DurationDays", SqlDbType.Int).Value = mapUser[nUserId].DurationDays;
                    //sqlCmd.Parameters.Add("@DurationRank", SqlDbType.Int).Value = data.Value.DurationRank;
                    sqlCmd.ExecuteNonQuery();
                }

                nUser = 0;
                nRank = 0;
                strSelectSql = @"SELECT * FROM emtradeplay." + strTbCalcDay + " ORDER by DurationDays desc, wealth desc ";
                sqlCmd = new SqlCommand(strSelectSql, sqlConn);
                sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    int nUserId = Convert.ToInt32(sqlReader["UserId"]);
                    if (!mapUser.ContainsKey(nUserId))
                        continue;

                    nUser++;
                    nRank++;
                    UserRank data = new UserRank();
                    data = mapUser[nUserId];
                    data.DurationDays = mapUser[nUserId].DurationDays;
                    data.DurationRank = nRank;
                    mapUserRank[data.UserId] = data;
                    Loger.Debug(@"D:\Services\Stock Trading UserRank #3\log\DRank" + CurrDay.ToString("yyMMdd") + ".log",
    "userId = " + nUserId
    + "\t\t" + " Days = " + data.DurationDays + "\t sqlDays = " + Convert.ToInt32(sqlReader["DurationDays"])
    + "\t\t" + "RankId = " + data.DurationRank);
                }
                sqlReader.Close();

                foreach (var data in mapUserRank)
                {
                    int nUserId = data.Value.UserId;
                    if (!mapUser.ContainsKey(nUserId))
                    {
                        continue;
                    }
                    sqlCmd = new SqlCommand("UPDATE emtradeplay." + strTbCalcDay
                        + " SET DurationRank=@DurationRank WHERE UserId=@UserID", sqlConn);
                    sqlCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = data.Value.UserId;
                    sqlCmd.Parameters.Add("@DurationRank", SqlDbType.Int).Value = data.Value.DurationRank;
                    sqlCmd.ExecuteNonQuery();
                }

                Loger.Debug("<<< 连续正收益排名结束 >>>");

                Loger.Debug("<<< 名次计算结束 UserCount = " + nUser + " >>>");
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                if (sqlReader != null && !sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        private bool DailyProfit()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始计算名次变化及日收益 <<< ");

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                DateTime BaseDayQuoteDay;
                DateTime CalcDay;
                BaseDayQuoteDay = CurrDay.AddDays(-1);
                while (!DaysUtils.IsQuoteDate(BaseDayQuoteDay))
                {
                    BaseDayQuoteDay = BaseDayQuoteDay.AddDays(-1);
                }
                CalcDay = CurrDay;
                string strTbBaseLastDay = "HistoryRank_" + BaseDayQuoteDay.ToString("yyMMdd");
                Loger.Debug(strTbBaseLastDay + " <=========> " + strTbCalcDay);

                sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbBaseLastDay + "' AND  type = 'U'", sqlConn);
                if (sqlCmd.ExecuteScalar() == null)
                {
                    Loger.Debug("--- 警告 : 第一天没有排名历史数据！ [" + strTbBaseLastDay + "]---");
                    //日收益率
                    sqlCmd = new SqlCommand(@"update emtradeplay." + strTbCalcDay + @"
                                                        set dailyprofit = profit ");
                    sqlCmd.Connection = sqlConn;
                    sqlCmd.ExecuteNonQuery();
                    Loger.Debug("<<< 日收益率计算结束 >>> ");
                    return false;
                }

                //总名次变化
                sqlCmd = new SqlCommand(@"update emtradeplay." + strTbCalcDay + @"
                                                        set rankchanged = a.rankid - b.rankid
                                                        from emtradeplay." + strTbBaseLastDay + @" a,emtradeplay." + strTbCalcDay + @" b
                                                        where a.userid=b.userid");
                sqlCmd.Connection = sqlConn;
                sqlCmd.ExecuteNonQuery();
                Loger.Debug("<<< 总名次变化计算结束 >>> ");
                //区域名次变化
                sqlCmd = new SqlCommand(@"update emtradeplay." + strTbCalcDay + @"
                                                        set arearankchanged = a.arearankid - b.arearankid
                                                        from emtradeplay." + strTbBaseLastDay + @" a,emtradeplay." + strTbCalcDay + @" b
                                                        where a.userid=b.userid");
                sqlCmd.Connection = sqlConn;
                sqlCmd.ExecuteNonQuery();
                Loger.Debug("<<< 区域名次变化计算结束 >>> ");

                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                if (sqlReader != null && !sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        private bool WeeklyProfit()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始计算周收益 <<< ");

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                DateTime RegValidityDay;
                DateTime BaseWeekDayQuoteDay;
                DateTime CalcWeekDay;
                if(BaseConfig.WeeklyProfitFlag == 0) //固定每周出周收益报表
                {
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Friday
                        && (DateTime.Now.TimeOfDay - (new TimeSpan(15, 30, 0))) > (new TimeSpan(0, 0, 0))
                        || DateTime.Now.DayOfWeek == DayOfWeek.Saturday
                        || DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                        )
                    {
                        int nday = Convert.ToInt32(CurrDay.DayOfWeek);
                        BaseWeekDayQuoteDay = CurrDay.AddDays(-nday - 2);//上周五
                        CalcWeekDay = BaseWeekDayQuoteDay.AddDays(7);
                    }
                    else
                    {
                        int nday = Convert.ToInt32(CurrDay.DayOfWeek);
                        BaseWeekDayQuoteDay = CurrDay.AddDays(-nday - 9);//上上周五
                        CalcWeekDay = BaseWeekDayQuoteDay.AddDays(7);
                    }
                }
                else //每天出周收益报表
                {
                    if (DateTime.Now.DayOfWeek != DayOfWeek.Saturday
                        && DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
                    {
                        int nday = Convert.ToInt32(CurrDay.DayOfWeek);
                        BaseWeekDayQuoteDay = CurrDay.AddDays(-nday - 2);//上周五
                        CalcWeekDay = CurrDay;
                     }
                    else
                    {
                        int nday = Convert.ToInt32(CurrDay.DayOfWeek);
                        BaseWeekDayQuoteDay = CurrDay.AddDays(-nday - 9);//上上周五
                        CalcWeekDay = BaseWeekDayQuoteDay.AddDays(7);
                    }
                }
                //BaseWeekDayQuoteDay = DateTime.Parse("2009-09-25");
                RegValidityDay = BaseWeekDayQuoteDay.AddDays(3);

                while (!DaysUtils.IsQuoteDate(BaseWeekDayQuoteDay))
                {
                    BaseWeekDayQuoteDay = BaseWeekDayQuoteDay.AddDays(-1);
                }
                while (!DaysUtils.IsQuoteDate(CalcWeekDay))
                {
                    CalcWeekDay = CalcWeekDay.AddDays(-1);
                }
                while (!DaysUtils.IsQuoteDate(RegValidityDay))
                {
                    RegValidityDay = RegValidityDay.AddDays(-1);
                }
                string strTbBaseWeekDay = "HistoryRank_" + BaseWeekDayQuoteDay.ToString("yyMMdd");
                string strTbCalcWeekDay = "HistoryRank_" + CalcWeekDay.ToString("yyMMdd");
                string strTbRegValidityDay = "HistoryRank_" + RegValidityDay.ToString("yyyyMMdd");
                Loger.Debug(strTbBaseWeekDay + " <=========> " + strTbCalcWeekDay);

                sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbRegValidityDay + "' AND  type = 'U'", sqlConn);
                if (sqlCmd.ExecuteScalar() != null) //获取本周注册用户名单
                {
                    sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbBaseWeekDay + "' AND  type = 'U'", sqlConn);
                    if (sqlCmd.ExecuteScalar() != null) //有周收益基准表
                    {
                        sqlCmd = new SqlCommand(@"select * from emtradeplay." + strTbRegValidityDay + @"
                                                        set weeklyprofit = (a.wealth - b.wealth )/b.wealth * 100
                                                        from emtradeplay." + strTbCalcWeekDay + " a,emtradeplay." + strTbBaseWeekDay + @" b  
                                                        where a.userid=b.userid  ");
                    }
                }

                sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbBaseWeekDay + "' AND  type = 'U'", sqlConn);
                if (sqlCmd.ExecuteScalar() != null) //有周收益基准表
                {
                    sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbCalcWeekDay + "' AND  type = 'U'", sqlConn);
                    if (sqlCmd.ExecuteScalar() != null) //有周收益计算表
                    {
                        sqlCmd = new SqlCommand(@"update emtradeplay." + strTbCalcDay + @"
                                                        set weeklyprofit = (a.wealth - b.wealth )/b.wealth * 100
                                                        from emtradeplay." + strTbCalcWeekDay + " a,emtradeplay." + strTbBaseWeekDay + @" b  
                                                        where a.userid=b.userid  ");
                        sqlCmd.Connection = sqlConn;
                        sqlCmd.ExecuteNonQuery();
                        Loger.Debug("<<< 周收益计算结束 >>> ");
                    }
                    else
                    {
                        Loger.Debug("--- 异常 : 未找到周收益计算表 --- ");
                    }
                }
                else //无周收益基准表
                {
                    sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbCalcWeekDay + "' AND  type = 'U'", sqlConn);
                    if (sqlCmd.ExecuteScalar() != null) //有周收益计算表
                    {
                        Loger.Debug("--- 警告 : 无周收益基准表，采用初始值计算！---");
                        sqlCmd = new SqlCommand(@"update emtradeplay." + strTbCalcDay + @"
                                                        set weeklyprofit = (wealth - "+BaseConfig.InitCashRMB+")/"+BaseConfig.InitCashRMB+@" * 100
                                                        ");
                        sqlCmd.Connection = sqlConn;
                        sqlCmd.ExecuteNonQuery();
                        Loger.Debug("<<< 初始周收益计算结束 >>> ");
                    }
                    else
                    {
                        Loger.Debug("--- 警告 : 第一周未到周五收盘，无周收益 --- ");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                if (sqlReader != null && !sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        private bool MonthlyProfit()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始计算月收益 <<< ");

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                DateTime BaseMonthDayQuoteDay;
                DateTime CalcMonthDay;
                DateTime lastDayOfMonth = DaysUtils.LastDayOfMonth(CurrDay); //本月最后一天
                while (!DaysUtils.IsQuoteDate(lastDayOfMonth))
                {
                    lastDayOfMonth = lastDayOfMonth.AddDays(-1); //修正为本月最后一个行情日
                }
                if (CurrDay == lastDayOfMonth
                    && (DateTime.Now.TimeOfDay - (new TimeSpan(15, 30, 0))) > (new TimeSpan(0, 0, 0)))
                {
                    BaseMonthDayQuoteDay = DaysUtils.LastDayOfPrdviousMonth(CurrDay); //上月最后一天
                    CalcMonthDay = CurrDay;
                }
                else
                {
                    BaseMonthDayQuoteDay = DaysUtils.LastDayOfPrdviousMonth(DaysUtils.LastDayOfPrdviousMonth(CurrDay)); //上上月最后一天
                    CalcMonthDay = DaysUtils.LastDayOfMonth(BaseMonthDayQuoteDay);
                }
                while (!DaysUtils.IsQuoteDate(BaseMonthDayQuoteDay))
                {
                    BaseMonthDayQuoteDay = BaseMonthDayQuoteDay.AddDays(-1);
                }
                while (!DaysUtils.IsQuoteDate(CalcMonthDay))
                {
                    CalcMonthDay = CalcMonthDay.AddDays(-1);
                }

                string strTbBaseMonthDay = "HistoryRank_" + BaseMonthDayQuoteDay.ToString("yyMMdd");
                string strTbCalcMonthDay = "HistoryRank_" + CalcMonthDay.ToString("yyMMdd");
                Loger.Debug(strTbBaseMonthDay + " <=========> " + strTbCalcMonthDay);

                sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbBaseMonthDay + "' AND  type = 'U'", sqlConn);
                if (sqlCmd.ExecuteScalar() != null)
                {
                    sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbCalcMonthDay + "' AND  type = 'U'", sqlConn);
                    if (sqlCmd.ExecuteScalar() != null)
                    {
                        sqlCmd = new SqlCommand(@"update emtradeplay." + strTbCalcDay + @"
                                                        set monthlyprofit = (a.wealth - b.wealth )/b.wealth * 100
                                                        from emtradeplay." + strTbCalcMonthDay + " a,emtradeplay." + strTbBaseMonthDay + @" b, emtradeplay." + strTbCalcDay + @" c
                                                        where a.userid=b.userid and a.userid = c.userid ");
                        sqlCmd.Connection = sqlConn;
                        sqlCmd.ExecuteNonQuery();
                        Loger.Debug("<<< 月收益计算结束 >>> ");
                    }
                    else
                    {
                        Loger.Debug("--- 异常 : 未找到月收益计算表 --- ");
                    }
                }
                else
                {
                    sqlCmd = new SqlCommand("SELECT name FROM sysobjects WHERE  name = '" + strTbCalcMonthDay + "' AND  type = 'U'", sqlConn);
                    if (sqlCmd.ExecuteScalar() != null)
                    {
                        Loger.Debug("--- 无月收益基准表，采用初始值计算 [" + strTbBaseMonthDay + "]---");
                        sqlCmd = new SqlCommand(@"update emtradeplay." + strTbCalcDay + @"
                                                        set monthlyprofit = (wealth - "+BaseConfig.InitCashRMB+")/"+BaseConfig.InitCashRMB+@" * 100
                                                        ");
                        sqlCmd.Connection = sqlConn;
                        sqlCmd.ExecuteNonQuery();
                        Loger.Debug("<<< 月收益计算结束 >>> ");
                    }
                    else
                    {
                        Loger.Debug("--- 警告 : 第一月未到月底收盘，无月收益 --- ");
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(ReplaceSqlPara(sqlCmd));
                Loger.Debug(err);
                if (sqlReader != null && !sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        #endregion

        #region 更新最终发布数据

        private bool UpdateDailyRank()
        {
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始更新当日排名表 <<< ");

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                sqlCmd = new SqlCommand(@"delete from dailyrank 
                                                        insert dailyrank 
                                                          select * from emtradeplay." + strTbCalcDay + " where rankid > 0 order by wealth desc ");
                sqlCmd.Connection = sqlConn;
                sqlCmd.ExecuteNonQuery();
                Loger.Debug("<<< 当日排名表更新结束 >>> ");
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                if (sqlReader != null && !sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        private bool ResponseHandler(int playID)
        {
            try
            {
                ccpostWS.RemotingRespond responseWS = new ccpostWS.RemotingRespond();
                responseWS.Url = BaseConfig.RankNotifySrv;
                responseWS.Ranks_Handled(playID);
                Loger.Debug("<<< 通知完成 >>>");
                return true;
            }
            catch (Exception ex)
            {
                Loger.Debug("<<< 通知失败 >>>");
                Loger.Debug(ex);
                return false;
            }

        }

        #endregion

        #region 其它
        private double ConvertPrice(double Price)
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

        private string ReplaceSqlPara(SqlCommand cmd)
        {
            if (cmd == null)
                return "SqlCommand null";
            String cmdText = cmd.CommandText;
            SqlParameterCollection paras = cmd.Parameters;
            if (paras != null)
            {
                foreach (SqlParameter p in paras)
                {
                    switch (p.SqlDbType)
                    {
                        case SqlDbType.Char:
                        case SqlDbType.Date:
                        case SqlDbType.DateTime:
                        case SqlDbType.SmallDateTime:
                        case SqlDbType.NChar:
                        case SqlDbType.NText:
                        case SqlDbType.NVarChar:
                        case SqlDbType.Text:
                        case SqlDbType.Time:
                        case SqlDbType.VarChar:
                            cmdText = cmdText.Replace(p.ParameterName, "'" + p.Value.ToString() + "'");
                            break;
                        default:
                            cmdText = cmdText.Replace(p.ParameterName, p.Value.ToString());
                            break;
                    }

                }
            }
            return cmdText;
        }

        private bool IsDBFTransSuccess()
        {
            System.IO.FileInfo fileSH = new FileInfo(BaseConfig.DBFSHPath);
            System.IO.FileInfo fileSZ=new FileInfo(BaseConfig.DBFSZPath);
            if (fileSH.LastWriteTime.Date != DateTime.Now.Date)
                return false;
            if(fileSZ.LastWriteTime.Date != DateTime.Now.Date)
                return false;

            if (fileSH.LastWriteTime.TimeOfDay > new TimeSpan(15, 0, 0)
                && fileSZ.LastWriteTime.TimeOfDay > new TimeSpan(15, 0, 0))
            {
                Loger.Debug(" --- 行情源时间检查： ---");
                Loger.Debug("dbf sh : " + fileSH.LastWriteTime);
                Loger.Debug("dbf sz : " + fileSZ.LastWriteTime);
                return true;
            }
            return false;
        }

        #endregion

    }
}
