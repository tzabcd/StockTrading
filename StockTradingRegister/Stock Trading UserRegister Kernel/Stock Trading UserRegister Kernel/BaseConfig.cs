using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Stock_Trading_UserRegister_Kernel
{ 
    /// <summary>
    /// 通知对象
    /// </summary>
    public class NotifySrv
    {
        public string DataBaseChar;  //对应交易服务器标识
        public string ri;  //通知的remoting

        public NotifySrv(string m_DataBaseChar,  string m_ri)
        {
            DataBaseChar = m_DataBaseChar;
            ri = m_ri;
        }
    }

   class BaseConfig
    {
        // 通知的对象集 Dictionary<PlayId, Dictionary<DataBaseChar, NotifySrv>> 
        public static Dictionary<int, Dictionary<string, NotifySrv>> mapNotifySrv = new Dictionary<int, Dictionary<string, NotifySrv>>();
        public static int PlayId;
        public static string ConnStr;  
        public static string rmtAdminKey;
        public static string CurrentRmtServerKey;  //当前交易服务器标识
        public static int RmtNumber; //使用
        public static bool bAllowReg;
 
        static BaseConfig()
        {
            try
            {
                string configFile = AppDomain.CurrentDomain.BaseDirectory + "UserRegisterSetting.xml";
               
                Dictionary<string, NotifySrv> mapPlayNotify = new Dictionary<string, NotifySrv>();
                
                if (System.IO.File.Exists(configFile))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(configFile);

                    PlayId = Convert.ToInt32(xmlDoc.SelectSingleNode("RegSetting/PlayId").InnerText);
                    ConnStr = xmlDoc.SelectSingleNode("RegSetting/ConnStr").InnerText;
                    rmtAdminKey = xmlDoc.SelectSingleNode("RegSetting/rmtAdminKey").InnerText;
                    CurrentRmtServerKey =xmlDoc.SelectSingleNode("RegSetting/CurrentRmtServerKey").InnerText;

                    RmtNumber = Convert.ToInt32(xmlDoc.SelectSingleNode("RegSetting/RmtNumber").InnerText);
                    int nAllowReg = Convert.ToInt32(xmlDoc.SelectSingleNode("RegSetting/AllowReg").InnerText);
                    if (nAllowReg == 0)
                        bAllowReg = false;
                    else
                        bAllowReg = true;

                    XmlNode node = xmlDoc.SelectSingleNode("RegSetting/NotifySrv");
                    foreach (XmlNode s in node.ChildNodes)
                    {
                        switch (s.Name.ToLower())
                        {
                            case "add":
                                int key = Convert.ToInt32(s.Attributes["key"].Value.Trim());
                                int playId = Convert.ToInt32(s.Attributes["PlayId"].Value.Trim());
                                string DataBaseChar = s.Attributes["DataBaseChar"].Value.Trim();
                                string ri = s.Attributes["ri"].Value.Trim();
                                if (!mapNotifySrv.ContainsKey(playId))
                                    mapNotifySrv.Add(playId, new Dictionary<string, NotifySrv>());
                                if (mapNotifySrv[playId].ContainsKey(DataBaseChar))
                                    mapNotifySrv[playId][DataBaseChar] = new NotifySrv(DataBaseChar,  ri);
                                else
                                    mapNotifySrv[playId].Add(DataBaseChar, new NotifySrv(DataBaseChar,  ri));

                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loger.Debug(ex.ToString());
            }
        }
    }
}
