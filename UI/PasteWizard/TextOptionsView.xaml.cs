using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.PasteWizard
{
    /// <summary>
    /// Interaction logic for TextOptionsView.xaml
    /// </summary>
    public partial class TextOptionsView : UserControl, IView
    {
        public TextOptionsView()
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
