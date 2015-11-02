using System;
using System.Collections.Generic;
using System.Text;

namespace Stock_Trading_Simulator_Kernel
{
    public partial class RemotingInterface 
    {

        /// <summary>
        /// 获取用户总数
        /// </summary>
        /// <param name="strAnyKey"></param>
        /// <returns></returns>
        public int RequestAllUsersCount(string strAnyKey)
        {
            try
            {
                if (strAnyKey == null || (string.Compare(strAnyKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0
                    && string.Compare(Common.Config("Authorization", "AdminKey").Trim(),
                    strAnyKey.Trim()) != 0))
                {
                    Common.Log("UnAuthorized Remoting Key [" + strAnyKey.Trim() + "].");
                    return -1;
                }
                else if (Common.stkBuffer == null)
                {
                    Common.Log("Interface Buffer Is Null.");
                    return -1;
                }
                else if (DateTime.Now.Subtract(dtLastReqAllUsersCount) < new TimeSpan(0, 0, 10))
                {
                    Common.Log("The Invoking Frequency of RequestAllUsersCount() Is Too High !" + Environment.NewLine +
                         "Last Invoked: " + dtLastReqAllUsersCount.ToString("yyyy-MM-dd HH:mm:ss"));
                    return -1;
                }
                dtLastReqAllUsersCount = DateTime.Now;
                return Common.stkBuffer.GetUsersCount();
            }
            catch (Exception err)
            {
                Common.Log(err);
                return -1;
            }
        }


        /// <summary>
        /// 服务器时间
        /// </summary>
        /// <param name="strAnyKey"></param>
        /// <returns></returns>
        public string RequestSvrDateTime(string strAnyKey)
        {
            try
            {
                if (strAnyKey == null || (string.Compare(strAnyKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0
                    && string.Compare(Common.Config("Authorization", "AdminKey").Trim(),
                    strAnyKey.Trim()) != 0))
                    return "";
                return DateTime.Now.ToString("公元yyyy年MM月dd日HH时mm分ss秒");
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 是否禁止交易指定证券类型的证券
        /// </summary>
        /// <param name="sType"></param>
        /// <returns></returns>
        private bool IsBanned(TradingSystem.StockType sType)
        {
            try
            {
                string strSpecies = "";
                for (int i = 0; i < 100; i++)
                {
                    strSpecies = Common.Config("Trading", "Species-" + i.ToString("00").Trim());
                    if (strSpecies.Length > 0)
                    {
                        if (string.Compare(strSpecies.ToUpper().Trim(),
                            sType.ToString().ToUpper().Trim()) == 0)
                            return false;
                    }
                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 获取服务配置信息
        /// </summary>
        /// <param name="strAnyKey"></param>
        /// <returns></returns>
        public RI_Configuration GetServiceConfiguration(string strAnyKey)
        {
            try
            {
                if (strAnyKey == null || (string.Compare(strAnyKey.Trim(),
                    Common.Config("Authorization", "UserKey").Trim()) != 0
                    && string.Compare(Common.Config("Authorization", "AdminKey").Trim(),
                    strAnyKey.Trim()) != 0))
                {
                    Common.Log("UnAuthorized Remoting Key [" + strAnyKey.Trim() + "].");
                    return new RI_Configuration();
                }
                else if (!Common.Switch_Configuration)
                {
                    Common.Log("The Interface [ServiceConfiguration] Is Closed.");
                    return new RI_Configuration();
                }
                return Management.GetConfiguration();
            }
            catch (Exception err)
            {
                Common.Log(err);
                return new RI_Configuration();
            }
        }

        /// <summary>
        /// 设置服务配置项
        /// </summary>
        /// <param name="strAdminKey"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public RI_Result SetServiceConfiguration(string strAdminKey, RI_Configuration config)
        {
            try
            {
                if (strAdminKey == null || string.Compare(strAdminKey.Trim(),
                    Common.Config("Authorization", "AdminKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strAdminKey.Trim() + "].");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_Configuration)
                {
                    Common.Log("The Interface [ServiceConfiguration] Is Closed.");
                    return RI_Result.Closed_Interface;
                }
                RI_Result Rtn = Management.SetConfiguration(config);
                if (Rtn == RI_Result.Success)
                    Common.Log("The Configuration Settings Have Been Changed.");
                return Rtn;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return RI_Result.Internal_Error;
            }
        }

        /// <summary>
        /// 设置服务运行状态
        /// </summary>
        /// <param name="strAdminKey"></param>
        /// <param name="bStatus"></param>
        /// <returns></returns>
        public RI_Result SetServiceStatus(string strAdminKey, bool bStatus)
        {
            try
            {
                if (strAdminKey == null || string.Compare(strAdminKey.Trim(),
                    Common.Config("Authorization", "AdminKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strAdminKey.Trim() + "].");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_ServiceStatus)
                {
                    Common.Log("The Interface [ServiceStatus] Is Closed.");
                    return RI_Result.Closed_Interface;
                }
                Management.Work = bStatus;
                if (Management.Work)
                {
                    Common.Log("The Service Starts Working.");
                    return RI_Result.Service_Start;
                }
                else
                {
                    Common.Log("The Service Stops Working.");
                    return RI_Result.Service_Stop;
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                return RI_Result.Internal_Error;
            }
        }
    }
}
