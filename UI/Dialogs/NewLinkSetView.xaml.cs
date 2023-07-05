using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.Dialogs
{
    public partial class NewLinkSetView : UserControl, IView
    {
        public NewLinkSetView()
        {
            InitializeComponent();
        }

        #region IView Members
        /// <summary>
        /// Binds the View to its <see cref="IViewModel"/>.
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
