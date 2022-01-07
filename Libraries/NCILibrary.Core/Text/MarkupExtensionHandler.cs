using System;

namespace NCI.Text
{
    public abstract class MarkupExtensionHandler
    {
        protected static readonly string TooManyParametersError = "The {0} handler can take a maximum of {1} parameter(s).  Number of parameters actually passed in: {2}.";
        protected static readonly string TooFewParametersError = "The {0} handler requires a minimum {1} parameter(s).  Number of parameters actually passed in: {2}.";
        protected static readonly string RequiredParameterNotSpecifiedOrInvalidError = "The {0} handler requires that parameter \"{1}\" at 0-based parameter index {2} be specified and that it be of type {3}.";

        public abstract string Name { get; }

        public abstract string Process(string[] parameters);
    }
}
