using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Common.Logging;
using NCI.Data;

namespace NCI.DataManager
{
    public class BlogSearchDataManager 
    {
        static ILog log = LogManager.GetLogger(typeof(BlogSearchDataManager));

        /// <summary>
        /// Connects to the database , and executes the stored proc with the required parameter. The 
        /// results are processed as SearchResult object.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="keyWords"></param>
        /// <param name="recordsPerPage"></param>
        /// <param name="maxResults"></param>
        /// <param name="searchFilter"></param>
        /// <param name="excludeSearchFilter"></param>
        /// <param name="language"></param>
        /// <returns>A collection which contains Search Result objects.</returns>
        public static ICollection<SeriesPrevNextResult> Execute(
        string contentid,
        string filter,
        string language,
        bool isLive
            )
        {
            try
            {
                ICollection<SeriesPrevNextResult> searchResults = new Collection<SeriesPrevNextResult>();


                string connString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                if (!string.IsNullOrEmpty(connString))
                {
                    using (SqlConnection conn = SqlHelper.CreateConnection(connString))
                    {
                        using (DataSet ds =
                                    SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "dbo.search_getNeighbor",
                                    new SqlParameter("@filter", filter),
                                    new SqlParameter("@contentid", contentid),
                                    new SqlParameter("@language", language),
                                    new SqlParameter("@isLive", isLive ? 1 : 0)

                            ))
                        {
                            if (ds.Tables.Count != 2)
                            {
                                log.Error("Execute(): Failed in BlogDataManager");
                                return searchResults;
                            }

                            //check for rows, is there more than 1 row bad, if zero leave as null




                            if (ds.Tables[0].Rows.Count > 1 || ds.Tables[0].Rows.Count == 0)
                            {
                                //log error
                                //return what should be returned
                                log.Error("Execute(): Failed in BlogDataManager");
                                return searchResults;
                            }


                            if (ds.Tables[1].Rows.Count > 1 || ds.Tables[1].Rows.Count == 0)
                            {
                                //log error
                                //return what should be returned
                                log.Error("Execute(): Failed in BlogDataManager");
                                return searchResults;
                            }





                          

                            SeriesPrevNextResult.SeriesItem previous = new SeriesPrevNextResult.SeriesItem(ds.Tables[0].Rows[0].Field<string>("prettyurl"), ds.Tables[0].Rows[0].Field<string>("short_title"));
                            SeriesPrevNextResult.SeriesItem next = new SeriesPrevNextResult.SeriesItem(ds.Tables[1].Rows[0].Field<string>("prettyurl"), ds.Tables[1].Rows[0].Field<string>("short_title"));
                            SeriesPrevNextResult searchResult = new SeriesPrevNextResult(previous, next);

                          
                            searchResults.Add(searchResult);


                        }
                    }
                }
                else
                    throw new Exception("Configuration Missing:Connection string is null, update the web config with connection string");

                return searchResults;
            }
            catch (Exception ex)
            {
                log.Error("Execute(): Failed in DataManager", ex);
                throw;
            }
        }




        /// <summary>
        /// Connects to the database , and executes the stored proc with the required parameter. The 
        /// results are processed as SearchResult object.
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="keyWords"></param>
        /// <param name="recordsPerPage"></param>
        /// <param name="maxResults"></param>
        /// <param name="searchFilter"></param>
        /// <param name="excludeSearchFilter"></param>
        /// <param name="resultsSortOrder"></param>
        /// <param name="language"></param>
        /// <param name="isLive"></param>
        /// <param name="actualMaxResult"></param>
        /// <param name="site"></param>
        /// <returns>A collection which contains Search Result objects.</returns>
        public static ICollection<SeriesPrevNextResult> Execute(
       
        string filter,
        string contentid,
        string language,
        bool isLive,
        string sitename
            )
        {
            try
            {
                ICollection<SeriesPrevNextResult> searchResults = new Collection<SeriesPrevNextResult>();
          

                string connString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                if (!string.IsNullOrEmpty(connString))
                {
                    using (SqlConnection conn = SqlHelper.CreateConnection(connString))
                    {
                        using (DataSet ds =
                                    SqlHelper.ExecuteDataset(conn, CommandType.StoredProcedure, "dbo.search_getNeighbor",
                                    new SqlParameter("@filter", filter),
                                    new SqlParameter("@contentid", contentid),
                                    new SqlParameter("@language", language),
                                    new SqlParameter("@isLive", isLive ? 1 : 0),
                                    new SqlParameter("@siteName", sitename)
                                    
                            ))
                        {
                            if (ds.Tables.Count != 2)
                            {
                                log.Error("Execute: Failed in BlogDataManager");
                                return searchResults;
                            }

                            //check for rows, is there more than 1 row bad, if zero leave as null

                            SeriesPrevNextResult.SeriesItem next = null;
                            SeriesPrevNextResult.SeriesItem previous=null;
                           


                            if (ds.Tables[0].Rows.Count > 1)
                            {
                                //log error
                                //return what should be returned
                                log.Error("Execute: Failed in BlogDataManager");
                                return searchResults;
                            }
                            if (ds.Tables[0].Rows.Count != 0)
                            {
                               previous = new SeriesPrevNextResult.SeriesItem(ds.Tables[0].Rows[0].Field<string>("prettyurl"), ds.Tables[0].Rows[0].Field<string>("long_title"));
                            
                            }
                            
                            if (ds.Tables[1].Rows.Count > 1)
                            {
                                //log error
                                //return what should be returned
                                log.Error("Execute(): Failed in BlogDataManager");
                                return searchResults;
                            }

                            if (ds.Tables[1].Rows.Count != 0)
                            {
                                next = new SeriesPrevNextResult.SeriesItem(ds.Tables[1].Rows[0].Field<string>("prettyurl"), ds.Tables[1].Rows[0].Field<string>("long_title"));
                            }
                       



                            SeriesPrevNextResult searchResult = new SeriesPrevNextResult(previous, next);


                            searchResults.Add(searchResult);
                                
                            
                        }
                    }
                }
                else
                    throw new Exception("Configuration Missing:Connection string is null, update the web config with connection string");

                return searchResults;
            }
            catch (Exception ex)
            {
                log.Error("Execute(): Failed in DataManager", ex);
                throw;
            }
        }
    }
}
