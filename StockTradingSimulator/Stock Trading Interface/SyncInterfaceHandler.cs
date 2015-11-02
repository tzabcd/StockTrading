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
        /// 添加用户资金到 listNewUserFund
        /// </summary>
        /// <param name="Fund"></param>
        /// <returns></returns>
        public bool AddNewUserFund(TradingSystem.UserFund Fund)
        {
            try
            {
                if (Fund.UserID <= 0)
                    return false;
                else if (Fund.Cash < 0.01 || Fund.UsableCash < 0.01)
                    return false;
                else if (Common.ComparePrice(Fund.Cash, Fund.UsableCash) != 0)
                    return false;

                if (Common.stkBuffer != null)
                {
                    RemotingInterface.RI_Fund stiFund = new RemotingInterface.RI_Fund();
                    stiFund.Clear();
                    stiFund.Cash = Fund.Cash;
                    stiFund.UsableCash = Fund.UsableCash;
                    stiFund.Wealth = Common.ConvertPrice(Fund.Wealth);
                    stiFund.Curr = (RemotingInterface.RI_Currency)Fund.Curr;
                    Common.stkBuffer.SetUserFund(Fund.UserID, stiFund);
                }
                lock (listNewUserFund)
                {
                    listNewUserFund.Add(Fund);
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
        /// 除权
        /// </summary>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <param name="Quotiety"></param>
        /// <returns></returns>
        public RemotingInterface.RI_Result SetExRights(string StockCode, TradingSystem.StockMarket Market, double Quotiety)
        {
            bool bConn = false;
            try
            {
                if (StockCode == null || Market == TradingSystem.StockMarket.Unknown)
                    return RemotingInterface.RI_Result.Banned_Stock;
                else if (Quotiety <= 1)
                    return RemotingInterface.RI_Result.Illegal_Quotiety;
                else if (!Common.IsWeekend
                    && DateTime.Now.TimeOfDay >= Common.BeginAMTS
                    && DateTime.Now.TimeOfDay <= Common.EndPMTS.Add(new TimeSpan(0, 15, 0)))
                    return RemotingInterface.RI_Result.Out_Of_Maintain_Time;
                bConn = true;

                // 盘后方可除权
                if (sqlConn_Adm.State == ConnectionState.Closed)
                    sqlConn_Adm.Open();
                sqlTrans_Adm = sqlConn_Adm.BeginTransaction();

                List<TradingSystem.UserStocks> listStocks = new List<TradingSystem.UserStocks>();
                List<TradingHistory> listTrades = new List<TradingHistory>();
                sqlCmd_Adm = new SqlCommand("SELECT * FROM [UserStocks] WHERE (StockCode = @StockCode) AND (Market = @Market", sqlConn_Adm, sqlTrans_Adm);
                sqlCmd_Adm.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Adm.Parameters["@StockCode"].Value = StockCode.Trim();
                sqlCmd_Adm.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Market"].Value = (byte)Market;
                sqlReader_Adm = sqlCmd_Adm.ExecuteReader();
                while (sqlReader_Adm.Read())
                {
                    TradingSystem.UserStocks data_stock = new TradingSystem.UserStocks();
                    TradingHistory data_trade = new TradingHistory();
                    data_stock.Initialize(); data_trade.Initialize();
                    data_stock.UserID = (int)sqlReader_Adm["UserID"]; data_trade.UserID = data_stock.UserID;
                    data_stock.StockCode = sqlReader_Adm["StockCode"].ToString().Trim(); data_trade.StockCode = data_stock.StockCode;
                    data_stock.Market = (TradingSystem.StockMarket)Convert.ToByte(sqlReader_Adm["Market"].ToString().Trim()); data_trade.Market = data_stock.Market;
                    data_stock.Volume = (int)sqlReader_Adm["Volume"]; data_trade.TradeVolume = (int)(((double)data_stock.Volume * (Quotiety - 1) + 50) / 100) * 100;
                    if (data_trade.TradeVolume < 100)
                        continue;
                    data_stock.AveragePrice = Common.ConvertPrice(
                        (double.Parse(sqlReader_Adm["AveragePrice"].ToString().Trim()) * data_stock.Volume)
                        / (data_stock.Volume + data_trade.TradeVolume));
                    data_stock.Volume += data_trade.TradeVolume;
                    data_stock.Curr = (TradingSystem.Currency)Convert.ToByte(sqlReader_Adm["Currency"].ToString().Trim()); data_trade.Curr = data_stock.Curr;
                    if (sqlReader_Adm["Sellable"].ToString().Trim() == "1")
                        data_stock.Sellable = true;
                    else
                        data_stock.Sellable = false;
                    data_trade.OrderID = (int)Special_OrderID.ExRights;
                    data_trade.Side = true;
                    data_trade.TradeDate = DateTime.Now;
                    data_trade.TradePrice = 0;
                    listStocks.Add(data_stock);
                    listTrades.Add(data_trade);
                }
                sqlReader_Adm.Close();

                foreach (TradingSystem.UserStocks data in listStocks)
                {
                    sqlCmd_Adm = new SqlCommand("UPDATE [UserStocks] SET Volume = @Volume, AveragePrice = @AveragePrice " +
                        "WHERE (UserID = @UserID) AND (StockCode = @StockCode) AND (Market = @Market) AND (Sellable = @Sellable)", sqlConn_Adm, sqlTrans_Adm);
                    sqlCmd_Adm.Parameters.Add("@Volume", SqlDbType.Int); sqlCmd_Adm.Parameters["@Volume"].Value = data.Volume;
                    sqlCmd_Adm.Parameters.Add("@AveragePrice", SqlDbType.Money); sqlCmd_Adm.Parameters["@AveragePrice"].Value = data.AveragePrice;
                    sqlCmd_Adm.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Adm.Parameters["@UserID"].Value = data.UserID;
                    sqlCmd_Adm.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Adm.Parameters["@StockCode"].Value = data.StockCode.Trim();
                    sqlCmd_Adm.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Market"].Value = (byte)data.Market;
                    sqlCmd_Adm.Parameters.Add("@Sellable", SqlDbType.TinyInt);
                    if (data.Sellable)
                        sqlCmd_Adm.Parameters["@Sellable"].Value = (byte)1;
                    else
                        sqlCmd_Adm.Parameters["@Sellable"].Value = (byte)0;
                    sqlCmd_Adm.ExecuteNonQuery();
                }

                foreach (TradingHistory data in listTrades)
                {
                    sqlCmd_Adm = new SqlCommand("INSERT INTO [TradingHistory] (OrderID,UserID," +
                        "OrderSide,StockCode,Market,TradeVolume,TradePrice,Currency,TradeDate) " +
                        "VALUES (@OrderID,@UserID,@OrderSide,@StockCode,@Market," +
                        "@TradeVolume,@TradePrice,@Currency,@TradeDate)", sqlConn_Adm, sqlTrans_Adm);
                    sqlCmd_Adm.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Adm.Parameters["@OrderID"].Value = data.OrderID;
                    sqlCmd_Adm.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Adm.Parameters["@UserID"].Value = data.UserID;
                    if (data.Side)
                    {
                        sqlCmd_Adm.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Adm.Parameters["@OrderSide"].Value = (byte)1;
                    }
                    else
                    {
                        sqlCmd_Adm.Parameters.Add("@OrderSide", SqlDbType.Bit); sqlCmd_Adm.Parameters["@OrderSide"].Value = (byte)0;
                    }
                    sqlCmd_Adm.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Adm.Parameters["@StockCode"].Value = data.StockCode.Trim();
                    sqlCmd_Adm.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Market"].Value = (byte)data.Market;
                    sqlCmd_Adm.Parameters.Add("@TradeVolume", SqlDbType.Int); sqlCmd_Adm.Parameters["@TradeVolume"].Value = data.TradeVolume;
                    sqlCmd_Adm.Parameters.Add("@TradePrice", SqlDbType.Money); sqlCmd_Adm.Parameters["@TradePrice"].Value = data.TradePrice;
                    sqlCmd_Adm.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Currency"].Value = (byte)data.Curr;
                    sqlCmd_Adm.Parameters.Add("@TradeDate", SqlDbType.DateTime); sqlCmd_Adm.Parameters["@TradeDate"].Value = data.TradeDate;
                    sqlCmd_Adm.ExecuteNonQuery();
                }

                sqlTrans_Adm.Commit();
                return RemotingInterface.RI_Result.Success;
            }
            catch (Exception err)
            {
                Common.DBLog(err, ReplaceSqlPara(sqlCmd_Adm), false);
                Common.Log(err);
                if (sqlReader_Adm != null && !sqlReader_Adm.IsClosed)
                    sqlReader_Adm.Close();
                if (sqlTrans_Adm != null && sqlTrans_Adm.Connection != null && sqlTrans_Adm.Connection.State == ConnectionState.Open)
                    sqlTrans_Adm.Rollback();
                return RemotingInterface.RI_Result.Internal_Error;
            }
            finally
            {
                if (bConn && sqlConn_Adm.State != ConnectionState.Closed)
                    sqlConn_Adm.Close();
            }
        }

        /// <summary>
        /// 分红
        /// </summary>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <param name="Quotiety"></param>
        /// <returns></returns>
        public RemotingInterface.RI_Result SetBonus(string StockCode, TradingSystem.StockMarket Market, double Quotiety)
        {
            bool bConn = false;
            try
            {
                if (StockCode == null || Market == TradingSystem.StockMarket.Unknown)
                    return RemotingInterface.RI_Result.Banned_Stock;
                else if (Quotiety <= 1)
                    return RemotingInterface.RI_Result.Illegal_Quotiety;
                else if (!Common.IsWeekend
                    && DateTime.Now.TimeOfDay >= Common.BeginAMTS
                    && DateTime.Now.TimeOfDay <= Common.EndPMTS.Add(new TimeSpan(0, 15, 0)))
                    return RemotingInterface.RI_Result.Out_Of_Maintain_Time;
                bConn = true;

                // 盘后方可分红
                if (sqlConn_Adm.State == ConnectionState.Closed)
                    sqlConn_Adm.Open();
                sqlTrans_Adm = sqlConn_Adm.BeginTransaction();

                List<FundHistory> listFunds = new List<FundHistory>();
                sqlCmd_Adm = new SqlCommand("SELECT * FROM [UserStocks] WHERE (StockCode = @StockCode) AND (Market = @Market", sqlConn_Adm, sqlTrans_Adm);
                sqlCmd_Adm.Parameters.Add("@StockCode", SqlDbType.NVarChar, 6); sqlCmd_Adm.Parameters["@StockCode"].Value = StockCode.Trim();
                sqlCmd_Adm.Parameters.Add("@Market", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Market"].Value = (byte)Market;
                sqlReader_Adm = sqlCmd_Adm.ExecuteReader();
                while (sqlReader_Adm.Read())
                {
                    FundHistory data = new FundHistory();
                    data.Initialize();
                    data.ChangedCash = (double)((int)sqlReader_Adm["Volume"]) * Quotiety;
                    data.ChangedTime = DateTime.Now;
                    data.Curr = (TradingSystem.Currency)Convert.ToByte(sqlReader_Adm["Currency"].ToString().Trim());
                    data.OrderID = (int)Special_OrderID.Bonus;
                    data.UserID = (int)sqlReader_Adm["UserID"];
                    listFunds.Add(data);
                }
                sqlReader_Adm.Close();

                for (int i = 0; i < listFunds.Count; i++)
                {
                    sqlCmd_Adm = new SqlCommand("SELECT Cash FROM [UserFund] WHERE (UserID = @UserID) AND (Currency = @Currency)", sqlConn_Adm, sqlTrans_Adm);
                    sqlCmd_Adm.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Adm.Parameters["@UserID"].Value = listFunds[i].UserID;
                    sqlCmd_Adm.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Currency"].Value = (byte)listFunds[i].Curr;
                    sqlReader_Adm = sqlCmd_Adm.ExecuteReader();
                    if (sqlReader_Adm.Read())
                    {
                        FundHistory data = listFunds[i];
                        data.OriginalCash = Convert.ToDouble(sqlReader_Adm["Cash"].ToString().Trim());
                        listFunds[i] = data;
                    }
                    sqlReader_Adm.Close();

                    sqlCmd_Adm = new SqlCommand("UPDATE [UserFund] SET Cash = Cash + @Add WHERE " +
                        "(UserID = @UserID) AND (Currency = @Currency)", sqlConn_Adm, sqlTrans_Adm);
                    sqlCmd_Adm.Parameters.Add("@Add", SqlDbType.Money); sqlCmd_Adm.Parameters["@Add"].Value = listFunds[i].ChangedCash;
                    sqlCmd_Adm.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Adm.Parameters["@UserID"].Value = listFunds[i].UserID;
                    sqlCmd_Adm.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Currency"].Value = (byte)listFunds[i].Curr;
                    sqlCmd_Adm.ExecuteNonQuery();

                    sqlCmd_Adm = new SqlCommand("INSERT INTO [FundHistory] (UserID,OriginalCash,ChangedCash,Currency,ChangedTime,OrderID) " +
                        "VALUES (@UserID,@OriginalCash,@ChangedCash,@Currency,@ChangedTime,@OrderID)", sqlConn_Adm, sqlTrans_Adm);
                    sqlCmd_Adm.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Adm.Parameters["@UserID"].Value = listFunds[i].UserID;
                    sqlCmd_Adm.Parameters.Add("@OriginalCash", SqlDbType.Money); sqlCmd_Adm.Parameters["@OriginalCash"].Value = listFunds[i].OriginalCash;
                    sqlCmd_Adm.Parameters.Add("@ChangedCash", SqlDbType.Money); sqlCmd_Adm.Parameters["@ChangedCash"].Value = listFunds[i].ChangedCash;
                    sqlCmd_Adm.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Currency"].Value = (byte)listFunds[i].Curr;
                    sqlCmd_Adm.Parameters.Add("@ChangedTime", SqlDbType.DateTime); sqlCmd_Adm.Parameters["@ChangedTime"].Value = listFunds[i].ChangedTime.ToString("yyyy-MM-dd HH:mm:ss");
                    sqlCmd_Adm.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Adm.Parameters["@OrderID"].Value = listFunds[i].OrderID;
                    sqlCmd_Adm.ExecuteNonQuery();
                }

                sqlCmd_Adm = new SqlCommand("UPDATE [UserFund] SET UsableCash = Cash", sqlConn_Adm, sqlTrans_Adm);
                sqlCmd_Adm.ExecuteNonQuery();

                sqlTrans_Adm.Commit();
                return RemotingInterface.RI_Result.Success;
            }
            catch (Exception err)
            {
                Common.DBLog(err, ReplaceSqlPara(sqlCmd_Adm), false);
                Common.Log(err);
                if (sqlReader_Adm != null && !sqlReader_Adm.IsClosed)
                    sqlReader_Adm.Close();
                if (sqlTrans_Adm != null && sqlTrans_Adm.Connection != null && sqlTrans_Adm.Connection.State == ConnectionState.Open)
                    sqlTrans_Adm.Rollback();
                return RemotingInterface.RI_Result.Internal_Error;
            }
            finally
            {
                if (bConn && sqlConn_Adm.State != ConnectionState.Closed)
                    sqlConn_Adm.Close();
            }
        }

        /// <summary>
        /// 扣除现金
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="Curr"></param>
        /// <param name="Quotiety"></param>
        /// <returns></returns>
        public RemotingInterface.RI_Result SetForfeiture(int UserID, TradingSystem.Currency Curr, double Quotiety)
        {
            bool bConn = false;
            try
            {
                if (UserID <= 0)
                    return RemotingInterface.RI_Result.Illegal_UserID;
                else if (Curr == TradingSystem.Currency.Unknown)
                    return RemotingInterface.RI_Result.Illegal_Currency;
                else if (Quotiety > 1 || Quotiety <= 0)
                    return RemotingInterface.RI_Result.Illegal_Quotiety;
                else if (!Common.IsWeekend
                    && DateTime.Now.TimeOfDay >= Common.BeginAMTS
                    && DateTime.Now.TimeOfDay <= Common.EndPMTS.Add(new TimeSpan(0, 15, 0)))
                    return RemotingInterface.RI_Result.Out_Of_Maintain_Time;
                bConn = true;

                // 盘后方可扣除现金
                if (sqlConn_Adm.State == ConnectionState.Closed)
                    sqlConn_Adm.Open();
                sqlTrans_Adm = sqlConn_Adm.BeginTransaction();

                List<FundHistory> listFunds = new List<FundHistory>();
                sqlCmd_Adm = new SqlCommand("SELECT Cash FROM [UserFund] WHERE (UserID = @UserID) AND (Currency = @Currency)", sqlConn_Adm, sqlTrans_Adm);
                sqlCmd_Adm.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Adm.Parameters["@UserID"].Value = UserID;
                sqlCmd_Adm.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Currency"].Value = (byte)Curr;
                sqlReader_Adm = sqlCmd_Adm.ExecuteReader();
                if (sqlReader_Adm.Read())
                {
                    double dOriginalCash = Convert.ToDouble(sqlReader_Adm["Cash"].ToString().Trim());
                    double dCash = dOriginalCash * (1 - Quotiety);
                    sqlReader_Adm.Close();

                    sqlCmd_Adm = new SqlCommand("UPDATE [UserFund] SET Cash = @Cash, UsableCash = @UsableCash WHERE (UserID = @UserID) AND (Currency = @Currency)", sqlConn_Adm, sqlTrans_Adm);
                    sqlCmd_Adm.Parameters.Add("@Cash", SqlDbType.Money); sqlCmd_Adm.Parameters["@Cash"].Value = dCash;
                    sqlCmd_Adm.Parameters.Add("@UsableCash", SqlDbType.Money); sqlCmd_Adm.Parameters["@UsableCash"].Value = dCash;
                    sqlCmd_Adm.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Adm.Parameters["@UserID"].Value = UserID;
                    sqlCmd_Adm.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Currency"].Value = (byte)Curr;
                    sqlCmd_Adm.ExecuteNonQuery();

                    sqlCmd_Adm = new SqlCommand("INSERT INTO [FundHistory] (UserID,OriginalCash,ChangedCash,Currency,ChangedTime,OrderID) " +
                        "VALUES (@UserID,@OriginalCash,@ChangedCash,@Currency,@ChangedTime,@OrderID)", sqlConn_Adm, sqlTrans_Adm);
                    sqlCmd_Adm.Parameters.Add("@UserID", SqlDbType.Int); sqlCmd_Adm.Parameters["@UserID"].Value = UserID;
                    sqlCmd_Adm.Parameters.Add("@OriginalCash", SqlDbType.Money); sqlCmd_Adm.Parameters["@OriginalCash"].Value = dOriginalCash;
                    sqlCmd_Adm.Parameters.Add("@ChangedCash", SqlDbType.Money); sqlCmd_Adm.Parameters["@ChangedCash"].Value = (dCash - dOriginalCash);
                    sqlCmd_Adm.Parameters.Add("@Currency", SqlDbType.TinyInt); sqlCmd_Adm.Parameters["@Currency"].Value = (byte)Curr;
                    sqlCmd_Adm.Parameters.Add("@ChangedTime", SqlDbType.DateTime); sqlCmd_Adm.Parameters["@ChangedTime"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Trim();
                    sqlCmd_Adm.Parameters.Add("@OrderID", SqlDbType.Int); sqlCmd_Adm.Parameters["@OrderID"].Value = Special_OrderID.Punishment;
                    sqlCmd_Adm.ExecuteNonQuery();
                }
                else
                {
                    sqlReader_Adm.Close();
                }

                sqlTrans_Adm.Commit();
                return RemotingInterface.RI_Result.Success;
            }
            catch (Exception err)
            {
                Common.DBLog(err, ReplaceSqlPara(sqlCmd_Adm), false);
                Common.Log(err);
                if (sqlReader_Adm != null && !sqlReader_Adm.IsClosed)
                    sqlReader_Adm.Close();
                if (sqlTrans_Adm != null && sqlTrans_Adm.Connection != null && sqlTrans_Adm.Connection.State == ConnectionState.Open)
                    sqlTrans_Adm.Rollback();
                return RemotingInterface.RI_Result.Internal_Error;
            }
            finally
            {
                if (bConn && sqlConn_Adm.State != ConnectionState.Closed)
                    sqlConn_Adm.Close();
            }
        }

    }
}
