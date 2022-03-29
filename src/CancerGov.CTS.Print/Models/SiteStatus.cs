using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CancerGov.CTS.Print.Models
{

    /// <summary>
    /// Site-specific recruitment statuses (not to be confused with the Trial Status).
    /// These are used to filter available study sites on results/view/listing pages.
    /// </summary>
    public static class SiteStatus
    {
        public static readonly string[] ActiveRecruitmentStatuses = {
            // These statuses appear in results:
            "active",
            "approved",
            "enrolling_by_invitation",
            "in_review",
            "temporarily_closed_to_accrual"
            // These statuses DO NOT appear in results:
            /// "closed_to_accrual",
            /// "completed",
            /// "administratively_complete",
            /// "closed_to_accrual_and_intervention",
            /// "withdrawn"
        };

        /// <summary>
        /// Helper method to determine whether a given site status string is considered to be "Actively Recruiting."
        /// </summary>
        /// <param name="status">a site status value</param>
        /// <returns></returns>
        public static bool IsActivelyRecruiting(string siteStatus)
        {
            return ActiveRecruitmentStatuses.Any(status => !String.IsNullOrWhiteSpace(siteStatus) && status == siteStatus.Trim().ToLower());
        }
    }
}
