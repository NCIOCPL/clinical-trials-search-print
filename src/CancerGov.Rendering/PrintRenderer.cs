using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common.Logging;
using Newtonsoft.Json.Linq;

namespace CancerGov.Rendering
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
        /// <returns>A blob of HTML containing the printable page.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <cref>trials</cref> or <cref>criteria</cref>
        /// is not set.</exception>
        public string Render(JObject trials, SearchCriteria criteria)
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

            string pageHtml = VelocityTemplate.MergeTemplateWithResultsByFilepath(
                _templatePath,
                 new
                 {
                     Results = trials["data"],
                     //SearchDate = searchDate.ToString("M/d/yyyy"),
                     Parameters = criteria.Criteria,
                     //TrialTools = new TrialVelocityTools()
                 }
            );

            return pageHtml;
        }
    }
}
