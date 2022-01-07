using System;
using System.Collections.Generic;
using System.Text;

namespace NCI.Data
{
    /// <summary>
    /// This is an attribute to give to methods of a query class to document the stored procedure that it calls.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class UsesSProcAttribute : Attribute
    {
        private string _procName;

        /// <summary>
        /// Initializes a new instance of a UsesSProcAttribute class
        /// </summary>
        /// <param name="procName">The name of the stored procedure that is called</param>
        public UsesSProcAttribute(string procName)
        {
            _procName = procName;
        }

        /// <summary>
        /// Gets the name of the stored procedure that is called
        /// </summary>
        public string ProcName
        {
            get
            {
                return _procName;
            }
        }
    }
}
