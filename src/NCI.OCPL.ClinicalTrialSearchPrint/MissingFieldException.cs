using System;

namespace NCI.OCPL.ClinicalTrialSearchPrint
{
    /// <summary>
    /// Infrastructure exception for reporting that a required field is not present.
    /// </summary>
    public class MissingFieldException : ArgumentException
    {
        public MissingFieldException(string message) : base(message){}  

        public MissingFieldException(string message, string paramName) : base(message, paramName){}

    }
}
