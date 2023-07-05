using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Esoteric.UI;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Editors;
using Lynx.Interfaces;
using Lynx.Models;
using Lynx.UI.SubsetGenerators;

namespace Lynx.UI.ViewModels
{
    public class LinkSetViewModel : BaseSetViewModel
    {
        #region Constructor/Destructor
        public LinkSetViewModel(IDomainManager domainManager, string tableName)
            : base(domainManager, tableName)
        {
            CompositionInitializer.SatisfyImports(this);

            // Create the delegates
            tableRowChanged = new DataRowChangeEventHandler(TableRowChanged);
            tableRowDeleted = new DataRowChangeEventHandler(TableRowDeleted);
            tableNewRow = new DataTableNewRowEventHandler(TableNewRow);

            // Register for some events
            var table = Table.SourceSet;
            table.RowChanged += tableRowChanged;
            table.RowDeleted += tableRowDeleted;
            table.TableNewRow += tableNewRow;

            table = Table.TargetSet;
            table.RowChanged += tableRowChanged;
            table.RowDeleted += tableRowDeleted;
            table.TableNewRow += tableNewRow;

            // Create the view
            var view = new Views.LinkSetView { Model = this };
            view.Loaded += new RoutedEventHandler(View_Loaded);
            View = view;
        }

        ~LinkSetViewModel()
        {
            // Unregister the events
            var table = Table.SourceSet;
            table.RowChanged -= tableRowChanged;
            table.RowDeleted -= tableRowDeleted;
            table.TableNewRow -= tableNewRow;

            table = Table.TargetSet;
            table.RowChanged -= tableRowChanged;
            table.RowDeleted -= tableRowDeleted;
            table.TableNewRow -= tableNewRow;
        }
        #endregion

        #region IoC Properties

        [Import]
        protected Actions.NewColumn NewColumnAction { get; set; }

        [Import]
        protected Actions.HollerithSort HollerithSortAction { get; set; }

        [Import]
        protected Actions.CopyAsText CopyAsTextAction { get; set; }
        #endregion

        #region Private Properties
        FilterAsNewLinkSet NewLinkSetGenerator
        {
            get
            {
                return newLinkSetGenerator 
                    ?? (newLinkSetGenerator = new FilterAsNewLinkSet { Relationships = Table });
            }
            set
            {
                newLinkSetGenerator = value;
            }
        }
        FilterAsNewLinkSet newLinkSetGenerator;
        #endregion

        #region XAML Binding Properties
        public LinkSet Table
        {
            get
            {
                return DomainManager.LinkSets.Get(Name);
            }
        }

        public ICommand Copy
        {
            get
            {
                return CopyAsTextAction.NoDialogCommand;
            }
        }

        public ICommand NewColumn
        {
            get
            {
                return NewColumnAction.DialogCommand;
            }
        }
        
        public ICommand HollerithSort
        {
            get
            {
                return HollerithSortAction.DialogCommand;
            }
        }

        public ComboBoxItemsProvider AvailableSourcesProvider
        {
            get
            {
                if( availableSourcesProvider == null )
                {
                    availableSourcesProvider = new ComboBoxItemsProvider();
                    availableSourcesProvider.ItemsSource = Table.SourceSet.OrderBy(e => e.Name);
                    availableSourcesProvider.DisplayMemberPath = "Name";
                    availableSourcesProvider.ValuePath = "ID";
                }
                return availableSourcesProvider;
            }
        }
        ComboBoxItemsProvider availableSourcesProvider;

        public ComboBoxItemsProvider AvailableTargetsProvider
        {
            get
            {
                if (availableTargetsProvider == null)
                {
                    availableTargetsProvider = new ComboBoxItemsProvider();
                    availableTargetsProvider.ItemsSource = Table.TargetSet.OrderBy(e => e.Name);
                    availableTargetsProvider.DisplayMemberPath = "Name";
                    availableTargetsProvider.ValuePath = "ID";
                }
                return availableTargetsProvider;
            }
        }
        ComboBoxItemsProvider availableTargetsProvider;

        public RelayCommand AsNewLinkSet
        {
            get
            {
                return asNewLinkSet ?? (asNewLinkSet = new RelayCommand(p => OnNewLinkSet(),
                                                                        p => NewLinkSetGenerator.IsValid()
                                                                        ));
            }
        }
        RelayCommand asNewLinkSet;

        public UserControl NewLinkSetOptions
        {
            get
            {
                return NewLinkSetGenerator.View as UserControl;
            }
        }
        #endregion

        #region Command Handlers
        void OnNewLinkSet()
        {
            var newSet = NewLinkSetGenerator.Generate();
            if (newSet != null)
                DomainManager.LinkSets.Add(newSet);
        }
        #endregion

        #region Event Handlers
        DataRowChangeEventHandler tableRowChanged;
        DataRowChangeEventHandler tableRowDeleted;
        DataTableNewRowEventHandler tableNewRow;

        void View_Loaded(object sender, RoutedEventArgs e)
        {
            // Fill in the grid
            NewLinkSetGenerator.Grid = (View as UserControl).FindFirstControl<XamDataGrid>();
        }

        void TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            RefreshDropdowns();
        }

        void TableRowDeleted(object sender, DataRowChangeEventArgs e)
        {
            RefreshDropdowns();
        }

        void TableRowChanged(object sender, DataRowChangeEventArgs e)
        {
            RefreshDropdowns();
        }

        void RefreshDropdowns()
        {
            availableSourcesProvider = null;
            OnPropertyChanged("AvailableSourcesProvider");
            availableTargetsProvider = null;
            OnPropertyChanged("AvailableTargetsProvider");
        }
        #endregion
    }
}
