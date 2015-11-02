using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace CalcDailyRank
{
    class DebugSqlUtils
    {
        public static string ReplaceSqlPara(SqlCommand cmd)
        {
            if (cmd == null)
                return "SqlCommand null";
            String cmdText = cmd.CommandText;
            SqlParameterCollection paras = cmd.Parameters;
            if (paras != null)
            {
                foreach (SqlParameter p in paras)
                {
                    switch (p.SqlDbType)
                    {
                        case SqlDbType.Char:
                        case SqlDbType.Date:
                        case SqlDbType.DateTime:
                        case SqlDbType.SmallDateTime:
                        case SqlDbType.NChar:
                        case SqlDbType.NText:
                        case SqlDbType.NVarChar:
                        case SqlDbType.Text:
                        case SqlDbType.Time:
                        case SqlDbType.VarChar:
                            cmdText = cmdText.Replace(p.ParameterName, "'" + p.Value.ToString() + "'");
                            break;
                        default:
                            cmdText = cmdText.Replace(p.ParameterName, p.Value.ToString());
                            break;
                    }

                }
            }
            return cmdText;
        }
    }
}
