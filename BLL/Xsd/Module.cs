using System.ComponentModel.Composition;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;

namespace Lynx.XsdExchange
{
    [Export(typeof(IModule))]
    public class Module : IModule
    {
        #region IoC Properties
        [Import]
        protected IRegionManager RegionManager { get; set; }
        #endregion

        #region IModule Members
        public void Initialize()
        {
            // Inject views into regions
            RegionManager.RegisterViewWithRegion("MenuImport", typeof(UI.MenuView));
        }
        #endregion
    }
}
