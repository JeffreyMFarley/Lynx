using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.Regions;

namespace Lynx.Console
{
    /// <summary>
    /// Provides extra processing for MenuItems defined in an application's shell
    /// </summary>
    public class ShellMenuItemRegion : Region
    {
        /// <summary>
        /// Holds the <see cref="MenuItem"/> in the shell that this region maps to
        /// </summary>
        public MenuItem TargetMenu { get; set; }

        /// <summary>
        /// Overrides the default region behavior to insert menu items at a higher-level 
        /// </summary>
        /// <param name="view">Must be a <see cref="Menu"/> or <see cref="MenuItem"/></param>
        /// <param name="viewName">The name of the view</param>
        /// <param name="createRegionManagerScope">TRUE if the region should be created within the scope of the RegionManager</param>
        /// <returns>The updated region manager</returns>
        public override IRegionManager Add(object view, string viewName, bool createRegionManagerScope)
        {
            MenuItem viewAsMenuItem = view as MenuItem;
            Menu viewAsMenu = view as Menu;

            // Add the children of the container
            if (viewAsMenuItem != null)
            {
                TransferMenuItems(viewAsMenuItem.Items, TargetMenu.Items, viewAsMenuItem.DataContext);
            }
            else if (viewAsMenu != null)
            {
                TransferMenuItems(viewAsMenu.Items, TargetMenu.Items, viewAsMenu.DataContext);
            }
            else
            {
                string message = string.Format(Properties.Resources.ShellMenuItemRegion_Format_UnsupportedTypeFound, view.GetType());
                throw new InvalidOperationException(message);
            }

            return base.Add(view, viewName, createRegionManagerScope);
        }

        /// <summary>
        /// Helper function that transfers the items from one XAML control to another
        /// </summary>
        /// <param name="source">The collction to tranfer items from</param>
        /// <param name="target">The colleciton to transfer items to</param>
        /// <param name="sourceDataContext">The object used to provide data context in the source</param>
        static void TransferMenuItems(ItemCollection source, ItemCollection target, object sourceDataContext)
        {
            // Save a copy of the list of child items
            var v = from child in source.OfType<MenuItem>()
                    select child;
            List<MenuItem> children = new List<MenuItem>();
            children.AddRange(v.ToArray());

            // Clear the view
            source.Clear();

            // Put the menu items in the target
            foreach (MenuItem child in children)
            {
                child.DataContext = sourceDataContext;
                target.Add(child);
            }
        }
    }
}
