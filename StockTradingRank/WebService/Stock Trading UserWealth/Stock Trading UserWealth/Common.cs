using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Configuration;

namespace Stock_Trading_UserWealth
{
    [Serializable]
    public struct UserWealth
    {
        public int UserID;
        public decimal RMB;
        public decimal USD;
        public decimal HKD;

        public void Initialize()
        {
            UserID = 0;
            RMB = 0.00m;
            USD = 0.00m;
            HKD = 0.00m;
        }
    }

    public class Common
    {
        private static SqlConnection sqlConn = null;
        private static SqlCommand sqlCmd = null;
        private static SqlDataReader sqlReader = null;

        private static string strConn = "";
        private static int playId = 0;
        public static Dictionary<int, UserWealth> mapUserWealth = null;

        public static DataSet dsUserWealth = null;

        private static Thread ThTrading = null;
        private static bool bTrading = false;

        public static DateTime CurrentDay = DateTime.Today;
        public static bool Initialize()
        {
            try
            {
                strConn = ConfigurationManager.AppSettings["connStr"].ToString();
                playId = Convert.ToInt32(ConfigurationManager.AppSettings["playId"]);

                sqlConn = new SqlConnection(strConn.Trim());

                mapUserWealth = new Dictionary<int, UserWealth>();

                Common.Log(">>>>>>>>>>>>>>>初始化<<<<<<<<<<<<<<<<<<<");
                if (!Common.SetUserWealthBuffer())
                    return false;

                CurrentDay = DateTime.Today;
                bTrading = true;
                ThTrading = new Thread(new ThreadStart(Run));
                ThTrading.IsBackground = true;
                ThTrading.Name = "ThTrading";
                ThTrading.Start();

                return true;
            }
            catch (Exception err)
            {
                Common.Log(err.ToString());
                return false;
            }
        }

        public static void Uninitialize()
        {
            try
            {
                if (mapUserWealth!=null)
                {
                    mapUserWealth.Clear();
                }

                bTrading = false;
                if (ThTrading != null && ThTrading.IsAlive)
                {
                    ThTrading.Join(4500);
                    if (ThTrading != null && ThTrading.IsAlive)
                        ThTrading.Abort();
                }
            }
            catch (Exception err)
            {
                Common.Log(err.ToString());
            }
            finally
            {
                Common.Log("Trading System Terminated");
            }
        }

        public static void Run()
        {
            while (bTrading)
            {
                if (CurrentDay != DateTime.Today && DateTime.Now.Hour == 9)
                {
                    SetUserWealthBuffer();
                    CurrentDay = DateTime.Today;
                    Log("刷新数据.");
                }
                Thread.Sleep(10000);
            }
        }

        public static bool SetUserWealthBuffer()
        {
            try
            {
                if (mapUserWealth == null)
                    mapUserWealth = new Dictionary<int, UserWealth>();
                else
                    mapUserWealth.Clear();

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                sqlCmd = new SqlCommand("GetUserWealth" + playId, sqlConn);
                sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    UserWealth userWealth = new UserWealth();
                    userWealth.Initialize();
                    userWealth.UserID = Convert.ToInt32(sqlReader["UserID"]);
                    userWealth.RMB = Convert.ToDecimal(sqlReader["WealthRMB"]);
                    userWealth.USD = Convert.ToDecimal(sqlReader["WealthUSD"]);
                    userWealth.HKD = Convert.ToDecimal(sqlReader["WealthHKD"]);

                    if (!mapUserWealth.ContainsKey(userWealth.UserID))
                    {
                        mapUserWealth.Add(userWealth.UserID, userWealth);
                    }
                }
                sqlReader.Close();

                Common.Log("mapUserWealth 加载成功.数目[" + mapUserWealth.Count.ToString() + "]");

                if (!GetDataSetUserWealth())
                {
                    Log("mapUserWealth DataSet转换失败");
                    return false;
                }
                else
                {
                    Log("mapUserWealth DataSet转换成功");
                }
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err.ToString());
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }
  
        public static bool AddNewUser(int areaId,int userId)
        {
            try
            {
//                if (sqlConn.State == ConnectionState.Closed)
//                    sqlConn.Open();

//                sqlCmd = new SqlCommand(@"SELECT UserId,AreaId FROM [UserRank]
//                                        WHERE UserId = @UserId AND AreaId = @AreaId ", sqlConn);
//                sqlCmd.Parameters.Add("@UserId", SqlDbType.Int); sqlCmd.Parameters["@UserId"].Value = userId;
//                sqlCmd.Parameters.Add("@AreaId", SqlDbType.Int); sqlCmd.Parameters["@AreaId"].Value = areaId;
//                if (sqlCmd.ExecuteNonQuery() > 0)
//                    return false;

//                sqlCmd = new SqlCommand(@"INSERT [UserRank] (UserId,AreaId,RMB,HKD,USD,RankDate) VALUES 
//                                                        (@UserId,@AreaId,100000,50000,5000,@RankDate)", sqlConn);
//                sqlCmd.Parameters.Add("@UserId", SqlDbType.Int); sqlCmd.Parameters["@UserId"].Value = userId;
//                sqlCmd.Parameters.Add("@AreaId", SqlDbType.Int); sqlCmd.Parameters["@AreaId"].Value = areaId;
//                sqlCmd.Parameters.Add("@RankDate", SqlDbType.DateTime); sqlCmd.Parameters["@RankDate"].Value = DateTime.Today;
//                sqlCmd.ExecuteNonQuery();

//                Common.Log("用户添加成功：userId:" + userId);
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err.ToString());
                return false;
            }
            //finally
            //{
            //    if (sqlConn.State != ConnectionState.Closed)
            //        sqlConn.Close();
            //}
        }

        public static bool GetDataSetUserWealth()
        {
            {
                try
                {
                    if (Common.mapUserWealth == null)
                        return false;

                    DataSet ds = new DataSet();
                    DataTable dt = new DataTable("tbUserWealth");

                    // Create DataColumn objects of data types.
                    DataColumn UserId = new DataColumn("UserId");
                    UserId.DataType = System.Type.GetType("System.Int32");
                    dt.Columns.Add(UserId);

                    DataColumn RMB = new DataColumn("RMB");
                    RMB.DataType = System.Type.GetType("System.Decimal");
                    dt.Columns.Add(RMB);

                    DataColumn USD = new DataColumn("USD");
                    USD.DataType = System.Type.GetType("System.Decimal");
                    dt.Columns.Add(USD);

                    DataColumn HKD = new DataColumn("HKD");
                    HKD.DataType = System.Type.GetType("System.Decimal");
                    dt.Columns.Add(HKD);

                    foreach (KeyValuePair<int, UserWealth> s in Common.mapUserWealth)
                    {

                        // Populate one row with values.
                        DataRow dr = dt.NewRow();
                        dr["UserId"] = s.Key.ToString();
                        dr["RMB"] = s.Value.RMB.ToString();
                        dr["USD"] = s.Value.USD.ToString();
                        dr["HKD"] = s.Value.HKD.ToString();
                        dt.Rows.Add(dr);
                    }

                    ds.Tables.Add(dt);

                    dsUserWealth = ds;

                    return true;

                }
                catch (Exception ex)
                {
                    Common.Log(ex.ToString());
                    return false;
                }

            }

        }

        public static void Log(string msg)
        {
            using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\UserWealth.log", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+" : "+ msg);
                sw.Close();
            }
        }

    }
}
