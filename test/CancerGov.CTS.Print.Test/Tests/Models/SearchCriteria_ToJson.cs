
using Newtonsoft.Json.Linq;
using Xunit;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.Test.Tests.Models
{
    public class SearchCriteria_ToJson
    {
        [Fact]
        public void EmptyList()
        {
            JToken expected = JToken.Parse("[]");

            var criteria = SearchCriteriaFactory.Create(null);
            string json = criteria.ToJson();
            JToken actual = JToken.Parse(json);

            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }

        [Fact]
        public void SingleListEntry()
        {
            JToken expected = JToken.Parse(@"
            [
                {""Label"":""Label 1"",""Value"":""Value 1""}
            ]");

            var criteria = SearchCriteriaFactory.Create(null);
            criteria.Add("Label 1", "Value 1");
            string json = criteria.ToJson();
            JToken actual = JToken.Parse(json);

            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }

        [Fact]
        public void MultipleListEntries()
        {
            JToken expected = JToken.Parse(@"
            [
                {""Label"":""Label 1"",""Value"":""Value 1""},
                {""Label"":""Label 2"",""Value"":""Value 2""},
                {""Label"":""Label 3"",""Value"":""Value 3""}
            ]");

            var criteria = SearchCriteriaFactory.Create(null);
            criteria.Add("Label 1", "Value 1");
            criteria.Add("Label 2", "Value 2");
            criteria.Add("Label 3", "Value 3");
            string json = criteria.ToJson();
            JToken actual = JToken.Parse(json);

            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }
    }
}
