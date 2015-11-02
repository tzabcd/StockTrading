using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using zns = EastMoney.StocksTrader.RemotingProvider;

namespace BonusSystem
{
    class Common
    {
        static void Main(string[] args)
        {
            try
            {
                SendBonus();
                SendAllotment();
            }
            catch (Exception ex)
            {
                Loger.Debug(ex.ToString());
            }
        }

        private static bool SendBonus()
        {
            string strConn = "";
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;
            SqlDataReader sqlReader = null;
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始发送股权登记公告 <<<");

                strConn = BaseConfig.ConnPlay;
                sqlConn = new SqlConnection(strConn.Trim());

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

               // string strSql = @"SELECT * FROM BonusList where BonusRegDate = @BonusRegDate";
                string strSql = @"SELECT * FROM BonusList ";
                sqlCmd = new SqlCommand(strSql, sqlConn);
                sqlCmd.Parameters.Add("@BonusRegDate", SqlDbType.SmallDateTime).Value = DateTime.Today.ToString("yyyy-MM-dd");
                sqlReader = sqlCmd.ExecuteReader();

                Dictionary<string, Bonus> mapBonus = new Dictionary<string, Bonus>();
                while (sqlReader.Read())
                {
                    int bonusId = Convert.ToInt32(sqlReader["BonusId"]);
                    string stockCode = sqlReader["StockCode"].ToString();
                    int stockMarket = Convert.ToInt32(sqlReader["stockMarket"]);

                    double sendBonus = Convert.ToDouble(sqlReader["SendBonus"]);
                    double transferBonus = Convert.ToDouble(sqlReader["TransferBonus"]);
                    double bonusIssue = Convert.ToDouble(sqlReader["BonusIssue"]);

                    DateTime bonusRegDate = Convert.ToDateTime(sqlReader["BonusRegDate"]);
                    DateTime bonusExeDate = Convert.ToDateTime(sqlReader["BonusExeDate"]);

                    int sendFlag = Convert.ToInt32(sqlReader["SendFlag"]);

                    Bonus currBonus = new Bonus();
                    currBonus.Initialize();
                    currBonus.StockCode = stockCode;
                    currBonus.Market = (StockMarket)stockMarket;
                    currBonus.SendBonus = sendBonus;
                    currBonus.TransferBonus = transferBonus;
                    currBonus.BonusIssue = bonusIssue;
                    currBonus.BonusRegDate = bonusRegDate;
                    currBonus.BonusExeDate = bonusExeDate;
                    currBonus.SendFlag = sendFlag;

                    mapBonus[stockCode] = currBonus;

                } 
                sqlReader.Close();
                Loger.Debug("mapBonus count = " + mapBonus.Count);

                zns.ITransactionRemotingProvider[] znRmtIobj = new zns.ITransactionRemotingProvider[BaseConfig.mapNotifySrv.Count];
                for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                {
                    znRmtIobj[i] = Activator.GetObject(typeof(zns.ITransactionRemotingProvider), BaseConfig.mapNotifySrv[i].ri) as zns.ITransactionRemotingProvider;
                }

                Dictionary<string, Bonus> t_mapBonus = new Dictionary<string, Bonus>(mapBonus);
                foreach (var data in t_mapBonus)
                {
                    Bonus t_Bonus = new Bonus();
                    t_Bonus = data.Value;
                    zns.Interface.RI_Result RetSetExRights = zns.Interface.RI_Result.Success;
                    for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                    {
                        znRmtIobj[i] = Activator.GetObject(typeof(zns.ITransactionRemotingProvider), BaseConfig.mapNotifySrv[i].ri) as zns.ITransactionRemotingProvider;
                        zns.ITransactionRemotingProvider rmt = znRmtIobj[i];
                        RetSetExRights = rmt.SetExRights("@!pH@2^!@8", t_Bonus.StockCode, (zns.Interface.RI_Market)t_Bonus.Market,
                             t_Bonus.SendBonus, t_Bonus.TransferBonus, t_Bonus.BonusIssue,
                             t_Bonus.BonusRegDate, t_Bonus.BonusExeDate, true);
                        Loger.Debug("StockCode = " + data.Value.StockCode + "\t" + "status = " + RetSetExRights);
                    }
                    t_Bonus.SendFlag = 1;
                    mapBonus[data.Key] = t_Bonus;
                }

                Loger.Debug("<<< 股权登记公告发送完成 >>>");
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

        private static bool SendAllotment()
        {
            string strConn = "";
            SqlConnection sqlConn = null;
            SqlCommand sqlCmd = null;
            SqlDataReader sqlReader = null;
            try
            {
                Loger.Debug("\r\n\r\n>>> 开始发送配股信息 <<<");

                strConn = BaseConfig.ConnPlay;
                sqlConn = new SqlConnection(strConn.Trim());

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

               // string strSql = @"SELECT * FROM AllotmentList where ExecuteDate > @ExecuteDate";
                string strSql = @"SELECT * FROM AllotmentList ";
                sqlCmd = new SqlCommand(strSql, sqlConn);
                sqlCmd.Parameters.Add("@ExecuteDate", SqlDbType.SmallDateTime).Value = DateTime.Today.ToString("yyyy-MM-dd");
                sqlReader = sqlCmd.ExecuteReader();

                Dictionary<string, Allotment> mapAllotment = new Dictionary<string, Allotment>();
                while (sqlReader.Read())
                {
                    int AllotmentId = Convert.ToInt32(sqlReader["AllotmentId"]);
                    string stockCode = sqlReader["StockCode"].ToString();
                    int stockMarket = Convert.ToInt32(sqlReader["stockMarket"]);

                    double Scheme = Convert.ToDouble(sqlReader["Scheme"]);
                    double Price = Convert.ToDouble(sqlReader["Price"]);

                    DateTime RecordDate = Convert.ToDateTime(sqlReader["RecordDate"]);
                    DateTime PayBeginDate = Convert.ToDateTime(sqlReader["PayBeginDate"]);
                    DateTime PayEndDate = Convert.ToDateTime(sqlReader["PayEndDate"]);
                    DateTime ExecuteDate = Convert.ToDateTime(sqlReader["ExecuteDate"]);

                    int sendFlag = Convert.ToInt32(sqlReader["SendFlag"]);

                    Allotment currAllotment = new Allotment();
                    currAllotment.Initialize();
                    currAllotment.StockCode = stockCode;
                    currAllotment.Market = (StockMarket)stockMarket;

                    currAllotment.Scheme = Scheme;
                    currAllotment.Price = Price;

                    currAllotment.RecordDate = RecordDate;
                    currAllotment.PayBeginDate = PayBeginDate;
                    currAllotment.PayEndDate = PayEndDate;
                    currAllotment.ExecuteDate = ExecuteDate;

                    currAllotment.SendFlag = sendFlag;

                    mapAllotment[stockCode] = currAllotment;

                }
                sqlReader.Close();
                Loger.Debug("mapAllotment count = " + mapAllotment.Count);

                zns.ITransactionRemotingProvider[] znRmtIobj = new zns.ITransactionRemotingProvider[BaseConfig.mapNotifySrv.Count];
                for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                {
                    znRmtIobj[i] = Activator.GetObject(typeof(zns.ITransactionRemotingProvider), BaseConfig.mapNotifySrv[i].ri) as zns.ITransactionRemotingProvider;
                }

                Dictionary<string, Allotment> t_mapAllotment = new Dictionary<string, Allotment>(mapAllotment);
                foreach (var data in t_mapAllotment)
                {
                    Allotment t_Allotment = new Allotment();
                    t_Allotment = data.Value;
                    zns.Interface.RI_Result RetSetAllotment = zns.Interface.RI_Result.Success;
                    for (int i = 0; i < BaseConfig.mapNotifySrv.Count; i++)
                    {
                        znRmtIobj[i] = Activator.GetObject(typeof(zns.ITransactionRemotingProvider), BaseConfig.mapNotifySrv[i].ri) as zns.ITransactionRemotingProvider;
                        zns.ITransactionRemotingProvider rmt = znRmtIobj[i];
                        RetSetAllotment = rmt.SetExAllotments("", t_Allotment.StockCode, (zns.Interface.RI_Market)t_Allotment.Market,
                        t_Allotment.Scheme, t_Allotment.Price,
                        t_Allotment.RecordDate, t_Allotment.PayBeginDate, t_Allotment.PayEndDate, t_Allotment.ExecuteDate,
                        true);
                        Loger.Debug("StockCode = " + data.Value.StockCode + "\t" + "status = " + RetSetAllotment);
                    }
                    t_Allotment.SendFlag = 1;
                    mapAllotment[data.Key] = t_Allotment;
                }

                Loger.Debug("<<< 配股信息发送完成 >>>");
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
    }




}
