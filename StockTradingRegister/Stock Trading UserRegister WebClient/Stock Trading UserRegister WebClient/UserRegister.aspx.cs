using System;
using System.Collections;
using System.Configuration;
using System.Data;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Stock_Trading_UserRegister_Kernel;

namespace Stock_Trading_UserRegister_WebClient
{
    public partial class _UserRegister : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnUserReg(object sender, EventArgs e)
        {
            try
            {
                string userName = "";
                int playId = 0;
                int gameId = 0;
                int areaId = 0;


                userName = txtUserName.Text;
                playId = Convert.ToInt32(txtPlayId.Text);
                gameId = Convert.ToInt32(txtGameId.Text);
                areaId = Convert.ToInt32(txtAreaId.Text);

                if (BaseConfig.remoteUserRegObj.IsExistUser(playId, userName))
                {
                    Response.Write("<script>alert('存在该用户！');</script>");
                }
                else
                {
                    RemotingInterface.UserInfo userInfo = new RemotingInterface.UserInfo();
                    userInfo.UserPassPortId = userName;
                    userInfo.UserNickName = "testNickName";
                    userInfo.AreaId = areaId;
                    userInfo.PlayId = playId;
                    userInfo.Rtime = DateTime.Now;
                    userInfo.RIP = "127.0.0.1";

                    if (BaseConfig.remoteUserRegObj.RequestNewUser(BaseConfig.strRegUserRemoteKey,userInfo))
                        Response.Write("添加成功");
                    else
                        Response.Write("添加失败");
                }
                 
            }

            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
    }
}
