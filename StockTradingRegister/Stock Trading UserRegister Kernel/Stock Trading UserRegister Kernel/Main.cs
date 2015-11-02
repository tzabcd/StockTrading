using System;
using System.IO;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Net;
using System.Xml;

namespace Stock_Trading_UserRegister_Kernel
{

    #region Remoting通道设置
    public class ClientIPServerSinkProvider : IServerChannelSinkProvider
    {
        private IServerChannelSinkProvider next = null;
        private Common coreConfig = new Common();

        public ClientIPServerSinkProvider(IDictionary properties, ICollection providerData)
        { }

        public void GetChannelData(IChannelDataStore channelData)
        {
        }
        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            IServerChannelSink nextSink = null;
            if (next != null)
            {
                nextSink = next.CreateSink(channel);
            }
            return new ClientIPServerSink(nextSink);
        }
        public IServerChannelSinkProvider Next
        {
            get { return next; }
            set { next = value; }
        }
    }
    public class ClientIPServerSink : BaseChannelObjectWithProperties, IServerChannelSink, IChannelSinkBase
    {
        private IServerChannelSink _next;
        private IPAddress ip = IPAddress.Parse("127.0.0.1");
        public ClientIPServerSink(IServerChannelSink next)
        {
            _next = next;
        }
        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
        {
        }
        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public ServerProcessing ProcessMessage(
            IServerChannelSinkStack sinkStack,
            IMessage requestMsg,
            ITransportHeaders requestHeaders,
            Stream requestStream,
            out IMessage responseMsg,
            out ITransportHeaders responseHeaders,
            out Stream responseStream)
        {
            try
            {
                if (_next != null)
                {
                    ip = (IPAddress)requestHeaders[CommonTransportKeys.IPAddress];
                    //if (ip != null && (ip.ToString().Trim() == "127.0.0.1" || CheckAuthorizedIP(ip.ToString())))
                    {
                        ServerProcessing spres = _next.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);
                        return spres;
                    }
                    //else
                    //{
                    //    Loger.Debug("Unauthorized Remoting IP: " + ip.ToString().Trim());
                    //    responseMsg = null;
                    //    responseHeaders = null;
                    //    responseStream = null;
                    //    return new ServerProcessing();
                    //}
                }
                else
                {
                    responseMsg = null;
                    responseHeaders = null;
                    responseStream = null;
                    return new ServerProcessing();
                }
            }
            catch (Exception err)
            {
                responseMsg = null;
                responseHeaders = null;
                responseStream = null;
                return new ServerProcessing();
            }
        }

        public IServerChannelSink NextChannelSink
        {
            get { return _next; }
            set { _next = value; }
        }
        private bool CheckAuthorizedIP(string strIP)
        {
            try
            {
                if (strIP == null)
                    return false;
                //for (byte i = 0; i < 10; i++)
                //{
                //    if (string.Compare(strIP.Trim(),
                //        Common.Config("Authorization", "IP_" + i.ToString().Trim())) == 0)
                //        return true;
                //}
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    #endregion



    public class Common
    {
        public static RegisterSystem urkRegister = null; //注册系统
        public static UserBuffer userBuffer = null; //用户缓存

        public static Synchronizer DBSync = null; //同步系统

        public static DateTime MaxDateTime = new DateTime(2099, 12, 31, 23, 59, 59);
        public static DateTime MinDateTime = new DateTime(1901, 1, 1, 0, 0, 0);
 
        public static string strConn = "";

        //撮合系统口列表
        public static Dictionary<int, Dictionary<string, EastMoney.StockIndexTrader.RemotingProvider.ITransactionRemotingProvider>> znRmtIobj
            = new Dictionary<int, Dictionary<string, EastMoney.StockIndexTrader.RemotingProvider.ITransactionRemotingProvider>>();
 
        public static bool Debug()
        {
            Console.WriteLine("Debug start ... ");
            Initialize();

            while(true)
            {
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            try
            {
                Console.WriteLine("Common Initialize ... ");
                RemotingConfiguration.Configure(Process.GetCurrentProcess().MainModule.FileName + ".config", false);

                Common.strConn = BaseConfig.ConnStr.Trim();

                //初始化撮合系统接口列表中的各接口
                foreach (KeyValuePair<int, Dictionary<string, NotifySrv>> s in BaseConfig.mapNotifySrv)
                {
                    foreach (KeyValuePair<string, NotifySrv> t in s.Value)
                    {
                        if (!znRmtIobj.ContainsKey(s.Key))
                        {
                            znRmtIobj.Add(s.Key, new Dictionary<string, EastMoney.StockIndexTrader.RemotingProvider.ITransactionRemotingProvider>());
                        }
                        if (znRmtIobj[s.Key].ContainsKey(t.Key))
                        {
                            znRmtIobj[s.Key][t.Key] = Activator.GetObject(typeof(EastMoney.StockIndexTrader.RemotingProvider.ITransactionRemotingProvider), BaseConfig.mapNotifySrv[s.Key][t.Key].ri) as EastMoney.StockIndexTrader.RemotingProvider.ITransactionRemotingProvider;
                        }
                        else
                        {
                            znRmtIobj[s.Key].Add(t.Key, Activator.GetObject(typeof(EastMoney.StockIndexTrader.RemotingProvider.ITransactionRemotingProvider), BaseConfig.mapNotifySrv[s.Key][t.Key].ri) as EastMoney.StockIndexTrader.RemotingProvider.ITransactionRemotingProvider);
                        }
                    }
                }

                //初始化缓存
                Common.userBuffer = new UserBuffer();
                if (Common.userBuffer.Initialize())
                {
                    Loger.Debug("Interface Buffer Initialized");
                }
                else
                {
                    Loger.Debug("Failed to Initialize The Interface Buffer");
                    return false;
                }
                Console.WriteLine("userBuffer Initialize Successed ... ");

                //初始化注册系统
                Common.urkRegister = new RegisterSystem();
                if (!Common.urkRegister.Initialize())
                {
                    return false;
                }
                Console.WriteLine("RegisterSystem Initialize Successed ... ");
                Console.WriteLine("Interface System Created ");
                Console.WriteLine("<<< Configuration Settings Loaded. bAllowReg = " + BaseConfig.bAllowReg.ToString());
                
                Loger.Debug("Interface System Created ");
                Loger.Debug("<<< Configuration Settings Loaded. bAllowReg = " + BaseConfig.bAllowReg.ToString());
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err.ToString());
                return false;
            }
        }

        public static void Uninitialize()
        {
            try
            {
                if (urkRegister != null)
                    urkRegister.Uninitialize();
            }
            catch (Exception err)
            {
                Loger.Debug(err.ToString());
            }
        }

 
         
    }
}
