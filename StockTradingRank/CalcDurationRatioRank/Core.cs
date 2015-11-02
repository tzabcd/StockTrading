using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace CalcDurationRatioRank
{
    [Serializable]
    public struct UserRatioRank
    {
        public int UserID;
        public string UserName;
        public double Ratio;
        public int RatioDays;

        public void Initialize()
        {
            UserID = 0;
            UserName = "";
            Ratio = 0.00d;
            RatioDays = 0;
        }
    }

    public class Core
    {
        public static DataSet GetUpRationRank(int playId)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable("tbUserWealth");

                // Create DataColumn objects of data types.
                DataColumn UserId = new DataColumn("UserId");
                UserId.DataType = System.Type.GetType("System.Int32");
                dt.Columns.Add(UserId);

                DataColumn UserName = new DataColumn("UserName");
                UserName.DataType = System.Type.GetType("System.String");
                dt.Columns.Add(UserName);

                DataColumn Ratio = new DataColumn("Ratio");
                Ratio.DataType = System.Type.GetType("System.Double");
                dt.Columns.Add(Ratio);

                DataColumn RatioDays = new DataColumn("RatioDays");
                RatioDays.DataType = System.Type.GetType("System.Int32");
                dt.Columns.Add(RatioDays);

                for (int i = 0; i < 10; i++)
                {
                    // Populate one row with values.
                    DataRow dr = dt.NewRow();
                    dr["UserId"] = (i+1).ToString();
                    dr["UserName"] = "yxh_" + i;
                    dr["Ratio"] = 20.00d;
                    dr["RatioDays"] = 20;
                    dt.Rows.Add(dr);
                }
                ds.Tables.Add(dt);
                return ds;
            }
            catch (Exception ex)
            {
                Loger.Debug(ex.ToString());
                return null;
            }
        }


    }


}
