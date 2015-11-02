using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace CalcDurationRank
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
                using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(msg.ToString());
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

        public static void Serialize(String filePath, Object obj)
        {
            if (obj == null)
            {
                Console.WriteLine(" object is null ");
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, obj);
            }
        }

        public static T DeSerialize<T>(string filePath, out Boolean IsExists) where T : class
        {
            IsExists = false;
            if (!File.Exists(filePath))
            {
                return default(T);
            }
            IsExists = true;
            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                Object objValue = formatter.Deserialize(fileStream);

                return objValue as T;
            }
        }
        #endregion
    }
}
