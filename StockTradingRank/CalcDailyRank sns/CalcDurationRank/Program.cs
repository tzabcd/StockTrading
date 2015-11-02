using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace CalcDurationRank
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<int, UserRank> mapUser = new Dictionary<int, UserRank>();
            string strConn = BaseConfig.ConnStr;
            SqlConnection sqlConn = new SqlConnection(strConn.Trim());
            SqlCommand sqlCmd = null;
            sqlCmd = new SqlCommand(@"update emtradeplay.HistoryRank_"+args[0]+" set durationdays = 1 where dailyprofit > 0 ");
            sqlCmd = new SqlCommand(@"select * from emtradeplay.HistoryRank_"+args[0]+" order by dailyprofit desc, wealth desc", sqlConn);
            sqlCmd.Parameters.Add("@PlayId", SqlDbType.Int).Value = BaseConfig.PlayId;
            SqlDataReader sqlReader = sqlCmd.ExecuteReader();
            while (sqlReader.Read())
            {
                int nUserid = Convert.ToInt32(sqlReader["userId"]);
                UserRank uRank = new UserRank();
                uRank.Initialize();
                uRank.UserId = Convert.ToInt32(sqlReader["userId"]);
                uRank.AreaId = Convert.ToInt32(sqlReader["areaId"]);
                uRank.UserName = sqlReader["userName"].ToString();
                uRank.UserDataBase = sqlReader["UserDataBase"].ToString();
                mapUser[nUserid] = uRank;
            }
        }
    }
}
