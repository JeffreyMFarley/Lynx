using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.Views
{
    /// <summary>
    /// Interaction logic for VisualizationView.xaml
    /// </summary>
    public partial class VisualizationView : UserControl, IView
    {
        public VisualizationView()
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
