using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using zns = Stock_Trading_Simulator_Kernel;

namespace ReviseTrade
{
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

        public static string RankRmtSrv;

        public static string RmtUserKey;
        public static Dictionary<string, string> mapRmtSrv = new Dictionary<string, string>();

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

                    RankRmtSrv = xmlDoc.SelectSingleNode("Play/RankRmtSrv").InnerText;

                    RmtUserKey = xmlDoc.SelectSingleNode("Play/rmtUserKey").InnerText;
                    XmlNode node = xmlDoc.SelectSingleNode("Play/RmtSrv");
                    foreach (XmlNode s in node.ChildNodes)
                    {
                        switch (s.Name.ToLower())
                        {
                            case "add":
                                string key =s.Attributes["key"].Value.Trim();
                                string ri = s.Attributes["ri"].Value.Trim();
                                mapRmtSrv[key] = ri;
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
