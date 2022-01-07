using System.Web;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Common.Logging;
using NCI.Util;

namespace NCI.DataManager
{
    public class BlogArchiveDataManager
    {
        //Getting the List of years/months from the DB
        public static DataTable Execute(string blogSeriesId, int numberOfYears)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.taxonomy_getMonthlyAgg", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@searchfilter", SqlDbType.VarChar);
                    cmd.Parameters["@searchfilter"].Value = blogSeriesId;
                    cmd.Parameters.Add("@numberofpreviousyears", SqlDbType.Int);
                    cmd.Parameters["@numberofpreviousyears"].Value = numberOfYears;

                    using (SqlDataAdapter dbAdapter = new SqlDataAdapter(cmd))
                    {
                        dbAdapter.Fill(dt);
                    }
                }
            }

            return dt;
        }
    }
}
