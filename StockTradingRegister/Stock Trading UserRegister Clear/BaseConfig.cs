using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Configuration;

namespace Stock_Trading_UserRegister_Clear
{ 
   class BaseConfig
    {
        public static string ConnStr;
        public static int PlayId;
 
        static BaseConfig()
        {
            try
            {
                ConnStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;
                PlayId = Convert.ToInt32(ConfigurationManager.AppSettings["PlayId"].ToString());
            }
            catch (Exception ex)
            {
                Loger.Debug(ex.ToString());
            }
        }
    }
}
