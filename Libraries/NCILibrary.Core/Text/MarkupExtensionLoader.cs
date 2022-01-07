using System;
using System.Collections.Generic;

namespace NCI.Text
{
    /// <summary>
    /// Provides access to a dictionary of delegates that create MarkupExtensionLoaders 
    /// keyed by the name of the markup extension.
    /// </summary>
    public abstract class MarkupExtensionLoader
    {
        /// <summary>
        /// Stores the mapping of a normalized (lowered) markup extension name to the delegate 
        /// that can create an instance of a MarkupExtensionHandler.
        /// </summary>
        private Dictionary<string, Func<MarkupExtensionHandler>> _handlers = new Dictionary<string, Func<MarkupExtensionHandler>>();


        /// <summary>
        /// Protected constructor that is called when derived classes are created.  Calls the 
        /// abstract LoadHandlers method so derived classes can add handlers to the _handlers 
        /// dictionary.
        /// </summary>
        protected MarkupExtensionLoader()
        {
            LoadHandlers();
        }


        /// <summary>
        /// Allows derived classes to add handlers to the internal _handlers dictionary, 
        /// normalizing the markup extension name to be lower case.
        /// </summary>
        /// <param name="name">
        /// The name of the markup extension as used in the tag, e.g. HtmlHelpers.Date.
        /// </param>
        /// <param name="handlerDelegate">
        /// A delegate that returns an instance of a MarkupExtensionHandler capable of processing 
        /// the named markup extension.
        /// </param>
        protected void Add(Func<MarkupExtensionHandler> handlerDelegate)
        {
            // Instantiate an instance to get its name.
            MarkupExtensionHandler handler = handlerDelegate();
            string name = handler.Name;

            // Add the delegate to the dictionary, keyed by name.
            _handlers.Add(name.ToLower(), handlerDelegate);
        }

        /// <summary>
        /// Abstract method to be implemented by derived types.  Derived types should call the 
        /// the "Add" method on this class to add all the handlers they manage.
        /// </summary>
        protected abstract void LoadHandlers();


        /// <summary>
        /// Provides access to the internal _handlers dictionary.
        /// </summary>
        /// <returns>A dictionary with the mapping of markup extension name to handler delegate</returns>
        public Dictionary<string, Func<MarkupExtensionHandler>> GetHandlers()
        {
            return _handlers;
        }
    }
}