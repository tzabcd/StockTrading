using System;
using System.Collections.Generic;
using System.Text;

namespace CalcDailyRank
{
    public class DaysUtils
    {
        #region 行情日计算

        public static bool IsQuoteDate(DateTime inputDay)
        {
            if (inputDay.DayOfWeek == DayOfWeek.Saturday || inputDay.DayOfWeek == DayOfWeek.Sunday)
                return false;
            foreach(var data in BaseConfig.listHoliday)
            {
                if (inputDay == data)
                    return false;
            }
            return true;
        }
 
        #endregion

        #region 月份相关

        public static DateTime FirstDayOfMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day);
        }

        public static DateTime LastDayOfMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1);
        }

        public static DateTime FirstDayOfPreviousMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddMonths(-1);
        }

        public static DateTime LastDayOfPrdviousMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddDays(-1);
        }
        #endregion

      

    }
}
