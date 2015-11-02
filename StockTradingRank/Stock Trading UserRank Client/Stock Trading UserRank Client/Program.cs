using System;
using System.Collections.Generic;
using System.Configuration;
using zns = Stock_Trading_Simulator_Kernel;

namespace Stock_Trading_UserRank_Client
{

    class Common
    {
        public static string url = ConfigurationManager.AppSettings["url"];
        public static zns.RemotingInterface znRmtIobj = Activator.GetObject(typeof(zns.RemotingInterface), url) as zns.RemotingInterface;
        public static string strRemoteKey = "testRemoteKey";

        static void Main(string[] args)
        {
            int userId = 38071;
            try
            {
                #region 各币种股票总市值
                double StocksFundRMB = 0;
                double StocksFundUSD = 0;
                double StocksFundHKD = 0;
                double tempOneStockFund = 0;
                double StockWealth = 0;

                List<zns.RemotingInterface.RI_Stock> listStock = new List<Stock_Trading_Simulator_Kernel.RemotingInterface.RI_Stock>();
                listStock = Common.znRmtIobj.RequestUserStocks("L0ca1", userId);
                if (listStock != null)
                {
                    foreach (zns.RemotingInterface.RI_Stock stock in listStock)
                    {
                        tempOneStockFund = 0;
                        tempOneStockFund = ConvertPrice(stock.AveragePrice) * stock.Volume;

                        switch (stock.Curr)
                        {
                            case zns.RemotingInterface.RI_Currency.RMB:
                                StocksFundRMB += tempOneStockFund; break;

                            case zns.RemotingInterface.RI_Currency.USD:
                                StocksFundUSD += tempOneStockFund; break;

                            case zns.RemotingInterface.RI_Currency.HKD:
                                StocksFundHKD += tempOneStockFund; break;
                        }

                        Console.WriteLine(stock.StockCode + ":" + stock.AveragePrice + "\t" + stock.Volume);
                    }
                    StockWealth = StocksFundRMB + StocksFundUSD * 1 + StocksFundHKD * 1;
                }
                else
                {
                    Console.WriteLine("没有持股");
                }
                #endregion

                #region 各币种现金
                double CashRMB = 0;
                double CashUSD = 0;
                double CashHKD = 0;

                Dictionary<byte, zns.RemotingInterface.RI_Fund> mapUserFund = new Dictionary<byte, Stock_Trading_Simulator_Kernel.RemotingInterface.RI_Fund>();
                mapUserFund = Common.znRmtIobj.RequestUserFund("L0ca1", userId);
                if (mapUserFund != null)
                {
                    foreach (KeyValuePair<byte, zns.RemotingInterface.RI_Fund> fund in mapUserFund)
                    {
                        switch ((zns.RemotingInterface.RI_Currency)fund.Key)
                        {
                            case zns.RemotingInterface.RI_Currency.RMB:
                                CashRMB = fund.Value.Cash; break;

                            case zns.RemotingInterface.RI_Currency.USD:
                                CashUSD = fund.Value.Cash; break;

                            case zns.RemotingInterface.RI_Currency.HKD:
                                CashHKD = fund.Value.Cash; break;
                        }
                        Console.WriteLine(fund.Key.ToString() + ":" + fund.Value.Cash);
                    }
                }
                else
                {
                    Console.WriteLine("现金为空");
                }
                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }

        public static double ConvertPrice(double Price)
        {
            try
            {
                long tmp1 = 1000;
                long tmp2 = (long)(Price * tmp1);
                return (double)tmp2 / (double)tmp1;
            }
            catch
            {
                return Price;
            }
        }
    }


}
