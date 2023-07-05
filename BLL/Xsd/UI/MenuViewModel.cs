using System.ComponentModel.Composition;
using System.Windows.Input;
using Esoteric.UI;

namespace Lynx.XsdExchange.UI
{
    [Export]
    public class MenuViewModel : ViewModelBase
    {
        #region IoC Properties
        [Import(typeof(ImportXsd))]
        protected IScriptableAction ImportAction { get; set; }
        #endregion

        #region Xaml Binding Properties
        public ICommand Import
        {
            get
            {
                return ImportAction.DialogCommand;
            }
        }
        #endregion
    }
}
