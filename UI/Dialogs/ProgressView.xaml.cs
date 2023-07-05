using System.Windows;
using System.ComponentModel.Composition;
using Esoteric.UI;

namespace Lynx.UI.Dialogs
{
    /// <summary>
    /// Presents a dialog box to the user that allows them to create or modify a <see cref="BusinessObjects.Person">Person</see>
    /// </summary>
    public partial class ProgressView : Window, IView
    {
        /// <summary>
        /// Creates the DirectoryView and initializes its controls
        /// </summary>
        public ProgressView()
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
