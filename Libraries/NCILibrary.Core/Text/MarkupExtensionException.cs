using System;

namespace NCI.Text
{
    public class MarkupExtensionException : Exception
    {
        public MarkupExtensionException()
        {
        }

        public MarkupExtensionException(string message) : base(message)
        {
        }

        public MarkupExtensionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
