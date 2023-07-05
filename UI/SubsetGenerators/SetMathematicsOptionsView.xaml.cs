using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.SubsetGenerators
{
    /// <summary>
    /// Interaction logic for SetMathematicsOptionsView.xaml
    /// </summary>
    public partial class SetMathematicsOptionsView : UserControl, IView
    {
        public SetMathematicsOptionsView()
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
