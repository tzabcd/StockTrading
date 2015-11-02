using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Stock_Trading_UserRegister_Kernel;


namespace Stock_Trading_UserRegister_Client
{
    /* remoting调用函数
     *  IsExistAreaUser 判断用户是否存在
     *  RequestNewUser  用户注册
     * 
     */
    class Program
    {
        public static string url = ConfigurationManager.AppSettings["url"];
        public static RemotingInterface remoteobj = Activator.GetObject(typeof(RemotingInterface), url) as RemotingInterface;
        public static string strRemoteKey = "testRemoteKey";

        static void Main(string[] args)
        {
            int playId = 1;
            int gameId = 1;
            string userName = "";
            int nNum = 100;
            for (int i = 0; i < nNum; i++)
            {
                Random rd = new Random();
                int areaId = rd.Next(1, 8);

                userName = GenerateRandom(new Random().Next(6, 12));

                RemotingInterface.UserInfo user = new RemotingInterface.UserInfo();
                user.PlayId = playId;
                user.GameId = gameId;
                user.AreaId = areaId;
                user.UserName = userName;
                user.Rtime = System.DateTime.Now;
                user.RIP = "127.0.0.1";

                if (!remoteobj.IsExistUser(playId, userName))
                {
                    if (!remoteobj.RequestNewUser("", user))
                    {
                        Console.WriteLine("i = " + i + "\tuserName = " + userName + "添加失败");
                    }
                    else
                    {
                        Console.WriteLine("i = " + i + "\tuserName = " + userName + "添加成功");
                    }
                }
                else
                {
                    Console.WriteLine("i = " + i + "\tuserName = " + userName + "用户已存在");
                }

                System.Threading.Thread.Sleep(100);
            }
            Console.ReadKey();
        }

        private static char[] constant = 
{ 
'0','1','2','3','4','5','6','7','8','9', 
'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'
};

        public static string GenerateRandom(int Length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(36)]);
            }
            return newRandom.ToString();
        }
    }


}
