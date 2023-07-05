using System.ComponentModel.Composition;
using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.Views
{
    /// <summary>
    /// Interaction logic for EditMenuView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class EditMenuView : Menu, IView
    {
        public EditMenuView()
        {
            InitializeComponent();
        }

        #region IView Members
        /// <summary>
        /// Holds a reference to the backing view model
        /// </summary>
        [Import(typeof(ViewModels.EditMenuViewModel))]
        public IViewModel Model
        {
            get
            {
                return DataContext as IViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        #endregion
    }
}
