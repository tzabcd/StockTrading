using System;
using System.ServiceProcess;
using System.Threading;
namespace CalcDailyRank
{
    /// <summary>
    /// ManagerAgent 的摘要说明。
    /// </summary>
    public class ManagerAgent : ServiceBase
    {
        private static string ServiceName = "Stock Trade UserRanker";

        private static string DisplayName = "Stock Trade UserRanker";

        private static string Description = "模拟炒股排名服务";

        protected override void OnStart(string[] args)
        {
            Common.Initialize();
        }

        protected override void OnStop()
        {
            Common.Uninitialize();
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                WindowsServiceInstaller srv = new WindowsServiceInstaller();

                if (args.Length == 2)
                {
                    ServiceName += " " + args[1];
                    DisplayName += " " + args[1];
                    Description += " " + args[1];
                }

                switch (args[0].ToLower())
                {
                    case "/r":
                        Common.Debug();
                        return;
                    case "/i":
                        Loger.Debug("Install service start ...");
                        Install(srv);
                        return;
                    case "/u":
                        Loger.Debug("Uninstall service start ...");
                        Uninstall(srv);
                        return;
                }
            }
            else
            {
                try
                {
                    Go();
                }
                catch (Exception ex)
                {
                    Loger.Debug("ManagerAgent::" + ex.Message);
                }
            }

            help();
        }

        private static void help()
        {
            Console.WriteLine("Command line parameters:");

            Console.WriteLine("\t/r\tDebug the service");

            Console.WriteLine("\t/i\tInstalls the service");

            Console.WriteLine("\t/u\tUnstalls the service");
        }

        private static void Install(WindowsServiceInstaller srv)
        {
            bool ok = srv.InstallService(System.Reflection.Assembly.GetExecutingAssembly().Location, ServiceName, DisplayName, Description);

            if (ok)
            {
                Console.WriteLine("Service installed.");
                Loger.Debug("Service installed.");
            }
            else
            {
                Console.WriteLine("There was a problem with installation.");
                Loger.Debug("There was a problem with installation.");
            }
        }

        private static void Uninstall(WindowsServiceInstaller srv)
        {
            bool ok = srv.UnInstallService(ServiceName);

            if (ok)
            {
                Console.WriteLine("Service uninstalled.");
                Loger.Debug("Service uninstalled.");
            }
            else
            {
                Console.WriteLine("There was a problem with uninstallation.");
                Loger.Debug("There was a problem with uninstallation.");
            }
        }

        private static void Go()
        {
            ServiceBase.Run(new ManagerAgent());
        }
    }
}
