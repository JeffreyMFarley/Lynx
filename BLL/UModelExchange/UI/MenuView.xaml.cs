using System.ComponentModel.Composition;
using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UModelExchange.UI
{
    /// <summary>
    /// Interaction logic for MenuView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MenuView : Menu, IView
    {
        public MenuView()
        {
            InitializeComponent();
        }

        #region IView Members
        /// <summary>
        /// Holds a reference to the backing view model
        /// </summary>
        [Import(typeof(MenuViewModel))]
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
