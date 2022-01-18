using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.Test.Models.Tests
{
    public class SearchCriteria_HasCriteria
    {
        /// <summary>
        /// A newly created SearchCriteria object should self-report as having no criteria.
        /// </summary>
        [Fact]
        public void InitialState()
        {
            var criteria = new SearchCriteria();

            Assert.False(criteria.HasCriteria);
        }

        /// <summary>
        /// A SearchCriteria object with a single criterion should report has having criteria.
        /// </summary>
        [Fact]
        public void LoadSingle()
        {
            var criteria = new SearchCriteria();
            criteria.Add("The label", "The value");

            Assert.True(criteria.HasCriteria);
        }

        /// <summary>
        /// A SearchCriteria object with multiple criteria should report has having criteria.
        /// </summary>
        [Fact]
        public void LoadMultiple()
        {
            var criteria = new SearchCriteria();
            criteria.Add("First label",  "First value");
            criteria.Add("Second label", "Second value");
            criteria.Add("Third label",  "Third value");
            criteria.Add("Fourth label", "Fourth value");

            Assert.True(criteria.HasCriteria);
        }
    }
}
