using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.MefExtensions;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Presentation.Regions;
using Esoteric.UI;

namespace Lynx.Console
{
    /// <summary>
    /// Manages the application-level objects and the dependency injection container
    /// </summary>
    /// <remarks>
    /// In Prism, the Bootstrapper has three responsibilities:
    /// <list type="number">
    /// <item>Setup the module catalog, which loads the modules into the applcation</item>
    /// <item>Hold and configure the <a href="http://www.codeproject.com/KB/aspnet/IOCDI.aspx">Inversion of Control</a> container</item>
    /// <item>Creates the <see cref="Shell"/> of the application which acts as the top-level container of all UI elements</item>
    /// </list>
    /// <para>It performs these tasks in the MefBootstrapper.Run() method.</para>
    /// </remarks>
    public class Bootstrapper : MefBootstrapper, IShellOpen
    {
        #region MefBootstrapper Overrides
        //-----------------------------------------------------------------------------
        /// <summary>
        /// Initialize the <see cref="AggregateCatalog"/> with the locations of the export providers
        /// </summary>
        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            // Get the path where the application was launched from
            string launchedPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Add all the Lynx DLLs in the executing directory
            AggregateCatalog.Catalogs.Add(new DirectoryCatalog(launchedPath, "Lynx*.*"));
            AggregateCatalog.Catalogs.Add(new DirectoryCatalog(launchedPath, "Esoteric*.dll"));
        }

        /// <summary>
        /// Initializes the <see cref="CompositionContainer"/>
        /// </summary>
        /// <remarks>
        /// Also initializes the application-wide <see cref="CompositionHost"/> with the <see cref="CompositionContainer"/>
        /// </remarks>
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            // Add the container as an available part
            Container.ComposeExportedValue<CompositionContainer>(Container);

            // Set the composition container in the Host
            CompositionHost.Initialize(Container);
        }

        /// <summary>
        /// Creates the main <see cref="Shell"/> of the application
        /// </summary>
        /// <returns>An instance of Shell</returns>
        protected override DependencyObject CreateShell()
        {
            Shell shell = new Shell();

            if (shell.OkToRun())
            {
                // Show the main screen
                shell.Show();
            }
            else
                shell.Close();

            return shell;
        }

        /// <summary>
        /// Adds custom region adapters to the application
        /// </summary>
        /// <returns>The updated <see cref="RegionAdapterMappings"/></returns>
        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            // Have the base class setup the initial mappings
            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();

            // Create an instance of the ShellMenuItemRegion
            Lazy<ShellMenuItemRegionAdapter> regionAdapter = Container.GetExport<ShellMenuItemRegionAdapter>();
            Container.ComposeParts(regionAdapter);

            // Add this Adapter to the mappings
            mappings.RegisterMapping(typeof(MenuItem), regionAdapter.Value);

            return mappings;
        }

        /// <summary>
        /// Build the Extension modules
        /// </summary>
        protected override void InitializeModules()
        {
            base.InitializeModules();

            // Load the modules the MEF way
            foreach (Lazy<IModule> module in Container.GetExports<IModule>())
            {
                if (!module.IsValueCreated)
                    Container.ComposeParts(module);
                module.Value.Initialize();
            }
        }
        #endregion

        public T Compose<T>()
        {
            var instance = Container.GetExportedValue<T>();
            return instance;
        }

        #region IShellOpen Members

        public bool Open(FileInfo file)
        {
            var opener = Compose<IShellOpen>();
            return opener.Open(file);
        }

        #endregion
    }
}
