using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;


namespace NCI.Web.Extensions
{
    public static class WebExtensions
    {
        /// <summary>
        /// Loops through the controls collection of a Control to find any child controls
        /// of type T.
        /// </summary>
        /// <remarks>
        /// This method does not support Control objects of type T which are nested inside 
        /// each other.  This method also follows all the normal rules of Control collections.
        /// </remarks>
        /// <typeparam name="T">The type of the control to find.</typeparam>
        /// <param name="ctrl"></param>
        /// <returns>A collection of Control objects which are of type T.</returns>
        public static IEnumerable<Control> FindControlByType<T>(this Control ctrl) 
        {
            List<Control> rtnControls = new List<Control>();

            if (ctrl != null)
            {
                //Loop through the controls of this control
                foreach (Control subCtrl in ctrl.Controls)
                {
                    if (subCtrl is T)
                    {
                        //The current control is the type we are looking for
                        rtnControls.Add(subCtrl);
                    }
                    else
                    {
                        //The current control is not of the type we are looking for,
                        //so call the same method on the current control to check
                        //its child controls.
                        rtnControls.AddRange(subCtrl.FindControlByType<T>());
                    }
                }
            }

            return rtnControls;
        }

        /// <summary>
        /// Loops through the controls collection of a Control to find any child controls
        /// of type T.
        /// </summary>
        /// <remarks>
        /// This method specifically finds nested Control objects of type T.
        /// This method also follows all the normal rules of Control collections.
        /// </remarks>
        /// <typeparam name="T">The type of the control to find.</typeparam>
        /// <param name="ctrl"></param>
        /// <returns>A collection of Control objects which are of type T.</returns>
        public static IEnumerable<Control> FindAllControlsByType<T>(this Control ctrl)
        {
            List<Control> rtnControls = new List<Control>();

            if (ctrl != null)
            {
                //Loop through the controls of this control
                foreach (Control subCtrl in ctrl.Controls)
                {
                    if (subCtrl is T)
                    {
                        //The current control is the type we are looking for
                        rtnControls.Add(subCtrl);
                    }
                    
                    //This methods seeks to find all nested controls of the 
                    //given type, so call the same method on the current control
                    //to check its child controls.
                    rtnControls.AddRange(subCtrl.FindAllControlsByType<T>());
                }
            }

            return rtnControls;
        }
    }
}
