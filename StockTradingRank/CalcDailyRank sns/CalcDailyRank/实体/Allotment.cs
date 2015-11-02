using System;
using System.Collections.Generic;
using System.Text;

namespace CalcDailyRank
{
    public struct Allotment
    {
        public string StockCode;
        public StockMarket Market;
        public Double Scheme; //比例
        public Double Price; //价格
        public DateTime RecordDate; //登记日
        public DateTime PayBeginDate; // 起始日
        public DateTime PayEndDate; // 截止日
        public DateTime ExecuteDate; // 除权日
        public Boolean Force; //强制标志

        public int SendFlag;

        public void Initialize()
        {
            StockCode = "";
            Market = StockMarket.Unknown;

            Scheme = 0;
            Price = 0;

            RecordDate = DateTime.MinValue;
            PayBeginDate = DateTime.MinValue;
            PayEndDate = DateTime.MinValue;
            ExecuteDate = DateTime.MinValue;
            Force = false;

            SendFlag = 0;
        }
    }
}
