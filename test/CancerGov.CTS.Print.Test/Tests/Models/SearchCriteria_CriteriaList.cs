using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.Test.Models.Tests
{
    public class SearchCriteria_CriteriaList
    {
        /// <summary>
        /// A newly created SearchCriteria object should have no criteria.
        /// </summary>
        [Fact]
        public void InitialState()
        {
            var criteria = SearchCriteriaFactory.Create(null);

            IReadOnlyList<Criterion> list = criteria.Criteria;
            Assert.Empty(list);
        }

        /// <summary>
        /// Items in the criterial list should appear in the order in which
        /// they were added to the collection.
        /// </summary>
        [Fact]
        public void ListContents()
        {
            var criteria = SearchCriteriaFactory.Create(null);
            criteria.Add("First label",  "First value");
            criteria.Add("Second label", "Second value");
            criteria.Add("Third label",  "Third value");
            criteria.Add("Fourth label", "Fourth value");

            IReadOnlyList<Criterion> list = criteria.Criteria;

            Assert.Equal(4, list.Count);

            Criterion item;

            item = list[0];
            Assert.Equal("First label", item.Label);
            Assert.Equal("First value", item.Value);

            item = list[3];
            Assert.Equal("Fourth label", item.Label);
            Assert.Equal("Fourth value", item.Value);
        }
    }
}
