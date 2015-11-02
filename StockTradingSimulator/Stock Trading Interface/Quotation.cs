#if INTERNEL
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using QDBFAnalyzer.StructuredAnalysis;

namespace Stock_Trading_Simulator_Kernel
{
    /// <summary>
    /// DBF文件读取类
    /// </summary>
    public class Quotation
    {
        private Dictionary<string, Show2003DBFRecord> mapSHRates = null; //上证类股票即时行情
        private Dictionary<string, SjshqDBFRecord> mapSZRates = null;//深证类股票即时行情
        private Dictionary<string, Show2003DBFRecord> mapSHSnapShot = null; //上证类股票快照
        private Dictionary<string, SjshqDBFRecord> mapSZSnapShot = null; //深证类股票快照
        private Thread ThReceiving = null;  //接收线程
        private bool bReceiving = false;　　//接收线程运行标志

        /// <summary>
        /// 构造
        /// </summary>
        public Quotation()
        {
            mapSHRates = new Dictionary<string, Show2003DBFRecord>();
            mapSZRates = new Dictionary<string, SjshqDBFRecord>();
            mapSHSnapShot = new Dictionary<string, Show2003DBFRecord>();
            mapSZSnapShot = new Dictionary<string, SjshqDBFRecord>();
        }
        /// <summary>
        /// 初始化,启动接收线程
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            try
            {
                bReceiving = true;
                ThReceiving = new Thread(new ThreadStart(Receiving));
                ThReceiving.Name = "ThReceiving";
                ThReceiving.Start();
                Common.Log("Quotation System Created");
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                Common.Log("Failed to Create the Quotation System.");
                return false;
            }
        }
        /// <summary>
        /// 关闭接收线程
        /// </summary>
        public void Uninitialize()
        {
            try
            {
                bReceiving = false;
                if (ThReceiving != null && ThReceiving.IsAlive)
                {
                    ThReceiving.Join(1000);
                    if (ThReceiving != null && ThReceiving.IsAlive)
                        ThReceiving.Abort();
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
            }
            finally
            {
                Common.Log("Quotation System Terminated");
            }
        }
        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            try
            {
                if (!Common.IsWeekend && Common.IsInStockQuotationTime)
                    return false;

                lock (mapSHRates)
                {
                    if (mapSHRates == null)
                        mapSHRates = new Dictionary<string, Show2003DBFRecord>();
                    else
                        mapSHRates.Clear();
                }

                lock (mapSZRates)
                {
                    if (mapSZRates == null)
                        mapSZRates = new Dictionary<string, SjshqDBFRecord>();
                    else
                        mapSZRates.Clear();
                }

                lock (mapSHSnapShot)
                {
                    if (mapSHSnapShot == null)
                        mapSHSnapShot = new Dictionary<string, Show2003DBFRecord>();
                    else
                        mapSHSnapShot.Clear();
                }

                lock (mapSZSnapShot)
                {
                    if (mapSZSnapShot == null)
                        mapSZSnapShot = new Dictionary<string, SjshqDBFRecord>();
                    else
                        mapSZSnapShot.Clear();
                }

                Common.Log("Quotation System Buffer Cleared");
                return true;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }
        /// <summary>
        /// 获取快照，存储片断
        /// </summary>
        /// <returns></returns>
        public bool GetSnapShot()
        {
            try
            {
                lock (mapSHRates)
                {
                    if (mapSHRates != null && mapSHRates.Count > 0)
                    {
                        lock (mapSHSnapShot)
                        {
                            mapSHSnapShot.Clear();
                            mapSHSnapShot = new Dictionary<string, Show2003DBFRecord>(mapSHRates);
                        }
                    }
                }
                lock (mapSZRates)
                {
                    if (mapSZRates != null && mapSZRates.Count > 0)
                    {
                        lock (mapSZSnapShot)
                        {
                            mapSZSnapShot.Clear();
                            mapSZSnapShot = new Dictionary<string, SjshqDBFRecord>(mapSZRates);
                        }
                    }
                }
                if (mapSHSnapShot != null && mapSHSnapShot.Count > 0
                    && mapSZSnapShot != null && mapSZSnapShot.Count > 0)
                {
                    foreach (object objSh in mapSHSnapShot.Keys)
                    {
                        if (objSh == null || objSh.ToString() == null)
                            continue;
                        string strKey = objSh.ToString().Trim();
                        if (!mapSHSnapShot.ContainsKey(strKey))
                            continue;
                        TradingSystem.StockType sType = Common.stkTrading.GetStockType(strKey, TradingSystem.StockMarket.Shanghai);
                        if (sType != TradingSystem.StockType.SH_A
                            && sType != TradingSystem.StockType.ETF
                            && sType != TradingSystem.StockType.SH_ClosedFund
                            && sType != TradingSystem.StockType.SH_Warrant
                            && sType != TradingSystem.StockType.SH_B
                            && sType != TradingSystem.StockType.SH_Bond)
                            continue;
                        SaveQuotation(mapSHSnapShot[strKey]);//存储上证类股票
                    }
                    foreach (object objSh in mapSZSnapShot.Keys)
                    {
                        if (objSh == null || objSh.ToString() == null)
                            continue;
                        string strKey = objSh.ToString().Trim();
                        if (!mapSZSnapShot.ContainsKey(strKey))
                            continue;
                        TradingSystem.StockType sType = Common.stkTrading.GetStockType(strKey, TradingSystem.StockMarket.Shenzhen);
                        if (sType != TradingSystem.StockType.SZ_A
                            && sType != TradingSystem.StockType.ETF
                            && sType != TradingSystem.StockType.SZ_ClosedFund
                            && sType != TradingSystem.StockType.SZ_Warrant
                            && sType != TradingSystem.StockType.SZ_B
                            && sType != TradingSystem.StockType.SZ_Bond)
                            continue;
                        SaveQuotation(mapSZSnapShot[strKey]);//存储深证类股票
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                Common.Log(err);
                return false;
            }
        }
        /// <summary>
        /// 获取即时行情
        /// </summary>
        /// <param name="StockCode">股票代码</param>
        /// <param name="Market">市场</param>
        /// <param name="SHRecord">上证类股票行情</param>
        /// <param name="SZRecord">深证类股票行情</param>
        /// <returns></returns>
        public bool GetStkRate(string StockCode, TradingSystem.StockMarket Market,
            out Show2003DBFRecord SHRecord, out SjshqDBFRecord SZRecord)
        {
            SHRecord = new Show2003DBFRecord();
            SZRecord = new SjshqDBFRecord();
            try
            {
                SHRecord.Clear(); SZRecord.Clear();
                if (StockCode == null || StockCode.Trim().Length <= 0 || Market == TradingSystem.StockMarket.Unknown)
                    return false;
                //上证类股票
                if (Market == TradingSystem.StockMarket.Shanghai)
                {
                    lock (mapSHSnapShot)
                    {
                        if (mapSHSnapShot == null || mapSHSnapShot.Count <= 0)
                            return false;
                        if (mapSHSnapShot.ContainsKey(StockCode.Trim()))
                        {
                            SHRecord = mapSHSnapShot[StockCode.Trim()];
                            return true;
                        }
                    }
                }
                //深证类股票
                else if (Market == TradingSystem.StockMarket.Shenzhen)
                {
                    lock (mapSZSnapShot)
                    {
                        if (mapSZSnapShot == null || mapSZSnapShot.Count <= 0)
                            return false;
                        else if (mapSZSnapShot.ContainsKey(StockCode.Trim()))
                        {
                            SZRecord = mapSZSnapShot[StockCode.Trim()];
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception err)
            {
                Common.Log(err);
                SHRecord.Clear();
                SZRecord.Clear();
                return false;
            }
        }

        /// <summary>
        /// 是否停牌或退市
        /// </summary>
        /// <param name="StockCode">股票代码</param>
        /// <param name="Market">市场</param>
        /// <returns></returns>
        public bool CheckSuspended(string StockCode, TradingSystem.StockMarket Market)
        {
            try
            {
                if (StockCode == null || StockCode.Trim().Length != 6)
                    return true;
                //上证类处理
                if (Market == TradingSystem.StockMarket.Shanghai)
                {
                    Dictionary<string, Show2003DBFRecord> mapSHTemp = null;//此变量有啥用？
                    lock (mapSHRates)
                    {
                        mapSHTemp = new Dictionary<string, Show2003DBFRecord>(mapSHRates);
                    }
                    if (!mapSHRates.ContainsKey(StockCode.Trim()))
                        return true;
                    else if (mapSHRates[StockCode.Trim()].OpenPrice < 0.001)
                        return true;
                    else
                        return false;
                }
                //深证类处理
                else if (Market == TradingSystem.StockMarket.Shenzhen)
                {
                    Dictionary<string, SjshqDBFRecord> mapSZTemp = null;
                    lock (mapSZRates)
                    {
                        mapSZTemp = new Dictionary<string, SjshqDBFRecord>(mapSZRates);
                    }
                    if (!mapSZRates.ContainsKey(StockCode.Trim()))
                        return true;
                    else if (mapSZRates[StockCode.Trim()].OpenPrice < 0.001)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 接收行情
        /// </summary>
        private void Receiving()
        {
            try
            {
                //上证DBF
                Show2003Analyzer show2003 = new Show2003Analyzer(Common.strSHQuotation.Trim());
                //深证DBF
                SjshqAnalyzer sjshq = new SjshqAnalyzer(Common.strSZQuotation.Trim());
                //上证类股票DBF即时行情
                Dictionary<string, Show2003DBFRecord> mapSHTemp = new Dictionary<string, Show2003DBFRecord>();
                //深证类股票DBF即时行情
                Dictionary<string, SjshqDBFRecord> mapSZTemp = new Dictionary<string, SjshqDBFRecord>();
                //接收运行中心
                while (bReceiving && mapSHRates != null && mapSZRates != null)
                {
                    try
                    {
                        if (!Management.Work || Common.IsWeekend
                            || DateTime.Now.TimeOfDay < Common.BeginAMTS
                            || DateTime.Now.TimeOfDay > Common.EndPMTS.Add(new TimeSpan(0, 10, 0)))
                        {
                            Thread.Sleep(30000);
                            continue;
                        }

                        mapSHTemp.Clear(); show2003.ReadAllRecords(out mapSHTemp);
                        mapSZTemp.Clear(); sjshq.ReadAllRecords(out mapSZTemp);
                        if (mapSHTemp.Count > 0)
                        {
                            lock (mapSHRates)
                            {
                                mapSHRates = new Dictionary<string, Show2003DBFRecord>(mapSHTemp);
                            }
                        }
                        if (mapSZTemp.Count > 0)
                        {
                            lock (mapSZRates)
                            {
                                mapSZRates = new Dictionary<string, SjshqDBFRecord>(mapSZTemp);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        Common.Log(err);
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
                Common.Log("Error: The Quotation Thread Has Crashed !");
                Common.stkTrading.Uninitialize();
            }
        }
        /// <summary>
        /// 存储上证类股票即时行情
        /// </summary>
        /// <param name="SHRate"></param>
        private void SaveQuotation(Show2003DBFRecord SHRate)
        {
            try
            {
                if (DateTime.Now.TimeOfDay < Common.BeginAMTS ||
                    (DateTime.Now.TimeOfDay > Common.EndAMTS
                    && DateTime.Now.TimeOfDay < Common.BeginPMTS)
                    || DateTime.Now.TimeOfDay > Common.EndPMTS)
                    return;
                //更新最近一次获取的上证类股票行情字典
                lock (Common.stkTrading.mapLastSHQuotation)
                {
                    if (Common.stkTrading.mapLastSHQuotation == null)
                        Common.stkTrading.mapLastSHQuotation = new Dictionary<string, Show2003DBFRecord>();

                    if (Common.stkTrading.mapLastSHQuotation.ContainsKey(SHRate.StockCode.Trim())
                        && Common.ComparePrice(Common.stkTrading.mapLastSHQuotation[SHRate.StockCode.Trim()].BuyingVal1, SHRate.BuyingVal1) == 0
                        && Common.ComparePrice(Common.stkTrading.mapLastSHQuotation[SHRate.StockCode.Trim()].SellingVal1, SHRate.SellingVal1) == 0
                        && Common.ComparePrice(Common.stkTrading.mapLastSHQuotation[SHRate.StockCode.Trim()].LatestPrice, SHRate.LatestPrice) == 0)
                        return;
                    Common.stkTrading.mapLastSHQuotation[SHRate.StockCode.Trim()] = SHRate;
                }

                if (!Directory.Exists(Common.strQuotationHistory.Trim() + "\\" + DateTime.Now.Date.ToString("yyyyMMdd")))
                    Directory.CreateDirectory(Common.strQuotationHistory.Trim() + "\\" + DateTime.Now.Date.ToString("yyyyMMdd"));

                using (StreamWriter SW = new StreamWriter(Common.strQuotationHistory.Trim() + "\\" + DateTime.Now.Date.ToString("yyyyMMdd")
                    + "\\SH_" + SHRate.StockCode.Trim() + ".log", true, Encoding.Default))
                {
                    //存储买1,卖1,最新价,最高价,最低价
                    SW.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "," + SHRate.BuyingVal1.ToString("f3") + "," +
                        SHRate.SellingVal1.ToString("f3") + "," + SHRate.LatestPrice.ToString("f3") + "," +
                        SHRate.HighestPrice.ToString("f3") + "," + SHRate.LowestPrice.ToString("f3"));
                    SW.Close();
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
            }
        }
        /// <summary>
        /// 存储深证类股票即时行情
        /// </summary>
        /// <param name="SZRate"></param>
        private void SaveQuotation(SjshqDBFRecord SZRate)
        {
            try
            {
                if (DateTime.Now.TimeOfDay < Common.BeginAMTS ||
                    (DateTime.Now.TimeOfDay > Common.EndAMTS
                    && DateTime.Now.TimeOfDay < Common.BeginPMTS)
                    || DateTime.Now.TimeOfDay > Common.EndPMTS)
                    return;
                //更新最近一次获取的深证类股票行情字典
                lock (Common.stkTrading.mapLastSZQuotation)
                {
                    if (Common.stkTrading.mapLastSZQuotation == null)
                        Common.stkTrading.mapLastSZQuotation = new Dictionary<string, SjshqDBFRecord>();
                    if (Common.stkTrading.mapLastSZQuotation.ContainsKey(SZRate.StockCode.Trim())
                        && Common.ComparePrice(Common.stkTrading.mapLastSZQuotation[SZRate.StockCode.Trim()].BuyingVal1, SZRate.BuyingVal1) == 0
                        && Common.ComparePrice(Common.stkTrading.mapLastSZQuotation[SZRate.StockCode.Trim()].SellingVal1, SZRate.SellingVal1) == 0
                        && Common.ComparePrice(Common.stkTrading.mapLastSZQuotation[SZRate.StockCode.Trim()].LatestPrice, SZRate.LatestPrice) == 0)
                        return;
                    Common.stkTrading.mapLastSZQuotation[SZRate.StockCode.Trim()] = SZRate;
                }

                if (!Directory.Exists(Common.strQuotationHistory.Trim() + "\\" + DateTime.Now.Date.ToString("yyyyMMdd")))
                    Directory.CreateDirectory(Common.strQuotationHistory.Trim() + "\\" + DateTime.Now.Date.ToString("yyyyMMdd"));
                using (StreamWriter SW = new StreamWriter(Common.strQuotationHistory.Trim() + "\\" + DateTime.Now.Date.ToString("yyyyMMdd")
                    + "\\SZ_" + SZRate.StockCode.Trim() + ".log", true, Encoding.Default))
                {
                    SW.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "," + SZRate.BuyingVal1.ToString("f3") + "," +
                        SZRate.SellingVal1.ToString("f3") + "," + SZRate.LatestPrice.ToString("f3") + "," +
                        SZRate.HighestPrice.ToString("f3") + "," + SZRate.LowestPrice.ToString("f3"));
                    SW.Close();
                }
            }
            catch (Exception err)
            {
                Common.Log(err);
            }
        }
    }
}
#endif