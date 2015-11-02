using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace Stock_Trading_UserRegister_Kernel
{
    /// <summary>
    /// 用户注册系统
    /// </summary>
   public class RegisterSystem
    {
       //新用户列表
        public List<RemotingInterface.UserInfo> listNewUser = null;

        //主要用于监视初始化时加载用户的过程，无实际意义。
        public Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>> mapUser = null; 

        private Thread ThTrading = null;
        private bool bRegister = false;

       /// <summary>
       /// 构造，新用户列表初始化
       /// </summary>
        public RegisterSystem()
        {
            listNewUser = new List<RemotingInterface.UserInfo>();
        }

       /// <summary>
       /// 注册服务运转线程函数
       /// </summary>
        private void Registing()
        {

        }

       /// <summary>
        /// 启动注册系统运转
       /// </summary>
       /// <returns></returns>
        public bool Initialize()
        {
            try
            {
                Common.DBSync = new Synchronizer(Common.strConn);
                if (!Common.DBSync.Initialize(ref mapUser) || mapUser == null) //初始化，从数据库加载数据到用户缓存表
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Failed to Create the DBSync System.");
                    Loger.Debug("Failed to Create the DBSync System.");

                    return false;
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": DBSync System Created");
                    Loger.Debug("DBSync System Created");
                }
                bRegister = true;
                ThTrading = new Thread(new ThreadStart(Registing));
                ThTrading.Name = "ThTrading";
                ThTrading.Start();

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Register System Created");
                Loger.Debug("Register System Created");

                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err.ToString());
                Loger.Debug("Failed to Create the Register System.");
                return false;
            }
        }

       /// <summary>
       /// 停止注册系统运转
       /// </summary>
        public void Uninitialize()
        {
            try
            {
                //bTrading = false;
                if (ThTrading != null && ThTrading.IsAlive)
                {
                    ThTrading.Join(4500);
                    if (ThTrading != null && ThTrading.IsAlive)
                        ThTrading.Abort();
                }
            }
            catch (Exception err)
            {
                Loger.Debug(err.ToString());
            }
            finally
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Register System Terminated");
                Loger.Debug("Register System Terminated");
            }
        }
    }
}
