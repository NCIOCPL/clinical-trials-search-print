using System;
using System.Configuration;

namespace NCI.Text.Configuration
{
    [ConfigurationCollection(typeof(LoaderElement), 
        AddItemName = "loader", 
        CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class LoaderElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LoaderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LoaderElement)element).Type;
        }
    }
}
