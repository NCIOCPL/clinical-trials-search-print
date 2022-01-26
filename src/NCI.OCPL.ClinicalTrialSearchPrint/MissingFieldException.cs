using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NCI.OCPL.ClinicalTrialSearchPrint
{
    /// <summary>
    /// Infrastructure exception for reporting that a required field is not present.
    /// </summary>
    public class MissingFieldException : Exception
    {
        public string FieldName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">The name of the missing field.</param>
        public MissingFieldException(string fieldName)
            : base($"Field '{fieldName}' was not set.")
        => FieldName = fieldName;
    }
}
