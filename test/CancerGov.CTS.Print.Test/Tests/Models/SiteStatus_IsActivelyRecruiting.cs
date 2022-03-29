using Xunit;

namespace CancerGov.CTS.Print.Models.Tests
{
    public class SiteStatus_IsActivelyRecruiting
    {
        /// <summary>
        /// Verify that <c>status</c> is considered an actively recruiting status.
        /// </summary>
        /// <param name="status">A valid site recruiting status</param>
        [Theory]
        [InlineData("active")]
        [InlineData("approved")]
        [InlineData("enrolling_by_invitation")]
        [InlineData("in_review")]
        [InlineData("temporarily_closed_to_accrual")]
        [InlineData("ACTIVE")]
        [InlineData("active   ")]
        [InlineData("  active")]
        public void ConsideredActive(string status)
        {
            bool isActive = SiteStatus.IsActivelyRecruiting(status);
            Assert.True(isActive);
        }

        /// <summary>
        /// Verify that <c>status</c> is NOT considered an actively recruiting status.
        /// </summary>
        /// <param name="status">A site recruiting status</param>
        [Theory]
        [InlineData("closed_to_accrual")]
        [InlineData("completed")]
        [InlineData("administratively_complete")]
        [InlineData("closed_to_accrual_and_intervention")]
        [InlineData("withdrawn")]
        [InlineData("chicken")]
        [InlineData(" ")]
        [InlineData(null)]
        public void NotConsideredActive(string status)
        {
            bool isActive = SiteStatus.IsActivelyRecruiting(status);
            Assert.False(isActive);
        }

    }
}
