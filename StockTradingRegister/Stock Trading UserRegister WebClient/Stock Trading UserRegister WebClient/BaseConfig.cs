using System;
using System.Data;
using System.Configuration;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Stock_Trading_UserRegister_Kernel;

namespace Stock_Trading_UserRegister_WebClient
{
    public class BaseConfig
    {
        private static string RegUserRemotingUrl;
        public static RemotingInterface remoteUserRegObj;
        public static string strRegUserRemoteKey;

        static BaseConfig()
        {
            RegUserRemotingUrl = ConfigurationManager.AppSettings["RegUserRemotingUrl"];
            remoteUserRegObj = Activator.GetObject(typeof(RemotingInterface), RegUserRemotingUrl) as RemotingInterface;
            strRegUserRemoteKey = "testRemoteKey";
        }
    }
}
