using System.ComponentModel.Composition;
using System.Windows.Input;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.UI.ViewModels
{
    public class EntitySetViewModel : BaseSetViewModel
    {
        #region Constructor
        public EntitySetViewModel(IDomainManager domainManager, string tableName)
            : base(domainManager, tableName)
        {
            CompositionInitializer.SatisfyImports(this);

            // Create the view
            View = new Views.GenericGridView { Model = this };
        }
        #endregion

        #region IoC Properties
        [Import]
        protected Actions.NewColumn NewColumnAction { get; set; }

        [Import]
        protected Actions.HollerithSort HollerithSortAction { get; set; }

        [Import]
        protected Actions.CopyAsText CopyAsTextAction { get; set; }
        #endregion

        #region XAML Binding Properties
        public EntitySet Table
        {
            get
            {
                return DomainManager.EntitySets.Get(Name);
            }
        }

        public ICommand Copy
        {
            get
            {
                return CopyAsTextAction.NoDialogCommand;
            }
        }

        public ICommand NewColumn
        {
            get
            {
                return NewColumnAction.DialogCommand;
            }
        }

        public ICommand HollerithSort
        {
            get
            {
                return HollerithSortAction.DialogCommand;
            }
        }
        #endregion
    }
}
