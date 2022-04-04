using System;
using System.IO;
using System.Web;

using Common.Logging;
using Newtonsoft.Json.Linq;
using NVelocity;
using NVelocity.App;

namespace CancerGov.CTS.Print.Rendering
{
    public class VelocityTemplate
    {
        static ILog log = LogManager.GetLogger(typeof(VelocityTemplate));

        private static VelocityEngineManager _engineManager = new VelocityEngineManager();

        public static string MergeTemplateWithResultsByFilepath(string filepath, object obj)
        {
            try
            {
                VelocityEngine velocity = _engineManager.Engine;
                _engineManager.WatchTemplateDirectory(filepath);

                velocity.Init();

                // Set up to pass the data and various helper objects to velocity.
                VelocityContext context = new VelocityContext();
                context.Put("PageData", obj);
                context.Put("CDEContext", new CDEContext());
                context.Put("Tools", new VelocityTools());

                // Load the template.
                StreamReader sr = new StreamReader(filepath);
                string template = sr.ReadToEnd();
                sr.Close();

                // Do the actual rendering.
                StringWriter writer = new StringWriter();
                velocity.Evaluate(context, writer, "", template);
                return writer.GetStringBuilder().ToString();
            }
            catch (Exception ex)
            {
                log.Error("MergeTemplateWithResultsByFilepath(): Failed when evaluating results template and object.", ex);
                throw;
            }
        }

