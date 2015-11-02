using System;
using System.Runtime.InteropServices;

namespace CalcDailyRank
{
    public class WindowsServiceInstaller
    {
        #region API函数定义
        [DllImport("advapi32.dll")]
        public static extern System.IntPtr OpenSCManager(String lpMachineName, String lpDatabaseName, UInt32 dwDesiredAccess);
        [DllImport("advapi32.dll", EntryPoint = "CreateServiceA")]
        public static extern System.IntPtr CreateService(IntPtr hSCManager, String lpServiceName, String lpDisplayName, UInt32 dwDesiredAccess, UInt32 dwServiceType, UInt32 dwStartType, UInt32 dwErrorControl, String lpBinaryPathName, String lpLoadOrderGroup, IntPtr lpdwTagId, String lpDependencies, String lpServiceStartName, String lpPassword);
        [DllImport("advapi32.dll")]
        public static extern System.Boolean CloseServiceHandle(IntPtr hSCObject);
        [DllImport("advapi32.dll")]
        public static extern System.IntPtr OpenService(IntPtr hSCManager, String lpServiceName, UInt32 dwDesiredAccess);
        [DllImport("advapi32.dll")]
        public static extern System.Boolean DeleteService(IntPtr hService);
        [DllImport("advapi32.dll")]
        public static extern System.Boolean ChangeServiceConfig2(IntPtr hService, UInt32 dwInfoLevel, ref String lpInfo);
        #endregion

        #region 变量定义
        public const System.UInt32 STANDARD_RIGHTS_REQUIRED = 0xF0000;
        public const System.UInt32 SC_MANAGER_CONNECT = 0x0001;
        public const System.UInt32 SC_MANAGER_CREATE_SERVICE = 0x0002;
        public const System.UInt32 SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
        public const System.UInt32 SC_MANAGER_LOCK = 0x0008;
        public const System.UInt32 SC_MANAGER_QUERY_LOCK_STATUS = 0x0010;
        public const System.UInt32 SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020;
        public const System.UInt32 SC_MANAGER_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED |
          SC_MANAGER_CONNECT |
          SC_MANAGER_CREATE_SERVICE |
          SC_MANAGER_ENUMERATE_SERVICE |
          SC_MANAGER_LOCK |
          SC_MANAGER_QUERY_LOCK_STATUS |
          SC_MANAGER_MODIFY_BOOT_CONFIG;
        // Service object specific access type
        public const System.UInt32 SERVICE_QUERY_CONFIG = 0x0001;
        public const System.UInt32 SERVICE_CHANGE_CONFIG = 0x0002;
        public const System.UInt32 SERVICE_QUERY_STATUS = 0x0004;
        public const System.UInt32 SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
        public const System.UInt32 SERVICE_START = 0x0010;
        public const System.UInt32 SERVICE_STOP = 0x0020;
        public const System.UInt32 SERVICE_PAUSE_CONTINUE = 0x0040;
        public const System.UInt32 SERVICE_INTERROGATE = 0x0080;
        public const System.UInt32 SERVICE_USER_DEFINED_CONTROL = 0x0100;
        public const System.UInt32 SERVICE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED |
          SERVICE_QUERY_CONFIG |
          SERVICE_CHANGE_CONFIG |
          SERVICE_QUERY_STATUS |
          SERVICE_ENUMERATE_DEPENDENTS |
          SERVICE_START |
          SERVICE_STOP |
          SERVICE_PAUSE_CONTINUE |
          SERVICE_INTERROGATE |
          SERVICE_USER_DEFINED_CONTROL;
        // service type
        public const System.UInt32 SERVICE_KERNEL_DRIVER = 0x00000001;
        public const System.UInt32 SERVICE_FILE_SYSTEM_DRIVER = 0x00000002;
        public const System.UInt32 SERVICE_ADAPTER = 0x00000004;
        public const System.UInt32 SERVICE_RECOGNIZER_DRIVER = 0x00000008;
        public const System.UInt32 SERVICE_DRIVER = SERVICE_KERNEL_DRIVER |
          SERVICE_FILE_SYSTEM_DRIVER |
          SERVICE_RECOGNIZER_DRIVER;
        public const System.UInt32 SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        public const System.UInt32 SERVICE_WIN32_SHARE_PROCESS = 0x00000020;
        public const System.UInt32 SERVICE_WIN32 = SERVICE_WIN32_OWN_PROCESS |
          SERVICE_WIN32_SHARE_PROCESS;
        public const System.UInt32 SERVICE_INTERACTIVE_PROCESS = 0x00000100;
        public const System.UInt32 SERVICE_TYPE_ALL = SERVICE_WIN32 |
          SERVICE_ADAPTER |
          SERVICE_DRIVER |
          SERVICE_INTERACTIVE_PROCESS;
        // Start Type
        public const System.UInt32 SERVICE_BOOT_START = 0x00000000;
        public const System.UInt32 SERVICE_SYSTEM_START = 0x00000001;
        public const System.UInt32 SERVICE_AUTO_START = 0x00000002;
        public const System.UInt32 SERVICE_DEMAND_START = 0x00000003;
        public const System.UInt32 SERVICE_DISABLED = 0x00000004;
        // Error control type
        public const System.UInt32 SERVICE_ERROR_IGNORE = 0x00000000;
        public const System.UInt32 SERVICE_ERROR_NORMAL = 0x00000001;
        public const System.UInt32 SERVICE_ERROR_SEVERE = 0x00000002;
        public const System.UInt32 SERVICE_ERROR_CRITICAL = 0x00000003;
        // Info levels for ChangeServiceConfig2 and QueryServiceConfig2
        public const System.UInt32 SERVICE_CONFIG_DESCRIPTION = 1;
        public const System.UInt32 SERVICE_CONFIG_FAILURE_ACTIONS = 2;
        #endregion

