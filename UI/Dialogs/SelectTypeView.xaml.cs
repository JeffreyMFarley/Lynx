using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.Dialogs
{
    public partial class SelectTypeView : UserControl, IView
    {
        public SelectTypeView()
        {
            InitializeComponent();
        }

        #region IView Members
        /// <summary>
        /// Holds a reference to the backing view model
        /// </summary>
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
