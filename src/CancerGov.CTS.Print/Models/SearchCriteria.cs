using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CancerGov.CTS.Print.Models
{
    /// <summary>
    /// A single search criterion.
    /// </summary>
    public class Criterion
    {
        /// <summary>
        /// Label for the criterion.
        /// </summary>
        public String Label { get; }

        /// <summary>
        /// Value of the criterion.
        /// </summary>
        public String Value { get; }

        /// <summary>
        /// Constructor for a new criterion.
        /// </summary>
        /// <param name="label">The item's label.</param>
        /// <param name="value">The item's value.</param>
        public Criterion(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }

    /// <summary>
    /// Maintains a collection of search criteria entries. Items are available
    /// from the Criteria list in the same order they were added to the collection.
    /// </summary>
    public class SearchCriteria
    {
        private readonly List<Criterion> criteriaList = new List<Criterion> ();

        /// <summary>
        /// Presents the collection of criteria as a read-only list in
        /// the same order they were added to the collection.
        /// </summary>
        public IReadOnlyList<Criterion> Criteria => criteriaList.AsReadOnly();

        /// <summary>
        /// Indicates whether the collection contains any criteria.
        /// </summary>
        public bool HasCriteria => criteriaList.Count > 0;

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="label">The item's label for display.</param>
        /// <param name="value">The item's value.</param>
        public void Add(string label, string value)
        {
            criteriaList.Add (new Criterion (label, value));
        }
    }
}
