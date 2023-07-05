using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.Regions;

namespace Lynx.Console
{
    /// <summary>
    /// Handles the mapping between a <see cref="ShellMenuItemRegion"/> and a <see cref="MenuItem"/> 
    /// </summary>
    [Export]
    public class ShellMenuItemRegionAdapter : RegionAdapterBase<MenuItem>
    {
        /// <summary>
        /// Creates a new instance of a ShellMenuItemRegionAdapter.
        /// </summary>
        /// <remarks>
        /// This constructor is called by the CompositionContainer
        /// </remarks>
        /// <param name="factory">A <see cref="IRegionBehaviorFactory"/> required by the base class</param>
        [ImportingConstructor]
        public ShellMenuItemRegionAdapter([Import] IRegionBehaviorFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// Handles the mapping of a <see cref="MenuItem"/> to a <see cref="ShellMenuItemRegion"/>
        /// </summary>
        /// <param name="region">The region to associate with the control</param>
        /// <param name="regionTarget">The control that will represent the region and its views</param>
        protected override void Adapt(IRegion region, MenuItem regionTarget)
        {
            if (region == null) throw new ArgumentNullException("region");
            if (regionTarget == null) throw new ArgumentNullException("regionTarget");

            bool itemsSourceIsSet = regionTarget.ItemsSource != null;
#if !SILVERLIGHT
            itemsSourceIsSet = itemsSourceIsSet || (BindingOperations.GetBinding(regionTarget, ItemsControl.ItemsSourceProperty) != null);
#endif
            if (itemsSourceIsSet)
            {
                throw new InvalidOperationException(Properties.Resources.ShellMenuItemRegionAdapter_TargetCannotHaveItems);
            }

            // Notify the region about the target
            ShellMenuItemRegion customRegion = region as ShellMenuItemRegion;
            Debug.Assert(customRegion != null);
            customRegion.TargetMenu = regionTarget;
        }

        /// <summary>
        /// Creates a new instance of a region
        /// </summary>
        /// <returns>A new instance of <see cref="ShellMenuItemRegion"/></returns>
        protected override IRegion CreateRegion()
        {
            return new ShellMenuItemRegion();
        }
    }
}
