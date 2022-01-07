using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCI.DataManager
{
    /// <summary>
    /// Class for Result of Blog Data Manager
    /// </summary>
    public class SeriesPrevNextResult
    {
        /// <summary>
        /// Gets and sets the previous item of this result.
        /// </summary>
        public SeriesItem Previous { get; private set; }
        
        /// <summary>
        /// Gets and sets the next item of this result
        /// </summary>
        public SeriesItem Next { get; private set; }

        public SeriesPrevNextResult()
        {
            this.Previous = null;
            this.Next = null;
        }

        /// <summary>
        /// Series Previous and Next Item
        /// </summary>
        /// <param name="prev">Previous Series Item</param>
        /// <param name="next">Next Series Item</param>
        public SeriesPrevNextResult(SeriesItem prev, SeriesItem next)
        {
            Previous = prev;
            Next = next;
        }

        /// <summary>
        /// Series Item Class
        /// </summary>
        public class SeriesItem
        {
            /// <summary>
            /// Gets and sets the Url for the Series Item
            /// </summary>
            public string Url { get; private set; }

            /// <summary>
            /// Gets and Sets the Title of the Series Item
            /// </summary>
            public string Title { get; private set; }
            
            /// <summary>
            /// Constructs the Series Item
            /// </summary>
            /// <param name="url"></param>
            /// <param name="title"></param>
            public SeriesItem(string url, string title)
            {
                this.Url = url;
                this.Title = title;
            }
        }
    }
}
