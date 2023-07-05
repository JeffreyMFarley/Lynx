using System.Text;
using Esoteric.UI;
using Lynx.Interfaces;

namespace Lynx.UI.ViewModels
{
    abstract public class BaseSetViewModel : ViewModelBase
    {
        #region Constructor
        protected BaseSetViewModel(IDomainManager domainManager, string tableName)
        {
            // Save the interface
            DomainManager = domainManager;

            Name = tableName;
        }
        #endregion

        #region Properties
        protected IDomainManager DomainManager { get; set; }
        #endregion

        #region XAML Binding Properties
        public string Name { get; set; }
        #endregion

        #region Object Members
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
