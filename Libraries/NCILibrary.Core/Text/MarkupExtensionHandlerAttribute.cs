using System;

namespace NCI.Text
{
    /// <summary>
    /// An attribute that allows MarkupExtensionHandlers to have documentation attached to them 
    /// that can be read at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MarkupExtensionHandlerAttribute : Attribute
    {
        public string Description { get; private set; }
        public string Usage { get; set; }
        public string[] Parameters { get; set; }


        public MarkupExtensionHandlerAttribute(string description)
        {
            this.Description = description;
            Parameters = new string[] { };
        }
    }
}