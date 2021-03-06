﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CalcDurationRatioRank
{
    public class Loger
    {
        #region loger
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Debug(object msg)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/log/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log", true, System.Text.Encoding.Default))
                {
                    Console.WriteLine(msg.ToString());
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : " + msg.ToString());
                    sw.Close();
                }
            }
            catch
            {

            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Debug(string path, object msg)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path + DateTime.Now.ToString("yyyy-MM-dd") + ".log", true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : " + msg.ToString());
                    sw.Close();
                }
            }
            catch
            {

            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void State(string msg)
        {
            using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "State.log", false, System.Text.Encoding.Default))
            {
                sw.WriteLine(msg);
            }
        }
        #endregion
    }
}
