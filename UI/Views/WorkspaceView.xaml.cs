using System.ComponentModel.Composition;
using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.Views
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class WorkspaceView : UserControl, IView
    {
        public WorkspaceView()
        {
            InitializeComponent();
        }

        #region IView Members

        /// <summary>
        /// Holds a reference to the backing view model
        /// </summary>
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
