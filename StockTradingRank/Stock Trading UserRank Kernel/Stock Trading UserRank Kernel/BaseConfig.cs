using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using zns = Stock_Trading_Simulator_Kernel;

namespace Stock_Trading_UserRank_Kernel
{
   public class NotifySrv
    {
       public string DataBaseChar;
        public string ri;
        public NotifySrv(string m_DataBaseChar,string m_ri)
        {
            DataBaseChar = m_DataBaseChar;
            ri = m_ri;
        }
    }

   public class Holiday
   {
       public DateTime HolidayDate;
       public bool Validity;
   }

    public class BaseConfig
    {
        public static int PlayId;
        public static int GameId;
        public static string AreaIds;
        public static string ConnStr;
        public static string DBFSHPath;
        public static string DBFSZPath;
        public static double RateUSD;
        public static double RateHKD;
        public static int RunTimeHour;
        public static double InitCashRMB;
        public static double InitCashUSD;
        public static double InitCashHKD;

        public static double RatioBaseLine;

        public static string RankNotifySrv;

        public static string RmtUserKey;
        public static Dictionary<int, NotifySrv> mapNotifySrv = new Dictionary<int, NotifySrv>();

        public static Dictionary<DateTime, bool> mapHoliday = new Dictionary<DateTime, bool>();
        static BaseConfig()
        {
            try
            {
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string configFile = appPath + "PlaySetting.xml";

                if (System.IO.File.Exists(configFile))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(configFile);

                    PlayId = Convert.ToInt32(xmlDoc.SelectSingleNode("Play/PlayId").InnerText);
                    GameId = Convert.ToInt32(xmlDoc.SelectSingleNode("Play/GameId").InnerText);
                    AreaIds = xmlDoc.SelectSingleNode("Play/AreaIds").InnerText;

                    ConnStr = xmlDoc.SelectSingleNode("Play/ConnStr").InnerText;
                    DBFSHPath = xmlDoc.SelectSingleNode("Play/DBFSHPath").InnerText;
                    DBFSZPath = xmlDoc.SelectSingleNode("Play/DBFSZPath").InnerText;

                    RateUSD = Convert.ToDouble(xmlDoc.SelectSingleNode("Play/RateUSD").InnerText);
                    RateHKD = Convert.ToDouble(xmlDoc.SelectSingleNode("Play/RateHKD").InnerText);

                    RunTimeHour = Convert.ToInt32(xmlDoc.SelectSingleNode("Play/RunTimeHour").InnerText);

                    InitCashRMB = Convert.ToDouble(xmlDoc.SelectSingleNode("Play/InitCashRMB").InnerText);
                    InitCashUSD = Convert.ToDouble(xmlDoc.SelectSingleNode("Play/InitCashUSD").InnerText);
                    InitCashHKD = Convert.ToDouble(xmlDoc.SelectSingleNode("Play/InitCashHKD").InnerText);

                    RatioBaseLine = Convert.ToDouble(xmlDoc.SelectSingleNode("Play/RatioBaseLine").InnerText);

                    RankNotifySrv = xmlDoc.SelectSingleNode("Play/RankNotifySrv").InnerText;

                    RmtUserKey = xmlDoc.SelectSingleNode("Play/rmtUserKey").InnerText;
                    XmlNode node = xmlDoc.SelectSingleNode("Play/NotifySrv");
                    foreach (XmlNode s in node.ChildNodes)
                    {
                        switch (s.Name.ToLower())
                        {
                            case "add":
                                int key =Convert.ToInt32( s.Attributes["key"].Value.Trim());
                                string DataBaseChar = s.Attributes["DataBaseChar"].Value.Trim();
                                string ri = s.Attributes["ri"].Value.Trim();
                                mapNotifySrv[key] = new NotifySrv(DataBaseChar, ri);
                                break;
                        }
                    }

                    XmlNode holidayNode = xmlDoc.SelectSingleNode("Play/Holiday");
                    foreach (XmlNode s in holidayNode.ChildNodes)
                    {
                        switch (s.Name.ToLower())
                        {
                            case "add":
                                DateTime key = Convert.ToDateTime(s.Attributes["key"].Value.Trim());
                                bool validity = Convert.ToBoolean(s.Attributes["validity"].Value.Trim());
                                mapHoliday[key] = validity;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loger.Debug("baseconfig error "+ex.ToString());
            }
        }
    }
}
