using System;
using System.Collections.Generic;
using System.Text;

namespace CalcDailyRank
{
    public struct Bonus
    {
        public string StockCode;
        public StockMarket Market;
        public double SendBonus;
        public double TransferBonus;
        public double BonusIssue;

        public DateTime BonusRegDate;
        public DateTime BonusExeDate;
        public int SendFlag;

        public string userDataBase;

        public void Initialize()
        {
            StockCode = "";
            Market = StockMarket.Unknown;

            SendBonus = 0;
            TransferBonus = 0;
            BonusIssue = 0;

            BonusRegDate = DateTime.MinValue;
            BonusExeDate = DateTime.MinValue;
            SendFlag = 0;
        }
    }
}
