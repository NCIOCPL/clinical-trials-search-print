using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NCI.Text.Configuration;

namespace NCI.Text
{
    /// <summary>
    /// 
    /// </summary>
    public class MarkupExtensionProcessor
    {
        /// <summary>
        /// The singleton instance of the markup extension processor.
        /// </summary>
        private static MarkupExtensionProcessor _instance;

        /// <summary>
        /// An object instance to use for locking access to _instance.
        /// </summary>
        private static object _syncObject = new object();

        
        /// <summary>
        /// A regular expression that matches instances of markup extension tags 
        /// of the following basic form:
        /// 
        /// {mx:Namespace.Name(parameter 1|parameter 2|...|parameter N)}
        /// 
        /// As an example, here is what a date formatting tag might look like:
        /// 
        /// {mx:HtmlHelper.Date(today|yyyy.mm.dd)}
        /// 
        /// Regex Logic
        /// ---------------------------------------------------------------------------------------
        /// Match a {
        /// Match zero or more whitespace characters
        /// Match me
        /// Match zero or more whitespace characters
        /// Match :
        /// Match zero or more whitespace characters
        /// Begin a match group called "name"
        /// Match the fully qualified name of the tag
        /// Close the "name" match group
        /// Match zero or more whitespace characters
        /// 
        /// At this point have something like "{mx:Namespace.Name" 
        /// or "{ me    :  Namespace.Name "
        /// 
        /// (1) Main Ending
        /// 
        /// Match (
        /// Begin a match group called "parameters"
        /// Match zero or more of anything but )
        /// Close the "parameters" match group
        /// Match )
        /// 
        /// 
        /// (2) Alternate Ending for parameterless tag without explicitly specified empty 
        /// parenthesis 
        /// 
        /// Match }
        /// ---------------------------------------------------------------------------------------
        /// </summary>
        private readonly Regex _markupExtensionRegex = new Regex(@"{\s*mx\s*:\s*(?<name>[\.|\w]+?)\s*\((?<parameters>.*?)\)\s*}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Maintains a lookup table of "Namespace.Function" markup extension tag names to 
        /// a delegate that returns an instance of a markup extension tag handler.  
        /// For example, a markup extension with tag {mx:HtmlHelper.Date()} would have an 
        /// entry mapping the string "HtmlHelper.Date" to delegate that when invoked returns 
        /// an instance of the MarkupExtensions.DateProcessor type.
        /// </summary>
        private Dictionary<string, Func<MarkupExtensionHandler>> _handlers = new Dictionary<string, Func<MarkupExtensionHandler>>();

        /// <summary>
        /// Determines if the processor should actually process the input text to the process method.  
        /// This value is true by default and can be set via configuration so that if something breaks 
        /// or there is horrible performance and we wanted to turn it off we can do so without changing 
        /// any code.
        /// </summary>
        private bool _enabled = true;

        /// <summary>
        /// </summary>
        private bool _returnHandlerErrorsAsOutput = false;


        /// <summary>
        /// Do nothing constructor to prevent instantiation outside the singleton instance.
        /// </summary>
        private MarkupExtensionProcessor()
        {
        }

        /// <summary>
        /// Loads delegates that create markup extension handlers for all configured markup 
        /// extensions.  Delegates are stored in a dictionary so new instances of markup 
        /// extension handlers can be created to handler processing.
        /// </summary>
        private void Initialize()
        {
            // Set local value to store if we want processing enabled or not.  If loading configuration fails, this will remain at its default value of true.
            _enabled = Config.MarkupExtensions.Enabled;

            // Set local value to store if we want to return errors as output.  If loading configuration fails, this will remain at its default value of false.
            _returnHandlerErrorsAsOutput = Config.MarkupExtensions.ReturnHandlerErrorsAsOutput;

            // Make sure there is at least one loader defined in config if processing is enabled.  Otherwise throw an exception per SCR 30531.  Note: this also covers the case where no markupExtensions element is defined at all.  The above code will still work and _enabled will be set to true since that is the default value but the loaders collection will be empty, the same as if the markupExtensions element was defined with no loaders element.  We tried putting an IsRequired = true attribute on the MarkupExtensionLoaders property of the MarkupExtensionsSection class but it did not cause an exception to be thrown when the markupExtensions element wasn't defined.  It seems that IsRequired does not apply to configuration properties that are collections.
            LoaderElementCollection loaders = Config.MarkupExtensions.MarkupExtensionLoaders;
            if ((_enabled == true) && (loaders.Count == 0))
            {
                throw new MarkupExtensionException("The markupExtension configuration element was either not defined or it was defined and processing is enabled but no loaders were defined.");
            }

            // Load up all markup extension loaders.
            foreach (LoaderElement loaderElement in loaders)
            {
                // Get the loader type.
                Type loaderType = Type.GetType(loaderElement.Type);
                if (loaderType == null)
                {
                    throw new MarkupExtensionException(String.Format("Unable to get type information for loader type \"{0}.\"", loaderElement.Type));
                }

                // Create an instance of the loader type.
                MarkupExtensionLoader loader = null;
                try
                {
                    loader = (MarkupExtensionLoader)Activator.CreateInstance(loaderType);
                }
                catch (Exception ex)
                {
                    throw new MarkupExtensionException(String.Format("Unable to create instance of loader type \"{0}.\"", loaderType), ex);
                }

                // Load all the different handlers.
                Dictionary<string, Func<MarkupExtensionHandler>> handlers = loader.GetHandlers();
                foreach (string key in handlers.Keys)
                {
                    // Add the delegate to the dictionary of handlers if it isn't already there.
                    if (_handlers.ContainsKey(key) == false)
                    {
                        _handlers.Add(key, handlers[key]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets individual parameters from a parameter string.
        /// </summary>
        /// <param name="parameterString">A raw parameter string with pipe separators, e.g. "Parameter 1|Parameter 2|...|Parameter N."</param>
        /// <returns>An array of each individual parameter value.</returns>
        private string[] GetParameters(string parameterString)
        {
            string[] parameters = null;

            if (String.IsNullOrEmpty(parameterString) == true)
            {
                // Special case for empty parameters - the split method below will 
                // return a string array with a single element that is an empty string.
                // We want an empty parameter array in this case.  We don't want to 
                // use the StringSplitOptions.RemoveEmptyEntries option however because 
                // we need to support empty optional parameters that have non-empty 
                // parameters after them.
                parameters = new string[] { };
            }
            else
            {
                // Split the string on the pipe character.  Note: we specifically do not want to remove empty entries since a parameter value could be left blank.
                parameters = parameterString.Split(new char[] { '|' }, StringSplitOptions.None);
            }

            return parameters;
        }

        /// <summary>
        /// In the case of an exception, we either want to display the error output 
        /// i.e. the Message property on the exception (so in a preview environment 
        /// the user can see what went wrong) or an empty string (so in a live 
        /// environment the page doesn't have errors displayed - it will still be 
        /// broken since content will just be "missing" but the end user won't see 
        /// an unprocessed tag or an error message).
        /// 
        /// There is no exception handling in this method since it should not be 
        /// possible for this method to throw exceptions.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private string GetErrorOutput(string errorMessage)
        {
            if (_returnHandlerErrorsAsOutput == true)
            {
                // Add "Markup Extension Error: " to the exception text to put output so end users can easily search for this test while still on the preview site to know something has gone wrong.  Didn't add this text to the exception itself since typically when log those errors you'll have access to a lot more info anyway (and will know it is a markup extension error based on the type of the Exception).
                return "Markup Extension Error: " + ((errorMessage == null) ? String.Empty : errorMessage);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Converts an instance of a markup extension to formatted output text.  
        /// If any exceptions or error conditions occur (like not finding the 
        /// requested markup extenstion processor by name) error output is returned 
        /// instead.  Under no circumstances should this method raise an exception 
        /// up to the calling code.  The only thing not protected by the try/catch 
        /// are calls to GetErrorOutput which is designed to not throw exceptions.
        /// </summary>
        /// <param name="name">The tag name of the markup extension, e.g. HtmlHelpers.Date.</param>
        /// <param name="parameters">The parameters specified in the markup extension tag.</param>
        /// <returns>Formatted text to replace the markup extension.</returns>
        private string Process(string name, string[] parameters)
        {
            string output = String.Empty;

            try
            {
                string nameLowered = name.ToLower(); // Keys are normalized to be all lower case.
                if (_handlers.ContainsKey(nameLowered) == true)
                {
                    Func<MarkupExtensionHandler> getHandlerDelegate = _handlers[nameLowered];
                    MarkupExtensionHandler handler = getHandlerDelegate();
                    output = handler.Process(parameters);
                }
                else
                {
                    output = GetErrorOutput(String.Format("Unknown markup extension.  No markup extension with the name \"{0}\" has been loaded.", name));
                }
            }
            catch (MarkupExtensionException mex)
            {
                // Catch any MarkupExtensionExceptions in the code above or from any handlers and output the error response according to "returnHandlerErrorsAsOutput" configuration.
                output = GetErrorOutput(mex.Message);
            }
            catch (Exception ex)
            {
                // Do the same thing as for a MarkupExtensionException for now but in the future when we have better logging, may want to do something different here.
                output = GetErrorOutput(ex.Message);
            }

            return output;
        }

        /// <summary>
        /// Gets the formatted output for the markup extension tag found by the passed in match.
        /// 
        /// There is no exception handling in this method since it should not be 
        /// possible for this method to throw exceptions.  If a match has been 
        /// passed in, it will have matched the regex and therefore the Match and 
        /// the two named groups will all be non-null.  In the case where the tag 
        /// had no parameters, the parameterString will be empty string.  The called 
        /// GetParameters method is designed to not throw exceptions as is the Process 
        /// method (although in this case there is a try/catch that covers the entire 
        /// method.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string GetReplacementTextForMatch(Match match)
        {
            // Get the markup extension tag name and parameters.
            string name = match.Groups["name"].Value;
            string parameterString = match.Groups["parameters"].Value;
            string[] parameters = GetParameters(parameterString);

            // Process markup extension tag.
            return Process(name, parameters);
        }


        /// <summary>
        /// Parses an entire stream of text, such as a full HTML document, finds all 
        /// markup extension tags and processes them by replacing with formatted text.
        /// 
        /// There is no exception handling in this method since it should not be 
        /// possible for this method to throw exceptions.  The only call made is to 
        /// Regex.Replace(string, MatchEvaluator) which according to MSDN documentation 
        /// only throws ArgumentNullException exceptions if the string or MatchEvaluator 
        /// are null.  We explicitly check the string is not null and the evaluator is a 
        /// delegate to a method on this object so it cannot be null.  The only other 
        /// way an exception could happen is if the MatchEvaluator method itself threw 
        /// an exception and that should not be possible either.
        /// </summary>
        /// <param name="textWithMarkupExtensions">Raw text containing markup extension tags.</param>
        /// <returns>Formatted text.</returns>
        public string Process(string textWithMarkupExtensions)
        {
            // If the processor is not enabled (we don't want to do processing) or the input 
            // string is null (processing makes no sense), just return the input, unchanged.
            if ((_enabled == false) || (textWithMarkupExtensions == null))
            {
                return textWithMarkupExtensions;
            }
            
            // The processor is enabled and the input text is not null - process the input text into formatted output text.
            string output = _markupExtensionRegex.Replace(textWithMarkupExtensions, new MatchEvaluator(GetReplacementTextForMatch));

            return output;
        }


        /// <summary>
        /// Provides access to the singleton instance, lazy initializing the markup extensions 
        /// when first accessed.
        /// </summary>
        public static MarkupExtensionProcessor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new MarkupExtensionProcessor();
                            _instance.Initialize();
                        }
                    }
                }

                return _instance;
            }
        }
    }
}