using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Lynx.Interfaces;
using Lynx.Models;
using Lynx.UI.ViewModels;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;

namespace Lynx.UI
{
    [Export(typeof(IModule))]
    [Export(typeof(IActiveDomain))]
    public class Module : IModule, IActiveDomain
    {
        #region IoC Properties
        [Import]
        protected IRegionManager RegionManager { get; set; }
        #endregion

        #region IModule Members
        public void Initialize()
        {
            // Inject views into regions
            RegionManager.RegisterViewWithRegion("MenuProject", typeof(Views.ProjectMenuView));
            RegionManager.RegisterViewWithRegion("MenuEdit", typeof(Views.EditMenuView));
        }
        #endregion

        #region IActiveDomain Members
        [Import]
        protected Func<Domain, IDomainManager> DomainManagerFactory { get; set; }

        public IDomainManager Manager
        {
            get { return manager; }
            private set { manager = value; }
        }
        IDomainManager manager;

        public string PathName
        {
            get;
            set;
        }

        DomainViewModel active;

        public bool Activate(Domain newDomain)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            var region = RegionManager.Regions["Workspace"];

            // Remove the old
            if (active != null)
            {
                region.Remove(active.View);
                active = null;
            }

            Manager = DomainManagerFactory(newDomain);

            // Add the new
            if (newDomain != null)
            {
                active = new DomainViewModel(Manager);
                region.Add(active.View);
                region.Activate(active.View);
            }

            Mouse.OverrideCursor = null;

            return true;
        }

        #endregion
    }
}
