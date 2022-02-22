using Newtonsoft.Json.Linq;

namespace CancerGov.CTS.Print.Models.Tests
{
    /// <summary>
    /// Base class for testing retrieval of criteria lists.
    /// </summary>
    public abstract class CriteriaList_Base
    {
        /// <summary>
        /// JSON mock of the request's searchCriteria object.
        /// </summary>
        abstract public JObject MockCriteria { get; }

        /// <summary>
        /// The expected label for the search criteria.
        /// </summary>
        abstract public string ExpectedLabel { get; }

        /// <summary>
        /// The expected value of the criteria.
        /// </summary>
        abstract public string ExpectedCriteriaValue { get; }
    }
}
