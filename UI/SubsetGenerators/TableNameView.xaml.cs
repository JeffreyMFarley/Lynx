using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.SubsetGenerators
{
    /// <summary>
    /// Interaction logic for TableNameView.xaml
    /// </summary>
    public partial class TableNameView : UserControl, IView
    {
        public TableNameView()
        {
            InitializeComponent();
        }

        #region IView Members
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
