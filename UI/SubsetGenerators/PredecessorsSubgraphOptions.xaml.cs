using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.SubsetGenerators
{
    public partial class PredecessorsSubgraphOptions : UserControl, IView
    {
        public PredecessorsSubgraphOptions()
        {
            InitializeComponent();
        }

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
	
    }
}
