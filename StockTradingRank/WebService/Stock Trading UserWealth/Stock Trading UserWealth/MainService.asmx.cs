using System;
using System.ComponentModel;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Stock_Trading_UserWealth
{
    [WebService(Namespace = "http://Stock_Trading_UserWealth.Em/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // [System.Web.Script.Services.ScriptService]
    public class StockTradingUserWealth : System.Web.Services.WebService
    {

        [WebMethod(Description = "用户昨日资金 <br> 返回类型为   DataSet <br> 出错则返回空DataSet ")]
        public DataSet MapUserWealth()
        {
            return Common.dsUserWealth;
        }


        [WebMethod(Description = "手动更新 <br> 返回类型为   bool ")]
        public bool RefreshData()
        {
            if (Common.SetUserWealthBuffer())
            {
                Common.Log("手动加载成功.");
                return true;
            }
            else
            {
                return false;
            }
        }

        [WebMethod(Description = "注册添加排名用户 <br> 返回类型为   bool ")]
        public bool AddNewUser(int areaId,int userId)
        {
           return Common.AddNewUser(areaId, userId);
        }
    }
}
