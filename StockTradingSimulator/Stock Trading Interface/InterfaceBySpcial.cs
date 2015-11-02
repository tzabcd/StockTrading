using System;
using System.Collections.Generic;
using System.Text;

namespace Stock_Trading_Simulator_Kernel
{
    public partial class RemotingInterface
    {
        /// <summary>
        /// 分红
        /// </summary>
        /// <param name="strAdminKey"></param>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <param name="Quotiety"></param>
        /// <returns></returns>
        public RI_Result SetBonus(string strAdminKey, string StockCode, RI_Market Market, double Quotiety)
        {
            try
            {
                if (strAdminKey == null || string.Compare(strAdminKey.Trim(),
                    Common.Config("Authorization", "AdminKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strAdminKey.Trim() + "].");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_Maintain)
                {
                    Common.Log("The Interface [ServiceMaintain] Is Closed.");
                    return RI_Result.Closed_Interface;
                }
                else if (Common.DBSync == null)
                    return RI_Result.Internal_Error;
                else
                {
                    RI_Result Rtn = Common.DBSync.SetBonus(StockCode, (TradingSystem.StockMarket)Market, Quotiety);
                    if (Rtn == RI_Result.Success)
                        Common.Log("Set Bonus Info [" + StockCode + "-" + (byte)Market + "/" + Quotiety.ToString("f4").Trim() + "].");
                    return Rtn;
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                return RI_Result.Internal_Error;
            }
        }

        /// <summary>
        /// 除权
        /// </summary>
        /// <param name="strAdminKey"></param>
        /// <param name="StockCode"></param>
        /// <param name="Market"></param>
        /// <param name="Quotiety"></param>
        /// <returns></returns>
        public RI_Result SetExRights(string strAdminKey, string StockCode, RI_Market Market, double Quotiety)
        {
            try
            {
                if (strAdminKey == null || string.Compare(strAdminKey.Trim(),
                    Common.Config("Authorization", "AdminKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strAdminKey.Trim() + "].");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_Maintain)
                {
                    Common.Log("The Interface [ServiceMaintain] Is Closed.");
                    return RI_Result.Closed_Interface;
                }
                else if (Common.DBSync == null)
                    return RI_Result.Internal_Error;
                else
                {
                    RI_Result Rtn = Common.DBSync.SetExRights(StockCode, (TradingSystem.StockMarket)Market, Quotiety);
                    if (Rtn == RI_Result.Success)
                        Common.Log("Set ExRights Info [" + StockCode + "-" + (byte)Market + "/" + Quotiety.ToString("f4").Trim() + "].");
                    return Rtn;
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                return RI_Result.Internal_Error;
            }
        }

        /// <summary>
        /// 扣除现金
        /// </summary>
        /// <param name="strAdminKey"></param>
        /// <param name="UserID"></param>
        /// <param name="Curr"></param>
        /// <param name="Quotiety"></param>
        /// <returns></returns>
        public RI_Result SetForfeiture(string strAdminKey, int UserID, RI_Currency Curr, double Quotiety)
        {
            try
            {
                if (strAdminKey == null || string.Compare(strAdminKey.Trim(),
                    Common.Config("Authorization", "AdminKey").Trim()) != 0)
                {
                    Common.Log("UnAuthorized Remoting Key [" + strAdminKey.Trim() + "].");
                    return RI_Result.Unauthorized;
                }
                else if (!Common.Switch_Maintain)
                {
                    Common.Log("The Interface [ServiceMaintain] Is Closed.");
                    return RI_Result.Closed_Interface;
                }
                else if (Common.DBSync == null)
                    return RI_Result.Internal_Error;
                else
                {
                    RI_Result Rtn = Common.DBSync.SetForfeiture(UserID, (TradingSystem.Currency)Curr, Quotiety);
                    if (Rtn == RI_Result.Success)
                        Common.Log("Set Forfeiture Info [" + UserID + "/" + Curr.ToString().Trim() + "].");
                    return Rtn;
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
