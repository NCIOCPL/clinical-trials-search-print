using System;
using System.Collections.Generic;
using System.Text;

namespace NCI.Util
{
  
    /// <summary>
    /// This Class has behaviour that raises Exceptions raised on converting Strings from various other types.
    /// </summary>
    public class NCIStringConversionFailedException : Exception
	{
        /// <summary>
        /// 
        /// </summary>
        public NCIStringConversionFailedException() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg">The message passed into the Custom Exception.</param>
        public NCIStringConversionFailedException(string msg) : base(msg) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg">The message passed into the Custom Exception.</param>
        /// <param name="innerException">The inner Exception for the Custom Exception.</param>
        public NCIStringConversionFailedException(string msg, Exception innerException) : base(msg, innerException) { }
	}
}

