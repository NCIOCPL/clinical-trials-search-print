using System;
using System.Text.RegularExpressions;
using NCI.Text;

namespace NCI.Web
{
    /// <summary>
    /// Returns a formatted date.  Also serves as a base class for other 
    /// date processing handlers that want to support the same date formatting code.
    /// 
    /// Index   Param                   Requirements
    /// 0       DateFormat              Optional
    /// </summary>
    [MarkupExtensionHandler("Returns a formatted date.",
        Usage = "{mx:HtmlHelpers.Date()} or {mx:HtmlHelpers.Date([DateFormat])}  Returns the current date formatted according to the specified date format or \"MonthNum1, YearNum4\" if not date format is specified.")]
    public class DateHandler : MarkupExtensionHandler
    {
        private string _dateFormat = "MonthWordFull DayNum1, YearNum4"; // Default format.


        private void ParseParams(string[] parameters)
        {
            // Make sure they didn't pass in too many parameters.
            int maxParameterCount = 1;
            if (parameters.Length > maxParameterCount)
            {
                throw new MarkupExtensionException(String.Format(MarkupExtensionHandler.TooManyParametersError, this.Name, maxParameterCount, parameters.Length));
            }

            // Get optional parameters.

            // DateFormat
            int parameterIndex = -1;
            if (parameters.Length > ++parameterIndex)
            {
                // TODO-v.NEXT: Don't have access to Strings.Clean from here - do manually for now and revisit better way.
                string dateFormatParameter = null;
                if ((parameters[parameterIndex] != null) && (parameters[parameterIndex].Trim() != String.Empty))
                {
                    dateFormatParameter = parameters[parameterIndex].Trim();
                }

                if (dateFormatParameter != null)
                    _dateFormat = dateFormatParameter;
            }
        }


        public static string GetFormattedDate(DateTime date, string format)
        {
            // Replace the placeholders in the date format string.
            // We used regex to ignore case since we didn't want to change the case on everything.
            format = Regex.Replace(format, "DayNum1", date.ToString(" d ").Trim(), RegexOptions.IgnoreCase);
            format = Regex.Replace(format, "DayNum2", date.ToString("dd"), RegexOptions.IgnoreCase);
            format = Regex.Replace(format, "MonthNum1", date.ToString(" M ").Trim(), RegexOptions.IgnoreCase);
            format = Regex.Replace(format, "MonthNum2", date.ToString("MM"), RegexOptions.IgnoreCase);
            format = Regex.Replace(format, "MonthWord3", date.ToString("MMM"), RegexOptions.IgnoreCase);
            format = Regex.Replace(format, "MonthWordFull", date.ToString("MMMM"), RegexOptions.IgnoreCase);
            format = Regex.Replace(format, "YearNum2", date.ToString("yy"), RegexOptions.IgnoreCase);
            format = Regex.Replace(format, "YearNum4", date.ToString("yyyy"), RegexOptions.IgnoreCase);
            format = Regex.Replace(format, "DayOfWeek", date.ToString("dddd"), RegexOptions.IgnoreCase);

            return format;
        }


        public override string Name
        {
            get { return "HtmlHelpers.Date"; }
        }

        /// <summary>
        /// TODO-v.NEXT: add optional timezone param?
        /// TODO-v.NEXT: put all format strings in Dictionary with corresponding value that goes in the .ToString() call so can use the .Keys colleciton in this method to validate they've passed in a valid format string?
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override string Process(string[] parameters)
        {
            ParseParams(parameters);

            // Get and return the formatted date.
            return GetFormattedDate(DateTime.Now, _dateFormat);
        }
    }
}
