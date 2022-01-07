using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Common.Logging;
using NCI.Data;

namespace NCI.DataManager
{
    /// <summary>
    /// This class is a datamanager class and deals with all data that resides 
    /// in the database and used maily for content and dynamic searches.
    /// </summary>
    public class SearchDataManager
    {
        static ILog log = LogManager.GetLogger(typeof(SearchDataManager));

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
        public static ICollection<SearchResult> Execute(
        int currentPage,
        DateTime startDate,
        DateTime endDate,
        string keyWords,
        int recordsPerPage,
        int maxResults,
        string searchFilter,
        string excludeSearchFilter,
        int resultsSortOrder,
        string language,
        bool isLive,
        out int actualMaxResult
            )
        {
            try
            {
                ICollection<SearchResult> searchResults = new Collection<SearchResult>();
                actualMaxResult = 0;

                string connString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                if (!string.IsNullOrEmpty(connString))
                {
                    using (SqlConnection conn = SqlHelper.CreateConnection(connString))
                    {
                        using (SqlDataReader reader =
                                    SqlHelper.ExecuteReader(conn, CommandType.StoredProcedure, "dbo.searchFilterKeywordDate",
                                    new SqlParameter("@Keyword ", string.IsNullOrEmpty(keyWords) ? null : keyWords),
                                    new SqlParameter("@StartDate", startDate == DateTime.MinValue ? null : String.Format("{0:MM/dd/yyyy}", startDate)),
                                    new SqlParameter("@EndDate", endDate == DateTime.MaxValue ? null : String.Format("{0:MM/dd/yyyy}", endDate)),
                                    new SqlParameter("@SearchFilter", searchFilter),
                                    new SqlParameter("@excludeSearchFilter", excludeSearchFilter),
                                    new SqlParameter("@ResultsSortOrder", resultsSortOrder),
                                    new SqlParameter("@language", language),
                                    new SqlParameter("@maxResults", maxResults),
                                    new SqlParameter("@recordsPerPage", recordsPerPage),
                                    new SqlParameter("@StartPage", currentPage),
                                    new SqlParameter("@isLive", isLive ? 1 : 0)
                            ))
                        {
                            while (reader.Read())
                            {
                                actualMaxResult = reader.GetInt32(0);
                            }

                            if (reader.NextResult())
                            {
                                SqlFieldValueReader sqlFVReader = new SqlFieldValueReader(reader);

                                while (sqlFVReader.Read())
                                {
                                    SearchResult searchResult = new SearchResult();
                                    searchResult.HRef = sqlFVReader.GetString("prettyurl");
                                    searchResult.LongTitle = sqlFVReader.GetString("Long_Title");
                                    searchResult.LongDescription = sqlFVReader.GetString("Long_Description");
                                    searchResult.ShortDescription = sqlFVReader.GetString("Short_Description");
                                    searchResult.ShortTitle = sqlFVReader.GetString("Short_Title");

                                    searchResult.QandAUrl = sqlFVReader.GetString("news_qanda_url");
                                    searchResult.ImageUrl = sqlFVReader.GetString("imageurl");
                                    searchResult.VideoUrl = sqlFVReader.GetString("videourl");
                                    searchResult.AudioUrl = sqlFVReader.GetString("audiourl");
                                    searchResult.Language = sqlFVReader.GetString("language");
                                    searchResult.OtherlanguageUrl = sqlFVReader.GetString("otherlanguageURL");
                                    searchResult.DateDisplayMode = sqlFVReader.GetString("Date_Display_Mode");
                                    searchResult.SubHeader = sqlFVReader.GetString("subheader");
                                    searchResult.ImageSource = sqlFVReader.GetString("imagesource");
                                    searchResult.AltText = sqlFVReader.GetString("alttext");
                                    searchResult.AbbreviatedSource = sqlFVReader.GetString("abbreviatedsource");

                                    searchResult.Alt = sqlFVReader.GetString("alt");
                                    searchResult.BlogBody = sqlFVReader.GetString("blogparagraph");
                                    searchResult.ThumbnailURL = sqlFVReader.GetString("thumbnailurl");
                                    searchResult.Author = sqlFVReader.GetString("author");
                                    searchResult.ContentType = sqlFVReader.GetString("contenttype");
                                    searchResult.ContentID = sqlFVReader.GetString("contentid");

                                    // Column for blog series content types only - return false for all other content types.
                                    // If this is a blog series landing page, check to see if comments have been turned on.
                                    try
                                    {
                                        searchResult.AllowComments = sqlFVReader.GetBoolean("allowComments");
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        searchResult.AllowComments = false;
                                    }

                                    // File size and mime type are only used for file content types. If the database column 
                                    // does not exist for the searched item (i.e. blogs), catch indexfOutOfRangeException and 
                                    // populate the search result with default values.
                                    try
                                    {
                                        searchResult.FileSize = sqlFVReader.GetInt32("item_size");
                                        searchResult.MimeType = sqlFVReader.GetString("item_type");
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        searchResult.FileSize = 0;
                                        searchResult.MimeType = null;
                                    }

                                    // Keep original published/modified/reviewed values, but also compare each value
                                    // and return the most recent of the three for use in lists.
                                    DateTime dfp = sqlFVReader.GetDateTime("Date_first_published");
                                    DateTime listDate = dfp;
                                    if (dfp != DateTime.MinValue)
                                    {
                                        searchResult.PostedDate = String.Format("{0:MM/dd/yyyy}", dfp);
                                        searchResult.PostedDate_NewsPortalFormat = String.Format("{0:MMMM d, yyyy}", dfp);
                                        // Format date for Blog Series page
                                        searchResult.DateForBlogs = String.Format("{0:MMMM d, yyyy}", dfp);
                                        // Format date for Spanish blog lists - dd de MMMM de yyyy
                                        searchResult.DateForBlogsEs = dfp.Day.ToString() + " de " + dfp.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-US"))
                                            + " de " + dfp.Year.ToString();
                                    }
                                    DateTime dlm = sqlFVReader.GetDateTime("date_last_modified");
                                    if (dlm != DateTime.MinValue)
                                        searchResult.UpdatedDate = String.Format("{0:MM/dd/yyyy}", dlm);
                                    if (dlm > listDate)
                                        listDate = dlm;

                                    DateTime dlr = sqlFVReader.GetDateTime("date_last_reviewed");
                                    if (dlr != DateTime.MinValue)
                                        searchResult.ReviewedDate = String.Format("{0:MM/dd/yyyy}", dlr);
                                    if (dlr > listDate)
                                        listDate = dlr;

                                    searchResult.DateForLists = String.Format("{0:MMMM d, yyyy}", listDate);
                                    // Format date for Spanish dynamic lists - dd de MMMM de yyyy
                                    searchResult.DateForListsEs = listDate.Day.ToString() + " de " + listDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-US"))
                                    + " de " + listDate.Year.ToString();
                                    searchResults.Add(searchResult);
                                }
                            }
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
                throw ex;
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
        /// <param name="taxonomyFiltersTable"></param>
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
        public static ICollection<SearchResult> Execute(
        int currentPage,
        DateTime startDate,
        DateTime endDate,
        string keyWords,
        DataTable taxonomyFiltersTable,
        int recordsPerPage,
        int maxResults,
        string searchFilter,
        string excludeSearchFilter,
        int resultsSortOrder,
        string language,
        bool isLive,
        out int actualMaxResult,
        string sitename
            )
        {
            try
            {
                ICollection<SearchResult> searchResults = new Collection<SearchResult>();
                actualMaxResult = 0;

                string connString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

                // Create list of SQL parameters required for passing into stored proc
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@Keyword ", string.IsNullOrEmpty(keyWords) ? null : keyWords));
                parameters.Add(new SqlParameter("@StartDate", startDate == DateTime.MinValue ? null : String.Format("{0:MM/dd/yyyy}", startDate)));
                parameters.Add(new SqlParameter("@EndDate", endDate == DateTime.MaxValue ? null : String.Format("{0:MM/dd/yyyy}", endDate)));
                parameters.Add(new SqlParameter("@SearchFilter", searchFilter));
                parameters.Add(new SqlParameter("@excludeSearchFilter", excludeSearchFilter));
                parameters.Add(new SqlParameter("@ResultsSortOrder", resultsSortOrder));
                parameters.Add(new SqlParameter("@language", language));
                parameters.Add(new SqlParameter("@maxResults", maxResults));
                parameters.Add(new SqlParameter("@recordsPerPage", recordsPerPage));
                parameters.Add(new SqlParameter("@StartPage", currentPage));
                parameters.Add(new SqlParameter("@isLive", isLive ? 1 : 0));
                parameters.Add(new SqlParameter("@siteName", sitename));

                // If the taxonomyFiltersTable exists, then there are taxonomy tags to filter by. Create the
                // taxonomyParam parameter to pass in the datatable to the stored proc, and add to the list
                // of parameters.
                if (taxonomyFiltersTable != null)
                {
                    SqlParameter taxonomyParam = new SqlParameter("@taxonomyFilter", taxonomyFiltersTable);
                    taxonomyParam.SqlDbType = SqlDbType.Structured;
                    taxonomyParam.TypeName = "dbo.udt_TaxonomyFilter";
                    parameters.Add(taxonomyParam);
                }

                if (!string.IsNullOrEmpty(connString))
                {
                    using (SqlConnection conn = SqlHelper.CreateConnection(connString))
                    {
                        using (SqlDataReader reader = SqlHelper.ExecuteReader(conn, CommandType.StoredProcedure, "dbo.searchFilterKeywordDate",
                                parameters.ToArray()
                            ))
                        {
                            while (reader.Read())
                            {
                                actualMaxResult = reader.GetInt32(0);
                            }
                            
                            if (reader.NextResult())
                            {
                                SqlFieldValueReader sqlFVReader = new SqlFieldValueReader(reader);

                                while (sqlFVReader.Read())
                                {
                                    SearchResult searchResult = new SearchResult();
                                    searchResult.HRef = sqlFVReader.GetString("prettyurl");
                                    searchResult.LongTitle = sqlFVReader.GetString("Long_Title");
                                    searchResult.LongDescription = sqlFVReader.GetString("Long_Description");
                                    searchResult.ShortDescription = sqlFVReader.GetString("Short_Description");
                                    searchResult.ShortTitle = sqlFVReader.GetString("Short_Title");

                                    searchResult.QandAUrl = sqlFVReader.GetString("news_qanda_url");
                                    searchResult.ImageUrl = sqlFVReader.GetString("imageurl");
                                    searchResult.VideoUrl = sqlFVReader.GetString("videourl");
                                    searchResult.AudioUrl = sqlFVReader.GetString("audiourl");
                                    searchResult.Language = sqlFVReader.GetString("language");
                                    searchResult.OtherlanguageUrl = sqlFVReader.GetString("otherlanguageURL");
                                    searchResult.DateDisplayMode = sqlFVReader.GetString("Date_Display_Mode");
                                    searchResult.SubHeader = sqlFVReader.GetString("subheader");
                                    searchResult.ImageSource = sqlFVReader.GetString("imagesource");
                                    searchResult.AltText = sqlFVReader.GetString("alttext");
                                    searchResult.AbbreviatedSource = sqlFVReader.GetString("abbreviatedsource");
                                    searchResult.SubscriptionRequired = sqlFVReader.GetBoolean("subscription_required");

                                    searchResult.Alt = sqlFVReader.GetString("alt");
                                    searchResult.BlogBody = sqlFVReader.GetString("blogparagraph");
                                    searchResult.ThumbnailURL = sqlFVReader.GetString("thumbnailurl");
                                    searchResult.Author = sqlFVReader.GetString("author");
                                    searchResult.ContentType = sqlFVReader.GetString("contenttype");
                                    searchResult.ContentID = sqlFVReader.GetString("contentid");

                                    // Column for blog series content types only - return false for all other content types.
                                    // If this is a blog series landing page, check to see if comments have been turned on.
                                    try
                                    {
                                        searchResult.AllowComments = sqlFVReader.GetBoolean("allowComments");
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        searchResult.AllowComments = false;
                                    }

                                    // File size and mime type are only used for file content types. If the database column 
                                    // does not exist for the searched item (i.e. blogs), catch indexfOutOfRangeException and 
                                    // populate the search result with default values.
                                    try
                                    {
                                        searchResult.FileSize = sqlFVReader.GetInt32("item_size");
                                        searchResult.MimeType = sqlFVReader.GetString("item_type");
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        searchResult.FileSize = 0;
                                        searchResult.MimeType = null;
                                    }

                                    // Keep original published/modified/reviewed values, but also compare each value
                                    // and return the most recent of the three for use in lists.
                                    int dateDisplay = sqlFVReader.GetInt32("date_display_mode");


                                    DateTime dfp = sqlFVReader.GetDateTime("Date_first_published");
                                    DateTime listDate = dfp;
                                    if (dfp != DateTime.MinValue)
                                    {
                                        searchResult.PostedDate = String.Format("{0:MM/dd/yyyy}", dfp);
                                        searchResult.PostedDate_NewsPortalFormat = String.Format("{0:MMMM d, yyyy}", dfp);
                                        // Format date for Blog Series page
                                        searchResult.DateForBlogs = String.Format("{0:MMMM d, yyyy}", dfp);
                                        // Format date for Spanish blog lists - dd de MMMM de yyyy
                                        searchResult.DateForBlogsEs = dfp.Day.ToString() + " de " + dfp.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-US"))
                                            + " de " + dfp.Year.ToString();
                                    }
                                    DateTime dlm = sqlFVReader.GetDateTime("date_last_modified");
                                    if (dlm != DateTime.MinValue)
                                        searchResult.UpdatedDate = String.Format("{0:MM/dd/yyyy}", dlm);


                                    DateTime dlr = sqlFVReader.GetDateTime("date_last_reviewed");
                                    if (dlr != DateTime.MinValue)
                                        searchResult.ReviewedDate = String.Format("{0:MM/dd/yyyy}", dlr);

                                    DateTime dateTime = GetDateDisplay(dateDisplay, dfp, dlm, dlr);
                                    if (dateTime == DateTime.MinValue)
                                    {
                                        searchResult.DateForLists = "";
                                        searchResult.DateForListsEs = "";
                                    }
                                    else
                                    {
                                        searchResult.DateForLists = String.Format("{0:MMMM d, yyyy}", dateTime);
                                        // Format date for Spanish dynamic lists - dd de MMMM de yyyy
                                        searchResult.DateForListsEs = listDate.Day.ToString() + " de " + listDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-US"))
                                        + " de " + listDate.Year.ToString();
                                    }
                                    searchResults.Add(searchResult);
                                }
                            }
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
        /// Get Proper Date Display for dynamic lists
        /// </summary>
        /// <param name="displayMode"><see cref="DateDisplayModes"/></param>
        /// <param name="dfp">Date First Published</param>
        /// <param name="dlm">Date Last Modified</param>
        /// <param name="dlr">Date Last Reviewed</param>
        /// <returns></returns>
        private static DateTime GetDateDisplay(int displayMode, DateTime dfp, DateTime dlm, DateTime dlr)
        {

            //initializing to if date display is none  
            //This would be DisplayDateModes.None
            DateTime dateRet = DateTime.MinValue;


            switch (displayMode)
            {
                case 1://DateDisplayModes.Posted which is 1
                    {
                        dateRet = dfp;
                        break;
                    }
                case 2://DateDisplayModes.Updated which is 2
                    {
                        dateRet = dlm;
                        break;
                    }
                case 3://DateDisplayModes.PostedUpdated which is 3
                    {
                        if (dlm > dateRet)
                            dateRet = dlm;
                        else 
                            dateRet = dfp;
                        break;
                    }
                case 4://DateDisplayModes.Reviewed which is 4
                    {
                        dateRet = dlr;
                        break;
                    }
                case 5://DateDisplayModes.PostedReviewed which is 5
                    {
                        if (dlr > dfp)
                            dateRet = dlr;
                        else
                            dateRet = dfp;
                        break;
                    }
                case 6://DateDisplayModes.UpdatedReviewed which is 6
                    {
                        if (dlr > dlm)
                            dateRet = dlr;
                        else
                            dateRet = dlm;
                        break;
                    }
                case 7://DateDisplayModes.All is 7
                    {
                        if (dfp < dlr && dfp > dlm)
                        {
                            dateRet = dlr;
                        }
                        else if (dfp > dlr && dfp < dlm)
                        {
                            dateRet = dlm;
                        }
                        else if (dfp < dlr && dfp < dlm)
                        {
                            if (dlm < dlr)
                            {
                                dateRet = dlr;
                            }
                            else
                            {
                                dateRet = dlm;
                            }

                        }
                        else
                            dateRet = dfp;

                        break;
                    }


            }
            
            return dateRet;
        }
    }
}
