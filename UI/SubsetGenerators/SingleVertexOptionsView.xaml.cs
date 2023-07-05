using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.SubsetGenerators
{
    /// <summary>
    /// Interaction logic for SingleVertexOptionsView.xaml
    /// </summary>
    public partial class SingleVertexOptionsView : UserControl, IView
    {
        public SingleVertexOptionsView()
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
