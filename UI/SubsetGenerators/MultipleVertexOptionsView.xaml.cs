using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.SubsetGenerators
{
    /// <summary>
    /// Interaction logic for MultipleVertexOptionsView.xaml
    /// </summary>
    public partial class MultipleVertexOptionsView : UserControl, IView
    {
        public MultipleVertexOptionsView()
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
