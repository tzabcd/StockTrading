using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Stock_Trading_Simulator_Kernel;
using Network_Service_Core;
namespace Stock_Trading_Simulator_Shell
{
    public static class Program
    {
        private static MainService MainSvc = new MainService();
        private static Core_ConfigAndLogger CoreConfig = new Core_ConfigAndLogger();
        public static void Main(string[] args)
        {
            try
            {
                
                string strID = "";
                if (args.Length > 1 && args[1] != null && args[1].Trim().Length > 0)
                    strID = args[1].Trim();
                if (args.Length > 0 && args[0] != null && args[0].Trim().ToUpper() == "/INSTALL")
                {
                    
                    if (Core_ServiceInstaller.Install(strID))
                        CoreConfig.Log("The service [" + Core_ServiceInstaller.ServiceName.Trim() + "] has been installed.");
                    else
                        CoreConfig.Log("The service [" + Core_ServiceInstaller.ServiceName.Trim() + "] has NOT been installed.");
                    Environment.Exit(Environment.ExitCode);
                }
                else if (args.Length > 0 && args[0] != null && args[0].Trim().ToUpper() == "/UNINSTALL")
                {
                    if (Core_ServiceInstaller.Uninstall())
                        CoreConfig.Log("The service [" + Core_ServiceInstaller.ServiceName.Trim() + "] has been uninstalled.");
                    else
                        CoreConfig.Log("The service [" + Core_ServiceInstaller.ServiceName.Trim() + "] has NOT been uninstalled.");
                    Environment.Exit(Environment.ExitCode);
                }
                else if (args != null && args.Length > 0 && args[0].Trim().ToUpper() == "/DEBUG")
                {
                    Core_ServiceInstaller.SetConsole(MainSvc);
                    MainSvc.StartFromConsole(args);
                }
                else
                {
                    ServiceBase[] ServicesToRun = new ServiceBase[] { MainSvc };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception err)
            {
                CoreConfig.Log(err);
                CoreConfig.Log("Failed to start the service.");
                System.Environment.Exit(0);
            }
        }
        public static void Log()
        {
            try
            {
                Common.Log();
                Console.WriteLine("* * * * [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] * * * *");
            }
            catch
            { }
        }
        public static void Log(string strValue)
        {
            try
            {
                if (strValue != null)
                {
                    Common.Log(strValue);
                    Console.WriteLine(strValue);
                }
            }
            catch
            { }
        }
        public static void Log(Exception ExValue)
        {
            try
            {
                if (ExValue != null && ExValue.Message != null && ExValue.StackTrace != null)
                {
                    CoreConfig.Log(ExValue.Message + Environment.NewLine + ExValue.StackTrace);
                    Console.WriteLine(ExValue.Message + Environment.NewLine + ExValue.StackTrace);
                }
            }
            catch
            { }
        }
        public static bool SetVal(string Section, string Key, string Value)
        {
            try
            {
                return CoreConfig.SetConfig(Section.Trim(), Key.Trim(), Value.Trim());
            }
            catch
            {
                return false;
            }
        }
    }
}