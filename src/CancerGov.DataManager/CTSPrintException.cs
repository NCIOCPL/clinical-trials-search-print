using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CancerGov.DataManagers
{
    /// <summary>
    /// Abstract base class for all Clinical Trials Search print exceptions.
    /// </summary>
    public abstract class CTSPrintException : Exception
    {
        public int ErrorCode { get; private set; }
        public CTSPrintException() : base() { }
        public CTSPrintException(String message) : base(message) { }
        public CTSPrintException(String message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// This is for the CTS print ID parameter being invalid (not a GUID)
    /// </summary>
    public class InvalidPrintIDException : CTSPrintException
    {
        public InvalidPrintIDException() : base() { }
        public InvalidPrintIDException(string message) : base(message) { }
    }

    /// <summary>
    /// This is for CTS print being attempted without a valid SQL connection
    /// </summary>
    public class DbConnectionException : CTSPrintException
    {
        public DbConnectionException() : base() { }
        public DbConnectionException(string message) : base(message) { }
    }

    /// <summary>
    /// This is for attempts to retrieve print pages based on print ID that doesn't exist
    /// </summary>
    public class PrintIDNotFoundException : CTSPrintException
    {
        public PrintIDNotFoundException() : base() { }
        public PrintIDNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// This is for when fetching a CTS print page fails
    /// </summary>
    public class PrintFetchFailureException : CTSPrintException
    {
        public PrintFetchFailureException() : base() { }
        public PrintFetchFailureException(string message) : base(message) { }
    }


}
