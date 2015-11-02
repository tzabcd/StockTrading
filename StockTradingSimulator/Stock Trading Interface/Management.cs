#if INTERNEL
using System;
using System.Collections.Generic;
using System.Text;

namespace Stock_Trading_Simulator_Kernel
{
    /// <summary>
    /// 系统管理类
    /// </summary>
    public class Management
    {
        private static bool bStatus = true;
        private static DateTime dtStatusChangedTime = DateTime.Now;
        private static DateTime dtConfigChangedTime = DateTime.Now;

        /// <summary>
        /// 是否工作日
        /// </summary>
        public static bool Work
        {
            get
            {
                try
                {
                    return bStatus;
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    bStatus = value;
                    dtStatusChangedTime = DateTime.Now;
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 状态改变时间
        /// </summary>
        public static DateTime StatusChangedTime
        {
            get
            {
                try
                {
                    return dtStatusChangedTime;
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// 配置改变时间
        /// </summary>
        public static DateTime ConfigChangedTime
        {
            get
            {
                try
                {
                    return dtConfigChangedTime;
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static RemotingInterface.RI_Result SetConfiguration(RemotingInterface.RI_Configuration settings)
        {
            try
            {
                if (settings.InitAUD < 0 || settings.InitAUD > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitCAD < 0 || settings.InitCAD > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitCHF < 0 || settings.InitCHF > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitEUR < 0 || settings.InitEUR > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitGBP < 0 || settings.InitGBP > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitHKD < 0 || settings.InitHKD > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitJPY < 0 || settings.InitJPY > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitNZD < 0 || settings.InitNZD > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitRMB < 0 || settings.InitRMB > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitUSD < 0 || settings.InitUSD > (double)1000 * 1000 * 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.InitAUD < 1000 && settings.InitCAD < 1000 &&
                    settings.InitCHF < 1000 && settings.InitEUR < 1000 &&
                    settings.InitGBP < 1000 && settings.InitHKD < 1000 &&
                    settings.InitJPY < 1000 && settings.InitNZD < 1000 &&
                    settings.InitRMB < 1000 && settings.InitUSD < 1000)
                    return RemotingInterface.RI_Result.Illegal_InitFund;
                else if (settings.MaxOrders < 16 || settings.MaxOrders > 256)
                    return RemotingInterface.RI_Result.Illegal_OrdersMax;
                else if (settings.SingleStockRate < 0.01 || settings.SingleStockRate > 10)
                    return RemotingInterface.RI_Result.Illegal_SingleStockRate;
                else if (settings.BuyTax < 0 || settings.BuyTax > 0.5)
                    return RemotingInterface.RI_Result.Illegal_Tax;
                else if (settings.SellTax < 0 || settings.SellTax > 0.5)
                    return RemotingInterface.RI_Result.Illegal_Tax;
                else if (settings.ListSpecies != null && settings.ListSpecies.Count > 32)
                    return RemotingInterface.RI_Result.Too_Many_Species;
                else if (Common.stkTrading == null)
                    return RemotingInterface.RI_Result.Internal_Error;
                else
                {
                    lock (Common.stkTrading)
                    {
                        Common.stkTrading.defaultAUD = settings.InitAUD;
                        Common.stkTrading.defaultCAD = settings.InitCAD;
                        Common.stkTrading.defaultCHF = settings.InitCHF;
                        Common.stkTrading.defaultEUR = settings.InitEUR;
                        Common.stkTrading.defaultGBP = settings.InitGBP;
                        Common.stkTrading.defaultHKD = settings.InitHKD;
                        Common.stkTrading.defaultJPY = settings.InitJPY;
                        Common.stkTrading.defaultNZD = settings.InitNZD;
                        Common.stkTrading.defaultRMB = settings.InitRMB;
                        Common.stkTrading.defaultUSD = settings.InitUSD;
                        Common.stkTrading.defaultBuyTax = settings.BuyTax;
                        Common.stkTrading.defaultSellTax = settings.SellTax;
                        Common.stkTrading.defaultSingleStockRate = settings.SingleStockRate;
                    }
                    Common.MaxOrders = settings.MaxOrders;
                    Common.WebService_PlayID = settings.WSPlayID;
                    Common.UpdConfig("Trading", "InitAUD", settings.InitAUD.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitCAD", settings.InitCAD.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitCHF", settings.InitCHF.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitEUR", settings.InitEUR.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitGBP", settings.InitGBP.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitHKD", settings.InitHKD.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitJPY", settings.InitJPY.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitNZD", settings.InitNZD.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitRMB", settings.InitRMB.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "InitUSD", settings.InitUSD.ToString("f0").ToString());
                    Common.UpdConfig("Trading", "BuyTax", settings.BuyTax.ToString("f4").ToString());
                    Common.UpdConfig("Trading", "SellTax", settings.SellTax.ToString("f4").ToString());
                    Common.UpdConfig("Trading", "SingleRate", settings.SingleStockRate.ToString("f3").ToString());
                    Common.UpdConfig("Trading", "MaxOrders", settings.MaxOrders.ToString().ToString());
                    Common.UpdConfig("System", "WSPlayID", settings.WSPlayID.ToString().ToString());
                    if (settings.ListSpecies != null)
                    {
                        for (int i = 0; i < 32; i++)
                        {
                            string strSpecies = settings.ListSpecies[i].ToUpper().Trim();
                            if (string.Compare(strSpecies, TradingSystem.StockType.ETF.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SH_A.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SH_B.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SH_Bond.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SH_ClosedFund.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SH_Index.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SH_OpenedFund.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SH_Warrant.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SZ_A.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SZ_B.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SZ_Bond.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SZ_ClosedFund.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SZ_Index.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SZ_OpenedFund.ToString().ToUpper().Trim()) == 0 ||
                                string.Compare(strSpecies, TradingSystem.StockType.SZ_Warrant.ToString().ToUpper().Trim()) == 0)
                                Common.UpdConfig("Trading", "Species-" + i.ToString("00").Trim(), settings.ListSpecies[i].Trim());
                            else
                                Common.UpdConfig("Trading", "Species-" + i.ToString("00").Trim(), "");
                        }
                    }
                    dtConfigChangedTime = DateTime.Now;
                    return RemotingInterface.RI_Result.Success;
                }
            }
            catch
            {
                return RemotingInterface.RI_Result.Internal_Error;
            }
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <returns></returns>
        public static RemotingInterface.RI_Configuration GetConfiguration()
        {
            try
            {
                RemotingInterface.RI_Configuration Rtn = new RemotingInterface.RI_Configuration(); Rtn.Clear();
                lock (Common.stkTrading)
                {
                    Rtn.InitAUD = Common.stkTrading.defaultAUD;
                    Rtn.InitCAD = Common.stkTrading.defaultCAD;
                    Rtn.InitCHF = Common.stkTrading.defaultCHF;
                    Rtn.InitEUR = Common.stkTrading.defaultEUR;
                    Rtn.InitGBP = Common.stkTrading.defaultGBP;
                    Rtn.InitHKD = Common.stkTrading.defaultHKD;
                    Rtn.InitJPY = Common.stkTrading.defaultJPY;
                    Rtn.InitNZD = Common.stkTrading.defaultNZD;
                    Rtn.InitRMB = Common.stkTrading.defaultRMB;
                    Rtn.InitUSD = Common.stkTrading.defaultUSD;
                    Rtn.BuyTax = Common.stkTrading.defaultBuyTax;
                    Rtn.SellTax = Common.stkTrading.defaultSellTax;
                    Rtn.SingleStockRate = Common.stkTrading.defaultSingleStockRate;
                }
                Rtn.MaxOrders = Common.MaxOrders;
                Rtn.WSPlayID = Common.WebService_PlayID;
                Rtn.ListSpecies.Clear();
                string strSpecies = "";
                for (int i = 0; i < 100; i++)
                {
                    strSpecies = Common.Config("Trading", "Species-" + i.ToString("00").Trim()).ToUpper().Trim();
                    if (strSpecies.Length > 0)
                    {
                        if (string.Compare(strSpecies, TradingSystem.StockType.ETF.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SH_A.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SH_B.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SH_Bond.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SH_ClosedFund.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SH_Index.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SH_OpenedFund.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SH_Warrant.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SZ_A.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SZ_B.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SZ_Bond.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SZ_ClosedFund.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SZ_Index.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SZ_OpenedFund.ToString().ToUpper().Trim()) == 0 ||
                            string.Compare(strSpecies, TradingSystem.StockType.SZ_Warrant.ToString().ToUpper().Trim()) == 0)
                            Rtn.ListSpecies.Add(Common.Config("Trading", "Species-" + i.ToString("00").Trim()));
                    }
                }
                return Rtn;
            }
            catch
            {
                return new RemotingInterface.RI_Configuration();
            }
        }
    }
}
#endif