        class CDEContext
        {
            public string Language { get; set; }
            public CDEContext()
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "es")
                {
                    Language = "es";
                }
                else
                {
                    Language = "en";
                }
            }
        }

        /// <summary>
        /// Helper Class that is bound to all Velocity Template contexts
        /// </summary>
        public class VelocityTools
        {
            /// <summary>
            /// Deterines if the object is null or not.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public bool IsNull(object obj)
            {
                return obj == null;
            }

            /// <summary>
            /// Expose String.IsNullOrWhitespace for use in velocity templates.
            /// </summary>
            /// <param name="str">The string to check.</param>
            /// <returns>True if the string is null, the empty string, or contains only whitespace.</returns>
            public bool IsNullOrWhitespace(string str)
            {
                return String.IsNullOrWhiteSpace(str);
            }

            /// <summary>
            /// Overload of IsNullOrWhitespace for checking whether JSON values are empty or whitespace.
            /// </summary>
            /// <param name="str">The JSON value to check.</param>
            /// <returns>True if the value is null, or contains either the empty string or white space.</returns>
            public bool IsNullOrWhitespace(JValue str)
            {
                // Velocity is passing JSON objects so we have to extract the underlaying value.
                // Techincally, this might not be a string; in that case, we'll do the
                // NullOrWhitspace check on the string form of whatever the heck was passed in.
                Object value = str?.Value;
                return value == null || this.IsNullOrWhitespace(value.ToString());
            }

            /// <summary>
            /// Returns a new string in which all occurences of oldValue are replaced
            /// with the contents of newValue. This is a wrapper for String.Replace().
            /// </summary>
            /// <param name="str">The string to be operated on.</param>
            /// <param name="oldValue">The substring to be replaced.</param>
            /// <param name="newValue">The replacement value.</param>
            /// <returns>A copy of str in which all instances of oldValue have been replaced with newValue.</returns>
            public string Replace(string str, string oldValue, string newValue)
            {
                string rtn = str.Replace(oldValue, newValue);
                return rtn;
            }

            /// <summary>
            /// Returns the current date.
            /// </summary>
            /// <param name="format">Format string using the same specification
            /// as <see cref="System.DateTime.ToString(string)"/> </param>
            /// <returns></returns>
            public string GetDate(string format = null)
            {
                DateTime now = DateTime.Now;

                if(String.IsNullOrWhiteSpace(format))
                    return now.ToString();
                else
                    return now.ToString(format);
            }

            /// <summary>
            /// Encodes <c>str</c>, replacing any special characters with HTML encoded equivalents.
            /// Wrapper for <see cref="System.Web.HttpUtility.HtmlEncode(string)"/>.
            /// </summary>
            /// <param name="str">A JSON string to be encoded.</param>
            /// <returns>A web-safe version of <c>str</c>.</returns>
            public string HtmlEncode(JValue str)
            {
                string val = str?.Value<string>();
                return HtmlEncode(val);
            }

            /// <summary>
            /// Encodes <c>str</c>, replacing any special characters with HTML encoded equivalents.
            /// Wrapper for <see cref="System.Web.HttpUtility.HtmlEncode(string)"/>.
            /// </summary>
            /// <param name="str">The string to be encoded.</param>
            /// <returns>A web-safe version of <c>str</c>.</returns>
            public string HtmlEncode(string str)
            {
                return HttpUtility.HtmlEncode(str);
            }

		}

        /// <summary>
        /// Helper class to manage use of a single instance of the Velocity Engine.
        /// The managed instance of the engine may be released by either calling 
        /// ResetVelocityEngine() directly, or by using WatchTemplateDirectory() to
        /// set a file watcher on the directory where the templates are stored.
        /// 
        /// </summary>
        /// <remarks>
        /// If WatchTemplateDirectory() is used, it is assumed that all templates
        /// are stored in the same directory structure.  Although changes to files
        /// in the directory structure will trigger replacement of the managed engine,
        /// create, delete, rename operations on directories will not.
        /// </remarks>
        private class VelocityEngineManager
        {
            /// <summary>
            /// The instance. Not available for access outside this helper class.
            /// </summary>
            private static VelocityEngine _velocityEngine = null;

            /// <summary>
            ///  Used by WatchTemplateDirectory() to watch for changes in the
            ///  directory structure where templates are stored.
            /// </summary>
            private static FileSystemWatcher templateDirectoryWatcher;

            /// <summary>
            /// Read-only property for accessing the managed instance of VelocityEngine.
            /// By design, use of this property enforces thread-safe access to the engine.
            /// All callers will have their own reference to the managed instance, which
            /// remains valid after a call to ResetVelocityEngine().
            /// </summary>
            public VelocityEngine Engine
            {
                get
                {
                    lock (this)
                    {
                        if (_velocityEngine == null)
                            _velocityEngine = new VelocityEngine();

                        return _velocityEngine;
                    }
                }
            }

            /// <summary>
            /// Thread-safe mechanism for removing the managed VelocityEngine instance.
            /// </summary>
            public void ResetVelocityEngine()
            {
                lock (this)
                {
                    _velocityEngine = null;
                }
            }

            /// <summary>
            /// Sets a file system watcher in the directory which contains filepath. In the event that any files
            /// are created, deleted, modified, or renamed, the current velocity engine will be released and the
            /// next access to the Engine property will result in a new one being allocated.
            /// </summary>
            /// <param name="filepath">The path to a velocity template.</param>
            public void WatchTemplateDirectory(string filepath)
            {
                if (templateDirectoryWatcher == null)
                {
                    lock (typeof(VelocityTemplate.VelocityEngineManager))
                    {
                        if(templateDirectoryWatcher == null)
                        {
                            // Set the watcher on the directory.
                            templateDirectoryWatcher = new FileSystemWatcher((Path.GetDirectoryName(filepath)));
                            templateDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.LastAccess | NotifyFilters.Attributes;
                            templateDirectoryWatcher.EnableRaisingEvents = true;

                            templateDirectoryWatcher.Changed += new FileSystemEventHandler(TemplatesChanged);
                            templateDirectoryWatcher.Created += new FileSystemEventHandler(TemplatesChanged);
                            templateDirectoryWatcher.Deleted += new FileSystemEventHandler(TemplatesChanged);
                            templateDirectoryWatcher.Renamed += new RenamedEventHandler(TemplatesChanged);
                        }
                    }
                }
            }

            /// <summary>
            /// Event handler for files in the template directory being modified, created, or deleted.
            /// Removes the reference to the managed velocity template
            /// </summary>
            /// <param name="src">event source (not used)</param>
            /// <param name="e">event arguments (not used)</param>
            private void TemplatesChanged(object src, FileSystemEventArgs e)
            {
                ResetVelocityEngine();
            }

            /// <summary>
            /// Event handler for files in the template directory being renamed.
            /// Removes the reference to the managed velocity template
            /// </summary>
            /// <param name="src">event source (not used)</param>
            /// <param name="e">event arguments (not used)</param>
            private void TemplatesRenamed(object src, RenamedEventArgs e)
            {
                ResetVelocityEngine();
            }

        }
    }
}
