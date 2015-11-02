using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using QDBFAnalyzer.StructuredAnalysis;

namespace Stock_Trading_UserRank_Kernel
{
    public enum StockMarket : byte
    {
        Unknown = 0,
        Shanghai = 1,
        Shenzhen = 2,
    }
    public class Quotation
    {
        private static Dictionary<string, Show2003DBFRecord> mapSHRates = null;
        private static Dictionary<string, SjshqDBFRecord> mapSZRates = null;
        private static Show2003Analyzer Show2003Reader = new Show2003Analyzer();
        private static SjshqAnalyzer SjshqReader = new SjshqAnalyzer();

        public Quotation()
        {
            try
            {
                mapSHRates = new Dictionary<string, Show2003DBFRecord>();
                mapSZRates = new Dictionary<string, SjshqDBFRecord>();
            }
            catch
            { }
        }
        public bool SetQuotaionLocation(string strShanghai, string strShenzhen)
        {
            try
            {
                if (strShanghai == null || strShenzhen == null ||
                    !File.Exists(strShanghai.Trim()) ||
                    !File.Exists(strShenzhen.Trim()))
                    return false;
                Show2003Reader.DBFName = strShanghai.Trim();
                SjshqReader.DBFName = strShenzhen.Trim();
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                return false;
            }
        }
        public bool ReloadQuotation()
        {
            try
            {
                lock (mapSHRates)
                {
                    if (mapSHRates != null)
                    {
                        mapSHRates.Clear();
                        if (!Show2003Reader.ReadAllRecords(out mapSHRates)
                            && Show2003Reader.LastError.Trim().Length > 0)
                        {
                            Loger.Debug("Loading SH Quotation Failed: " + Show2003Reader.LastError.Trim());
                            return false;
                        }
                    }
                    else
                    {
                        Loger.Debug("Loading SH Quotation Failed.");
                        return false;
                    }
                }
                lock (mapSZRates)
                {
                    if (mapSZRates != null)
                    {
                        mapSZRates.Clear();
                        if (!SjshqReader.ReadAllRecords(out mapSZRates)
                            && SjshqReader.LastError.Trim().Length > 0)
                        {
                            Loger.Debug("Loading SZ Quotation Failed: " + SjshqReader.LastError.Trim());
                            return false;
                        }
                    }
                    else
                    {
                        Loger.Debug("Loading SZ Quotation Failed.");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                return false;
            }
        }
        public bool FindQuotation(string StockCode, out Show2003DBFRecord SHRecord)
        {
            SHRecord = new Show2003DBFRecord();
            try
            {
                SHRecord.Clear();
                if (StockCode == null || StockCode.Trim().Length != 6)
                    return false;
                if (mapSHRates == null)
                    return false;
                lock (mapSHRates)
                {
                    if (mapSHRates.ContainsKey(StockCode.Trim()))
                    {
                        SHRecord = mapSHRates[StockCode.Trim()];
                        return true;
                    }
                }
                return false;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                SHRecord.Clear();
                return false;
            }
        }
        public bool FindQuotation(string StockCode, out SjshqDBFRecord SZRecord)
        {
            SZRecord = new SjshqDBFRecord();
            try
            {
                SZRecord.Clear();
                if (StockCode == null || StockCode.Trim().Length != 6)
                    return false;
                if (mapSZRates == null)
                    return false;
                lock (mapSZRates)
                {
                    if (mapSZRates.ContainsKey(StockCode.Trim()))
                    {
                        SZRecord = mapSZRates[StockCode.Trim()];
                        return true;
                    }
                }
                return false;
            }
            catch (Exception err)
            {
                Loger.Debug(err);
                SZRecord.Clear();
                return false;
            }
        }
    }
}