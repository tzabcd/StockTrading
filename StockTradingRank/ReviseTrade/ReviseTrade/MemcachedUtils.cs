using System;
using System.Collections;
using Memcached.ClientLibrary;

namespace ReviseTrade
{
    public class MemCachedUtils
    {
        public static MemcachedClient mc;

        public static bool Initialize()
        {
            try
            {
                string[] serverlist = { "127.0.0.1" };

                //初始化池
                SockIOPool pool = SockIOPool.GetInstance();
                pool.SetServers(serverlist);

                pool.InitConnections = 3;
                pool.MinConnections = 3;
                pool.MaxConnections = 250;

                pool.SocketConnectTimeout = 1000;
                pool.SocketTimeout = 3000;

                pool.MaintenanceSleep = 30;
                pool.Failover = true;

                pool.Nagle = false;
                pool.Initialize();


                mc = new MemcachedClient();
                mc.EnableCompression = false;

               Loger.Debug(" memcached client Initialized ");
                System.Collections.Hashtable hsStats = new System.Collections.Hashtable(mc.Stats());
                foreach (DictionaryEntry s in hsStats)
                {
                    Loger.Debug(s.Key.ToString());
                    foreach (DictionaryEntry t in (Hashtable)s.Value)
                    {
                        Loger.Debug(t.Key.ToString() + " : " + t.Value.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Loger.Debug("MemcachedClient Initialize error : " + ex.ToString());
                return false;
            }
        }

        public static bool UnInitialize()
        {
            if (SockIOPool.GetInstance() != null)
                SockIOPool.GetInstance().Shutdown();

            return true;
        }
    }
}
