using System;
using System.Collections.Generic;
using System.Text;

namespace CalcDailyRank
{
    class ConvertUtils
    {
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
