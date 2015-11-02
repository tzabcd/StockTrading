using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.ComponentModel;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.IO;
using Network_Service_Core;
using Stock_Trading_Simulator_Kernel;

namespace Stock_Trading_Simulator_Shell
{
    public partial class MainService : ServiceBase
    {
        public MainService()
        {
            InitializeComponent();
        }

        private Core_ThreadAndCounter MainInvoker = new Core_ThreadAndCounter();
        private TimeSpan tsReload = TimeSpan.MinValue;
        private bool bDataCleared = true;

        protected override void OnStart(string[] args)
        {
            try
            {
                Common.Log();
                Common.Log("Service: " + Core_ServiceInstaller.ServiceName.Trim());
                Common.Log("Version: " + Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion);

                if (!Common.Initialize())
                {
                    Common.Log("Failed to load all settings.");
                    OnStop();
                    return;
                }
                if (!TimeSpan.TryParse(Common.Config("System", "Reload").Trim(), out tsReload))
                    tsReload = new TimeSpan(0, 0, 0);

                MainInvoker.Create(new Core_ThreadAndCounter.InvokingFunction(LoadingSettings), 600);
                MainInvoker.Start(false);
                Common.Log("The service started.");
            }
            catch (Exception err)
            {
                Common.Log(err);
                OnStop();
            }
        }
        protected override void OnStop()
        {
            try
            {
                if (MainInvoker.IsVaild)
                {
                    MainInvoker.Stop();
                    MainInvoker.Terminate(15);
                }
                Common.Uninitialize();
            }
            catch (Exception err)
            {
                Common.Log(err);
            }
            finally
            {
                Common.Log("The service stopped.");
                Environment.Exit(0);
            }
        }

        public void StartFromConsole(string[] args)
        {
            OnStart(args);
        }
        public void StopFromConsole()
        {
            OnStop();
        }

        private void LoadingSettings()
        {
            try
            {
                if (Common.IsWeekend)
                    return;

                #region Çå¿Õ¡°¸ôÒ¹¡±Êý¾Ý
                if (!bDataCleared && DateTime.Now.TimeOfDay >= tsReload &&
                    DateTime.Now.TimeOfDay <= tsReload.Add(new TimeSpan(0, 30, 0)))
                {
                    if (Common.stkBuffer.Clear() &&
                        Common.stkTrading.Clear())
                        bDataCleared = true;
                }
                else if (DateTime.Now.TimeOfDay < tsReload
                    || DateTime.Now.TimeOfDay > tsReload.Add(new TimeSpan(0, 30, 0)))
                    bDataCleared = false;
                #endregion

                string strSH = Common.Config("Quotation", "Shanghai").Trim();
                string strSZ = Common.Config("Quotation", "Shenzhen").Trim();
                if (strSH.Length <= 0 || !File.Exists(strSH)
                    || strSZ.Length <= 0 || !File.Exists(strSZ))
                {
                    Common.Log("--- Illegal Configuration Settings [Quotation:Shanghai/Shenzhen]." + Environment.NewLine +
                        "Using Last Correct Setting [Quotation:Shanghai = " + Common.strSHQuotation + "]." + Environment.NewLine +
                        "Using Last Correct Setting [Quotation:Shenzhen = " + Common.strSZQuotation + "].");
                    Program.SetVal("Quotation", "Shanghai", Common.strSHQuotation.Trim());
                    Program.SetVal("Quotation", "Shenzhen", Common.strSZQuotation.Trim());
                }
                else
                {
                    strSH = Common.strSHQuotation;
                    strSZ = Common.strSZQuotation;
                }

                #region Make Orders Interfaces
                string strReq = Common.Config("Interface", "NewUser").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_NewUser)
                        Common.Log(" *** Interface-NewUser [Closed] *** ");
                    Common.Switch_NewUser = false;
                }
                else
                {
                    if (!Common.Switch_NewUser)
                        Common.Log(" *** Interface-NewUser [Opened] *** ");
                    Common.Switch_NewUser = true;
                }