        #region 方法定义
        /// <summary>
        /// 安装服务
        /// </summary>
        /// <param name="svcPath">服务路径</param>
        /// <param name="svcName">服务名称</param>
        /// <param name="svcDispName">服务描述</param>
        /// <returns></returns>
        public bool InstallService(string svcPath, string svcName, string svcDisplayName, String svcDescription)
        {
            Boolean bRet = false;
            IntPtr hServiceManager = IntPtr.Zero;
            IntPtr hService = IntPtr.Zero;
            try
            {
                hServiceManager = OpenSCManager(Environment.MachineName, null, SC_MANAGER_ALL_ACCESS);
                if (hServiceManager != IntPtr.Zero)
                {
                    hService = CreateService(hServiceManager,
                        svcName,
                        svcDisplayName,
                        SERVICE_ALL_ACCESS,
                        SERVICE_WIN32_OWN_PROCESS | SERVICE_INTERACTIVE_PROCESS,
                        SERVICE_DEMAND_START,
                        SERVICE_ERROR_NORMAL,
                        svcPath,
                        null,
                        IntPtr.Zero,
                        null,
                        null,
                        null);
                    if (hService != IntPtr.Zero)
                    {
                        bRet = ChangeServiceConfig2(hService, SERVICE_CONFIG_DESCRIPTION, ref svcDescription);
                        CloseServiceHandle(hService);
                        hService = IntPtr.Zero;
                    }
                }
                CloseServiceHandle(hServiceManager);
                hServiceManager = IntPtr.Zero;
            }
            catch
            {
                bRet = false;
            }
            return bRet;
        }
        /// <summary>
        /// 删除服务
        /// </summary>
        /// <param name="svcName">服务名称</param>
        /// <returns></returns>
        public bool UnInstallService(string svcName)
        {
            Boolean bRet = false;
            IntPtr hServiceManager = IntPtr.Zero;
            IntPtr hService = IntPtr.Zero;
            try
            {
                hServiceManager = OpenSCManager(Environment.MachineName, null, SC_MANAGER_ALL_ACCESS);
                if (hServiceManager != IntPtr.Zero)
                {
                    hService = OpenService(hServiceManager, svcName, SERVICE_ALL_ACCESS);
                    if (hService != IntPtr.Zero)
                    {
                        bRet = DeleteService(hService);
                        CloseServiceHandle(hService);
                        hService = IntPtr.Zero;
                    }
                }
                CloseServiceHandle(hService);
                hService = IntPtr.Zero;
            }
            catch
            {
                bRet = false;
            }
            return bRet;
        }
        #endregion
    }
}
