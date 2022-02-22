using System;

using Common.Logging;
using Newtonsoft.Json.Linq;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.Rendering
{
    public class PrintRenderer
    {
        static ILog log = LogManager.GetLogger(typeof(PrintRenderer));

        private string _templatePath;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="templatePath">Path to the file containing the template that is to be rendered.</param>
        /// <exception cref="ArgumentNullException">Thrown if the template file is not specified.</exception>
        public PrintRenderer(string templatePath)
        {
            if(String.IsNullOrWhiteSpace(templatePath))
            {
                log.Error($"{nameof(templatePath)} not set.");
                throw new ArgumentNullException(nameof(templatePath));
            }

            _templatePath = templatePath;
        }

        /// <summary>
        /// Renders a trial results page.
        /// </summary>
        /// <param name="trials">The JSON data structure returned by a successful call to the Clinical Trials API.</param>
        /// <param name="criteria">The search criteria as print-ready label/value pairs.</param>
        /// <param name="locationData">A parsed version of the location search criteria.</param>
        /// <returns>A blob of HTML containing the printable page.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <cref>trials</cref> or <cref>criteria</cref>
        /// is not set.</exception>
        public string Render(JObject trials, SearchCriteria criteria, LocationCriteria locationData, string linkTemplate, string newSearchLink)
        {
            if (trials == null)
            {
                log.Error($"{nameof(trials)} not set.");
                throw new ArgumentNullException(nameof(trials));
            }
            if (criteria == null)
            {
                log.Error($"{nameof(criteria)} not set.");
                throw new ArgumentNullException(nameof(criteria));
            }
            if(String.IsNullOrWhiteSpace(linkTemplate) || linkTemplate[0] != '/')
            {
                string message = $"The value of {nameof(linkTemplate)} must be a relative link. ('{linkTemplate}')";
                log.Error(message);
                throw new ArgumentException(message, nameof(linkTemplate));
            }
            if (String.IsNullOrWhiteSpace(newSearchLink) || newSearchLink[0] != '/')
            {
                string message = $"The value of {nameof(newSearchLink)} must be a relative link. ('{newSearchLink}')";
                log.Error(message);
                throw new ArgumentException(message, nameof(newSearchLink));
            }

            string pageHtml = VelocityTemplate.MergeTemplateWithResultsByFilepath(
                _templatePath,
                 new
                 {
                     Results = trials["data"],
                     Criteria = criteria.Criteria,
                     LocationData = locationData,
                     TrialTools = new TrialVelocityTools(),
                     LinkTemplate = linkTemplate,
                     NewSearchLink = newSearchLink,
                 }
            );

            return pageHtml;
        }
    }
}
