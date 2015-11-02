using System;

namespace CalcDailyRank
{
    class Common
    {
        private static RankSystem rankSystem =null;

        public static bool Initialize()
        {
            try
            {
                rankSystem = new RankSystem();
                if (rankSystem.Initialize())
                {
                    Loger.Debug("--- Rank System Initialized. ---");
                }
            }
            catch (Exception ex)
            {
                Loger.Debug("---error :" + ex.ToString());
            }
            return true;
        }

        public static void Debug()
        {
            Loger.Debug("--- Debug Start ---");
            Initialize();
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static bool Uninitialize()
        {
            if (rankSystem != null)
                rankSystem.Uninitialize();
            return false;
        }
         
    }
}
