using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace CalcDurationRatioRank
{
    /// <summary>
    /// RatioRank 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class RatioRank : System.Web.Services.WebService
    {
        /// <summary>
        /// 获取正收益排行
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public DataSet GetUpRationRank(int playId)
        {
            return Core.GetUpRationRank(playId);
        }
    }
}
