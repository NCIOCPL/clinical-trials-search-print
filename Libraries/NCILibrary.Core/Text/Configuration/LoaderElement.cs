using System;
using System.Configuration;

namespace NCI.Text.Configuration
{
    public class LoaderElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)base["type"];
            }
        }
    }
}
