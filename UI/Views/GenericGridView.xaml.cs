using System.Windows.Controls;
using Esoteric.UI;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;

namespace Lynx.UI.Views
{
    public partial class GenericGridView : UserControl, IView
    {
        public GenericGridView()
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

        private void XamDataGrid_FieldLayoutInitialized(object sender, FieldLayoutInitializedEventArgs e)
        {
            if (!e.FieldLayout.IsDefault)
            {
                e.FieldLayout.Settings.AllowAddNew = false;
                e.FieldLayout.Settings.LabelLocation = LabelLocation.SeparateHeader;

                e.FieldLayout.FieldSettings.AllowEdit = false;
                e.FieldLayout.FieldSettings.AllowGroupBy = false;
                e.FieldLayout.FieldSettings.AllowRecordFiltering = false;
            }
        }
    }
}