                strReq = Common.Config("Interface", "ImmediateOrder").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_ImmediateOrder)
                        Common.Log(" *** Interface-ImmediateOrder [Closed] *** ");
                    Common.Switch_ImmediateOrder = false;
                }
                else
                {
                    if (!Common.Switch_ImmediateOrder)
                        Common.Log(" *** Interface-ImmediateOrder [Opened] *** ");
                    Common.Switch_ImmediateOrder = true;
                }

                strReq = Common.Config("Interface", "LimitedOrder").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_LimitedOrder)
                        Common.Log(" *** Interface-LimitedOrder [Closed] *** ");
                    Common.Switch_LimitedOrder = false;
                }
                else
                {
                    if (!Common.Switch_LimitedOrder)
                        Common.Log(" *** Interface-LimitedOrder [Opened] *** ");
                    Common.Switch_LimitedOrder = true;
                }

                strReq = Common.Config("Interface", "CancelOrder").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_CancelOrder)
                        Common.Log(" *** Interface-CancelOrder [Closed] *** ");
                    Common.Switch_CancelOrder = false;
                }
                else
                {
                    if (!Common.Switch_CancelOrder)
                        Common.Log(" *** Interface-CancelOrder [Opened] *** ");
                    Common.Switch_CancelOrder = true;
                }
                #endregion
                #region Request Info Interfaces
                strReq = Common.Config("Interface", "UserFund").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserFund)
                        Common.Log(" *** Interface-UserFund [Closed] *** ");
                    Common.Switch_UserFund = false;
                }
                else
                {
                    if (!Common.Switch_UserFund)
                        Common.Log(" *** Interface-UserFund [Opened] *** ");
                    Common.Switch_UserFund = true;
                }

                strReq = Common.Config("Interface", "UserOrders").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserOrders)
                        Common.Log(" *** Interface-UserOrders [Closed] *** ");
                    Common.Switch_UserOrders = false;
                }
                else
                {
                    if (!Common.Switch_UserOrders)
                        Common.Log(" *** Interface-UserOrders [Opened] *** ");
                    Common.Switch_UserOrders = true;
                }

                strReq = Common.Config("Interface", "UserStocks").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserStocks)
                        Common.Log(" *** Interface-UserStocks [Closed] *** ");
                    Common.Switch_UserStocks = false;
                }
                else
                {
                    if (!Common.Switch_UserStocks)
                        Common.Log(" *** Interface-UserStocks [Opened] *** ");
                    Common.Switch_UserStocks = true;
                }

                strReq = Common.Config("Interface", "UserTrades").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserTrades)
                        Common.Log(" *** Interface-UserTrades [Closed] *** ");
                    Common.Switch_UserTrades = false;
                }
                else
                {
                    if (!Common.Switch_UserTrades)
                        Common.Log(" *** Interface-UserTrades [Opened] *** ");
                    Common.Switch_UserTrades = true;
                }

                strReq = Common.Config("Interface", "UserFundChanges").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_UserFundChanges)
                        Common.Log(" *** Interface-UserFundChanges [Closed] *** ");
                    Common.Switch_UserFundChanges = false;
                }
                else
                {
                    if (!Common.Switch_UserFundChanges)
                        Common.Log(" *** Interface-UserFundChanges [Opened] *** ");
                    Common.Switch_UserFundChanges = true;
                }
                #endregion
                #region Management Interfaces
                strReq = Common.Config("Interface", "ServiceStatus").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_ServiceStatus)
                        Common.Log(" *** Interface-ServiceStatus [Closed] *** ");
                    Common.Switch_ServiceStatus = false;
                }
                else
                {
                    if (!Common.Switch_ServiceStatus)
                        Common.Log(" *** Interface-ServiceStatus [Opened] *** ");
                    Common.Switch_ServiceStatus = true;
                }

                strReq = Common.Config("Interface", "ServiceConfiguration").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_Configuration)
                        Common.Log(" *** Interface-ServiceConfiguration [Closed] *** ");
                    Common.Switch_Configuration = false;
                }
                else
                {
                    if (!Common.Switch_Configuration)
                        Common.Log(" *** Interface-ServiceConfiguration [Opened] *** ");
                    Common.Switch_Configuration = true;
                }

                strReq = Common.Config("Interface", "ServiceMaintain").ToUpper().Trim();
                if (strReq == "0" || strReq == "FALSE" || strReq == "F" ||
                    strReq == "NO" || strReq == "N" || strReq == "CLOSE" || strReq == "CLOSED")
                {
                    if (Common.Switch_Maintain)
                        Common.Log(" *** Interface-ServiceMaintain [Closed] *** ");
                    Common.Switch_Maintain = false;
                }
                else
                {
                    if (!Common.Switch_Maintain)
                        Common.Log(" *** Interface-ServiceMaintain [Opened] *** ");
                    Common.Switch_Maintain = true;
                }
                #endregion
            }
            catch (Exception err)
            {
                Common.Log(err);
            }
        }
    }
}
