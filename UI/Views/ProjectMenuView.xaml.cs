using System.ComponentModel.Composition;
using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.Views
{
    /// <summary>
    /// Interaction logic for ProjectMenuView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ProjectMenuView : Menu, IView
    {
        public ProjectMenuView()
        {
            InitializeComponent();
        }

        #region IView Members
        /// <summary>
        /// Holds a reference to the backing view model
        /// </summary>
        [Import(typeof(ViewModels.ProjectMenuViewModel))]
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
