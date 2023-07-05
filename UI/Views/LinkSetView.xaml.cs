using System;
using System.Windows;
using System.Windows.Controls;
using Esoteric.UI;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;
using Lynx.Models;

namespace Lynx.UI.Views
{
    /// <summary>
    /// Interaction logic for LinkSetView.xaml
    /// </summary>
    public partial class LinkSetView : UserControl, IView
    {
        public LinkSetView()
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
            var sourceProvider = this.FindResource("SourceDropdown") as Style;
            var targetProvider = this.FindResource("TargetDropdown") as Style;

            // Fixup the fields
            foreach (var field in e.FieldLayout.Fields)
            {
                switch (field.Name)
                {
                    case Domain.SourceIDColumn:
                        field.Settings.EditorStyle = sourceProvider;
                        break;

                    case Domain.TargetIDColumn:
                        field.Settings.EditorStyle = targetProvider;
                        break;

                    case Domain.SourceNameColumn:
                    case Domain.TargetNameColumn:
                        field.Visibility = Visibility.Collapsed;
                        break;
                }

            }
        }
    }
}
