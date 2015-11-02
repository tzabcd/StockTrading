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
using zns = Stock_Trading_Simulator_Kernel;

namespace ReviseTrade
{
    class Program
    {

        private static string BasePath = "";

        private static SqlConnection sqlConn = null;
        private static SqlCommand sqlCmd = null;
        private static SqlDataReader sqlReader = null;
        private static string strConn = "";

        public static Dictionary<string, zns.RemotingInterface> mapRmtOjb = null;

        static void Main(string[] args)
        {
            Console.WriteLine(" Start Program ... ");
            if (Initialize())
            {
                Console.WriteLine(" Initialize successed ... ");
                SaveBuffer();

                //string strUserList = "3309,3288,3323,3330,3304,3321,3335,3300,3314,3319,3312,3834,3827";
                //string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "20090918-1506/";
                //RestoreData(path,strUserList);
            }
            else
            {
                Console.WriteLine(" Initialize failed ... ");
            }
        }

        static bool Initialize()
        {
            try
            {
                BasePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + DateTime.Now.ToString("yyyyMMdd-HHmm") + "/";
                mapRmtOjb = new Dictionary<string, Stock_Trading_Simulator_Kernel.RemotingInterface>();
                foreach (var data in BaseConfig.mapRmtSrv)
                {
                    mapRmtOjb[data.Key] = Activator.GetObject(typeof(zns.RemotingInterface), BaseConfig.mapRmtSrv[data.Key]) as zns.RemotingInterface;
                }
                return true;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }

        private static void SaveBuffer()
        {
            strConn = BaseConfig.ConnStr;
            sqlConn = new SqlConnection(BaseConfig.ConnStr);
            try
            {
                ClearData();
                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                //读取指定区域内参与交易的用户列表
                string strSql = @"SELECT UserId,AreaId,UserName,UserDataBase FROM emtradeplay.dbo.UserList " +
                    " WHERE Validity =1 AND TradeFlag = 1 AND AreaId in (" + BaseConfig.AreaIds + ")" +
                    " ORDER BY UserId  DESC ";
                sqlCmd = new SqlCommand(strSql, sqlConn);
                sqlReader = sqlCmd.ExecuteReader();
                Console.WriteLine(" Read user start ... ");
                while (sqlReader.Read())
                {
                    int userId = Convert.ToInt32(sqlReader["UserId"]);
                    string userDataBase = sqlReader["UserDataBase"].ToString();
                    Console.WriteLine("--- current user = " + userId);
                    if (userDataBase == null || userDataBase == string.Empty)
                    {
                        Loger.Debug("--- warning : userId = " + userId + " DataBase = null ---");
                        continue;
                    }
                    zns.RemotingInterface cRmt = null;
                    cRmt = mapRmtOjb[userDataBase];

                    try
                    {
                        //用户订单
                        Dictionary<int, zns.RemotingInterface.RI_Order> mapUserOrder = new Dictionary<int, zns.RemotingInterface.RI_Order>();
                        mapUserOrder = cRmt.RequestUserOrders(BaseConfig.RmtUserKey, userId);
                        if (mapUserOrder != null)
                            Loger.Serialize(BasePath + "/UserOrder/" + userId + ".dat", mapUserOrder);
                        else
                            Loger.Serialize(BasePath + "/UserOrder/" + userId + ".dat", "");
                    }
                    catch (System.Exception e)
                    {
                        Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                        Loger.Debug(BasePath + "LostUser.log", userId + " -- mapUserOrder --");
                    }
                    try
                    {
                        //用户持股
                        List<zns.RemotingInterface.RI_Stock> listUserStock = new List<zns.RemotingInterface.RI_Stock>();
                        listUserStock = cRmt.RequestUserStocks(BaseConfig.RmtUserKey, userId);
                        if (listUserStock != null)
                            Loger.Serialize(BasePath + "/UserStock/" + userId + ".dat", listUserStock);
                        else
                            Loger.Serialize(BasePath + "/UserStock/" + userId + ".dat", "");
                    }
                    catch (System.Exception e)
                    {
                        Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                        Loger.Debug(BasePath + "LostUser.log", userId + " -- listUserStock --");
                    }
                    try
                    {
                        //用户交易
                        List<zns.RemotingInterface.RI_Trading> listUserTrade = new List<zns.RemotingInterface.RI_Trading>();
                        listUserTrade = cRmt.RequestUserTrades(BaseConfig.RmtUserKey, userId);
                        if (listUserTrade != null)
                            Loger.Serialize(BasePath + "/UserTrade/" + userId + ".dat", listUserTrade);
                        else
                            Loger.Serialize(BasePath + "/UserTrade/" + userId + ".dat", "");
                    }
                    catch (System.Exception e)
                    {
                        Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                        Loger.Debug(BasePath + "LostUser.log", userId + " -- listUserTrade --");
                    }
                    try
                    {
                        //用户资金
                        Dictionary<byte, zns.RemotingInterface.RI_Fund> mapUserFund = new Dictionary<byte, zns.RemotingInterface.RI_Fund>();
                        mapUserFund = cRmt.RequestUserFund(BaseConfig.RmtUserKey, userId);
                        if (mapUserFund != null)
                            Loger.Serialize(BasePath + "/UserFund/" + userId + ".dat", mapUserFund);
                        else
                            Loger.Serialize(BasePath + "/UserFund/" + userId + ".dat", "");
                    }
                    catch (System.Exception e)
                    {
                        Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                        Loger.Debug(BasePath + "LostUser.log", userId + " -- mapUserFund --");
                    }
                    try
                    {
                        //用户资金流水
                        List<zns.RemotingInterface.RI_FundChanges> listUserFundChange = new List<zns.RemotingInterface.RI_FundChanges>();
                        listUserFundChange = cRmt.RequestUserFundChanges(BaseConfig.RmtUserKey, userId);
                        if (listUserFundChange != null)
                            Loger.Serialize(BasePath + "/UserFundChange/" + userId + ".dat", listUserFundChange);
                        else
                            Loger.Serialize(BasePath + "/UserFundChange/" + userId + ".dat", "");
                    }
                    catch (System.Exception e)
                    {
                        Loger.Debug(BasePath + "/RmtError.log", userId + "\t" + e.ToString());
                        Loger.Debug(BasePath + "LostUser.log", userId + " -- listUserFundChange --");
                    }
                }  //end while(sqlReader.Read()) 
                sqlReader.Close();
                Console.WriteLine("<<<  获取缓存数据完成 >>> ");
            }
            catch (Exception err)
            {
                Loger.Debug("Proc Error:" + err.ToString());
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        private static void RestoreData(string path,string userList)
        {
            string[] arrUser = userList.Split(',');
            for(int i=0;i<arrUser.Length;i++)
            {
                int UserId = Convert.ToInt32(arrUser[i]);
                bool bExists=false;
                //用户订单
                Dictionary<int, zns.RemotingInterface.RI_Order> mapUserOrder = new Dictionary<int, zns.RemotingInterface.RI_Order>();
                mapUserOrder = Loger.DeSerialize<Dictionary<int, zns.RemotingInterface.RI_Order>>(path + "/UserOrder/" + UserId + ".dat", out bExists);
                if(mapUserOrder!=null)
                {
                    Double UserFund = 1000000.0000;
                    foreach(var data in mapUserOrder)
                    {
                        if(data.Value.OrderStatus==zns.RemotingInterface.RI_Status.Finished)
                        {
                            Console.WriteLine("userId = " + UserId + "\t " + data.Key + "\t Order = " + data.Value.OrderID + " " + data.Value.StockCode + " " + data.Value.OrderStatus + " ");
                            if (data.Value.Side == true)
                            {
                                UserFund -= data.Value.TradePrice * data.Value.OrderVolume;
                            }
                            else
                            {
                                UserFund -= data.Value.TradePrice * data.Value.OrderVolume - data.Value.OrderPrice * data.Value.OrderVolume * 3 / 1000;
                            }
                        }
                    }
                    Console.WriteLine("UserId = "+UserId+ " \t UserFund ="+UserFund+"\r\n");
                }
            }

        }

        private static void ClearData()
        {
            try
            {
                Console.WriteLine(">>> Clear Data start <<< ");
                string[] arrDirData = { "UserStock", "UserOrder", "UserTrade", "UserFund", "UserFundChange" };
                for (int i = 0; i < arrDirData.Length; i++)
                {
                    if (Directory.Exists(BasePath + arrDirData[i]))
                    {
                        FileInfo[] files = new DirectoryInfo(BasePath + arrDirData[i]).GetFiles("*.*");
                        foreach (var file in files)
                        {
                            File.Delete(file.FullName);
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(BasePath + arrDirData[i]);
                    }
                }
                Console.WriteLine("<<< Clear Data successed >>> ");
            }
            catch (System.Exception e)
            {
                Console.WriteLine(" Clear Data failed : " + e.ToString());
            }
        }
    }
}
