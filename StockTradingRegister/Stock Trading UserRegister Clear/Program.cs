using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace Stock_Trading_UserRegister_Clear
{
    [Serializable]
    public struct UserInfo
    {
        public int UserId;
        public string UserName;
        public int PlayId;
        public int GameId;
        public int AreaId;
        public DateTime Rtime;
        public string RIP;
        public string UserDataBase;
        public byte Validity;

        public void Clear()
        {
            UserId = 0;
            UserName = "";
            PlayId = 0;
            GameId = 0;
            AreaId = 0;
            Rtime = DateTime.MinValue;
            RIP = "";
            UserDataBase = "";
            Validity = 0;
        }

    }

    class Program
    {
        private static Dictionary<int, UserInfo> mapUser = null;
        private static Dictionary<string, string> mapClassUser = null;

        private static bool Initialize()
        {
            try
            {
                mapUser = new Dictionary<int, UserInfo>();
                mapClassUser = new Dictionary<string, string>();
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static void Main(string[] args)
        {
            Initialize();
            LoadUser();
            ClearUser();
        }



        private static bool LoadUser()
        {
            Console.WriteLine(">>> load user started <<<");

            SqlConnection sqlConn = new SqlConnection(BaseConfig.ConnStr);
            SqlCommand sqlCmd = null;

            if (sqlConn.State == ConnectionState.Closed)
                sqlConn.Open();
            if (mapUser == null)
                mapUser = new Dictionary<int, UserInfo>();
            else
                mapUser.Clear();

            if (mapClassUser == null)
                mapClassUser = new Dictionary<string, string>();
            else
                mapClassUser.Clear();
  

            try
            {
                sqlCmd = new SqlCommand(@" select * from userlist" + BaseConfig.PlayId+" order by username ", sqlConn);
                sqlReader = sqlCmd.ExecuteReader();

                while (sqlReader.Read())
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.Clear();
                    userInfo.UserId = Convert.ToInt32(sqlReader["UserId"]);
                    userInfo.UserName = sqlReader["UserName"].ToString();

                    mapUser[userInfo.UserId] = userInfo;
                    if (!mapClassUser.ContainsKey(userInfo.UserName.Substring(0, 1)))
                        mapClassUser.Add(userInfo.UserName.Substring(0, 1), userInfo.UserName + ",");
                    else
                        mapClassUser[userInfo.UserName.Substring(0, 1)] += userInfo.UserName + ",";
 
                }
                sqlReader.Close();
                Console.WriteLine(" mapClassUser count = " + mapClassUser.Count);
                Console.WriteLine("<<< load user finished >>>");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        private static bool ClearUser()
        {

            foreach(var data in mapUser)
            {
                int userId = data.Value.UserId;
                string userName = data.Value.UserName;
                string regUserName = Regex.Match(userName, @"([^\d]*)").Groups[1].Value;
                string source = mapClassUser[userName.Substring(0, 1)];
                MatchCollection matches = Regex.Matches(source, "(" + regUserName + @"[^,]*)");
                string userLogContent = string.Empty;
                foreach(Match m in matches)
                {
                    userLogContent += m.Groups[1].Value + "\r\n";
                }
                Loger.Debug(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + regUserName + ".txt", userLogContent);
            }
            return true;
        }
    }
}
