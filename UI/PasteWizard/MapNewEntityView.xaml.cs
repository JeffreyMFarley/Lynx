using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.PasteWizard
{
    /// <summary>
    /// Interaction logic for MapNewEntityView.xaml
    /// </summary>
    public partial class MapNewEntityView : UserControl, IView
    {
        public MapNewEntityView()
        {
            InitializeComponent();
        }

        #region IView Members

        /// <summary>
        /// Provides the data context for this view
        /// </summary>
        public IViewModel Model
        {
            get
            {
                return this.DataContext as IViewModel;
            }
            set
            {
                this.DataContext = value;
            }
        }

        #endregion
	
    }
}
