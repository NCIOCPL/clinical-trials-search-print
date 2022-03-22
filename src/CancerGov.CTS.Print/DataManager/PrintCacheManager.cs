using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using NCI.Data;
using NCI.DataManager;
using Common.Logging;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.DataManager
{
    public class PrintCacheManager
    {
        static ILog log = LogManager.GetLogger(typeof(PrintCacheManager));

        private string ConnectionString { get; set; }

        public PrintCacheManager(string dbConnection)
        {
            if (String.IsNullOrWhiteSpace(dbConnection))
            {
                log.Error($"{nameof(dbConnection)} must be set to a valid connection string.");
                throw new ArgumentNullException(nameof(dbConnection));
            }

            ConnectionString = dbConnection;
        }

        /// <summary>
        /// Connects to the database, and executes the stored proc with the required parameters. The 
        /// resulting content is the guid associated with the cached print content.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="searchParams"></param>
        /// <param name="isLive"></param>
        /// <returns>A guid.</returns>
        //public static Guid SavePrintResult(string content, IEnumerable<String> trialIDs, CTSSearchParams searchParams, bool isLive)
        public Guid Save(string[] trialIds, SearchCriteria searchParams, string content)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString))
            {
                Guid printResultGuid = Guid.Empty;

                // Do something here to turn searchParams into a string.  Maybe a really nifty ToString() implementation.....

                SqlParameter[] parameters = {
                    new SqlParameter("@printid", SqlDbType.UniqueIdentifier),
                    new SqlParameter("@content", SqlDbType.NVarChar, content.Length),
                    new SqlParameter("@searchparams", SqlDbType.NVarChar),
                    new SqlParameter("@isLive", SqlDbType.Bit),
                    new SqlParameter("@trialids", SqlDbType.Structured)
                };
                parameters[0].Direction = ParameterDirection.Output;
                parameters[1].Value = content;
                parameters[2].Value = searchParams.ToJson();
                parameters[3].Value = 1; // Legacy argument. The "preview" site is no longer used.
                parameters[4].Value = CreatePrintIdDataTable(trialIds);

                try
                {
                    SqlHelper.ExecuteNonQuery(conn, CommandType.StoredProcedure, "dbo.ct_insertPrintresultcache", parameters);
                    printResultGuid = (Guid)parameters[0].Value;
                }
                catch (SqlException ex)
                {
                    log.Error("Unable to save data. Search Params: " + searchParams, ex);
                }

                return printResultGuid;
            }
        }

        private static DataTable CreatePrintIdDataTable(IEnumerable<String> trialIDs)
        {
            // This datatable must be structured like the datatable in the stored proc, 
            // in order to be passed in correctly as a parameter.
            DataTable dt = new DataTable();

            // Second column, "trialid", is an varchar(124)
            DataColumn dc = new DataColumn();
            dc.DataType = System.Type.GetType("System.String");
            dc.MaxLength = 124;
            dc.ColumnName = "trialid";
            dt.Columns.Add(dc);

            foreach (var id in trialIDs)
            {
                DataRow row = dt.NewRow();
                row["trialid"] = id;
                dt.Rows.Add(row);
            }

            return dt;
        }

        /// <summary>
        /// Retrieves cached CTS print HTML from the database.
        /// </summary>
        /// <param name="printId">A GUID identifying the cached page to retrieve.</param>
        /// <returns>The page HTML, or NULL if the document does not exist.</returns>
        public async Task<string> GetPage(Guid printID)
        {
            string printPageHtml = null;

            using (SqlConnection conn = SqlHelper.CreateConnection(ConnectionString))
            {
                using (SqlDataReader reader =
                        await SqlHelper.ExecuteReaderAsync(conn, CommandType.StoredProcedure, "dbo.ct_getPrintResultCache",
                            new SqlParameter("@PrintId ", printID),
                            new SqlParameter("@isLive", 1)  // Legacy argument. The "preview" site is no longer used.
                        )
                )
                {
                    if (await reader.ReadAsync())
                    {
                        SqlFieldValueReader sqlFVReader = new SqlFieldValueReader(reader);
                        printPageHtml = sqlFVReader.GetString("content");
                        printPageHtml = printPageHtml.Replace("${generatePrintURL}", printID.ToString());
                    }
                    else
                    {
                        log.Debug($"No page found for {nameof(printID)} '{printID}'");
                    }    
                }
            }

            return printPageHtml;
        }
    }
}
