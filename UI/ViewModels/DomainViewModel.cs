using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using Esoteric.UI;
using Lynx.Models;
using Lynx.Interfaces;

namespace Lynx.UI.ViewModels
{
    public class DomainViewModel : ViewModelBase
    {
        #region Constructor
        public DomainViewModel(IDomainManager domainManager)
        {
            // Save the interface
            DomainManager = domainManager;

            // Create the delegates
            domainTablesChanged = new CollectionChangeEventHandler(DomainTablesChanged);
            tableCleared += new DataTableClearEventHandler(DomainTableCleared);
            tableColumnChanged = new DataColumnChangeEventHandler(TableColumnChanged);
            tableRowChanged  = new DataRowChangeEventHandler(TableRowChanged);
            tableRowDeleted = new DataRowChangeEventHandler(TableRowDeleted);
            tableNewRow = new DataTableNewRowEventHandler(TableNewRow);

            // Register for some events
            DomainManager.TablesCollectionChanged += domainTablesChanged;

            // Add the entity sets
            foreach (var entitySet in DomainManager.EntitySets)
               Sets.Add(new EntitySetViewModel(DomainManager, entitySet.TableName));

            // Add the link sets
            foreach (var linkSet in DomainManager.LinkSets)
                Sets.Add(new LinkSetViewModel(DomainManager, linkSet.TableName));

            // Create the main view
            View = new Views.WorkspaceView { Model = this };
        }
        #endregion

        #region Private Properties
        IDomainManager DomainManager { get; set; }
        #endregion

        #region XAML Binding Properties
        public QuickGraphViewModel QuickGraph
        {
            get
            {
                return quickGraph ?? (quickGraph = new QuickGraphViewModel(DomainManager));
            }
            set
            {
                quickGraph = value;
            }
        }
        QuickGraphViewModel quickGraph;

        public VisualizationViewModel Visualizations
        {
            get
            {
                return visualizations ?? (visualizations = new VisualizationViewModel(DomainManager));
            }
            set
            {
                visualizations = value;
            }
        }
        VisualizationViewModel visualizations;
	

        public ObservableCollection<BaseSetViewModel> Sets
        {
            get
            {
                return sets ?? (sets = new ObservableCollection<BaseSetViewModel>());
            }
        }
        ObservableCollection<BaseSetViewModel> sets;

        public BaseSetViewModel SelectedSet
        {
            get
            {
                return selectedSet;
            }
            set
            {
                selectedSet = value;
                OnPropertyChanged("SelectedSet");
            }
        }
        BaseSetViewModel selectedSet;
        #endregion

        #region Event Handlers
        CollectionChangeEventHandler domainTablesChanged;
        DataTableClearEventHandler tableCleared;
        DataColumnChangeEventHandler tableColumnChanged;
        DataRowChangeEventHandler tableRowChanged;
        DataRowChangeEventHandler tableRowDeleted;
        DataTableNewRowEventHandler tableNewRow;
        
        void DomainTablesChanged(object sender, CollectionChangeEventArgs e)
        {
            var uiThread = Application.Current.Dispatcher.Thread.ManagedThreadId;
            var currentThread = Thread.CurrentThread.ManagedThreadId;
            if (currentThread != uiThread)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action<object, CollectionChangeEventArgs>(DomainTablesChanged),
                                                           DispatcherPriority.Background, sender, e);
                return;
            }

            if (e.Action == CollectionChangeAction.Add)
            {
                EntitySet entitySet = e.Element as EntitySet;
                if( entitySet != null )
                    Sets.Add(new EntitySetViewModel(DomainManager, entitySet.TableName));

                LinkSet linkSet = e.Element as LinkSet;
                if (linkSet != null)
                    Sets.Add(new LinkSetViewModel(DomainManager, linkSet.TableName));

                //table.TableCleared += tableCleared;
                //table.ColumnChanged += tableColumnChanged;
                //table.RowChanged += tableRowChanged;
                //table.RowDeleted += tableRowDeleted;
                //table.TableNewRow += tableNewRow;
            }
        }

        void DomainTableCleared(object sender, DataTableClearEventArgs e)
        {
        }

        void TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
        }

        void TableRowDeleted(object sender, DataRowChangeEventArgs e)
        {
        }

        void TableRowChanged(object sender, DataRowChangeEventArgs e)
        {
        }

        void TableColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
        }
        #endregion

        #region Destructor
        ~DomainViewModel()
        {
            DomainManager.TablesCollectionChanged -= domainTablesChanged;

            //foreach (var entitySet in DomainManager.EntitySets)
            //{
            //    entitySet.ColumnChanged -= tableColumnChanged;
            //    entitySet.RowChanged -= tableRowChanged;
            //    entitySet.RowDeleted -= tableRowDeleted;
            //    entitySet.TableCleared -= tableCleared;
            //    entitySet.TableNewRow -= tableNewRow;
            //}

            //foreach (var linkSet in DomainManager.LinkSets)
            //{
            //    linkSet.ColumnChanged -= tableColumnChanged;
            //    linkSet.RowChanged -= tableRowChanged;
            //    linkSet.RowDeleted -= tableRowDeleted;
            //    linkSet.TableCleared -= tableCleared;
            //    linkSet.TableNewRow -= tableNewRow;
            //}
        }
        #endregion
    }
}
