using Xunit;

namespace CancerGov.CTS.Print.Models.Tests
{
    public class StateNameHelper_GetStateName
    {
        [Theory]
        [InlineData("CA", "California")]
        [InlineData("MD", "Maryland")]
        [InlineData("pa", "Pennsylvania")]
        [InlineData("chicken", "chicken")]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void Lookups(string code, string expected)
        {
            string actual = StateNameHelper.GetStateName(code);

            Assert.Equal(expected, actual);
        }

    }
}
