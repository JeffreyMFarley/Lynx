using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Esoteric.DAL;
using Esoteric.UI;
using Lynx.Models;
using Lynx.UI.PasteWizard.ETL;

namespace Lynx.UI.PasteWizard
{
    public class TransformViewModel : IViewModel, IValidation, INotifyPropertyChanged
    {
        #region Constructors

        public TransformViewModel(DataView source, string requiredTargetColumn)
        {
            TargetColumnName = requiredTargetColumn;
            IsTargetColumnEditable = false;

            var sourceColumns = source.Table.Columns.Cast<ObservantColumn>();

            // Handle the ID column
            if (requiredTargetColumn == Domain.IDColumn )
            {
                AvailableTransforms.Add(new GenerateGuid());

                foreach(var c in sourceColumns.Where(x => x.IsGuid))
                    AvailableTransforms.Add(new Copy(c.ColumnName));

                if( AvailableTransforms.Count > 1)
                    SelectedTransform = AvailableTransforms[1];
                else
                    SelectedTransform = AvailableTransforms[0];
            }

            // Handle the Name column
            else if (requiredTargetColumn == Domain.NameColumn)
            {
                var keyColumn = sourceColumns.FirstOrDefault(x => x.IsKey && !x.IsGuid);
                if (keyColumn == null)
                    keyColumn = sourceColumns.FirstOrDefault(x => !x.IsGuid);

                ITransform t = new NewEmpty();
                AvailableTransforms.Add(t);

                ITransform defaultSelection = null;
                foreach (var c in sourceColumns)
                {
                    t = new Copy(c.ColumnName);
                    AvailableTransforms.Add(t);

                    if (keyColumn != null && c.Ordinal == keyColumn.Ordinal)
                        defaultSelection = t;
                }

                SelectedTransform = (defaultSelection == null) ? AvailableTransforms[0] : defaultSelection;
            }

            // Handle the Description Column
            else
            {
                AvailableTransforms.Add(new NewEmpty());

                foreach (var c in sourceColumns)
                    AvailableTransforms.Add(new Copy(c.ColumnName));

                SelectedTransform = AvailableTransforms[0];
            }

            IsDataTypeVisible = false;
        }
        
        public TransformViewModel(DataView source, int? defaultColumn)
        {
            var sourceColumns = source.Table.Columns.Cast<ObservantColumn>();

            ITransform t = new NewEmpty();
            AvailableTransforms.Add(t);

            ITransform defaultSelection = null;
            foreach (var c in sourceColumns)
            {
                t = new Copy(c.ColumnName);
                AvailableTransforms.Add(t);

                if (defaultColumn != null && c.Ordinal == defaultColumn.Value)
                { 
                    defaultSelection = t;
                    TargetColumnName = c.ColumnName;
                }
            }

            foreach (var c in sourceColumns)
            {
                t = new TagsToFlags(c.ColumnName);
                AvailableTransforms.Add(t);
            }

            SelectedTransform = (defaultSelection == null) ? AvailableTransforms[0] : defaultSelection;
        }

        #endregion

        #region IViewModel Members

        public IView View
        {
            get
            {
                return view ?? (view = new TransformView { Model = this });
            }
            set
            {
                view = value;
            }
        }
        IView view;
	
        #endregion

        #region XAML Binding Properties

        public ObservableCollection<ITransform> AvailableTransforms
        {
            get
            {
                if (availableTransforms == null)
                {
                    availableTransforms = new ObservableCollection<ITransform>();
                }
                return availableTransforms;
            }
        }
        ObservableCollection<ITransform> availableTransforms;

        public ITransform SelectedTransform
        {
            get
            {
                return selectedTransform;
            }
            set
            {
                selectedTransform = value;
                OnPropertyChanged("SelectedTransform");
                
                if( selectedTransform == null )
                    return;

                // Check and change flags
                var type = selectedTransform.GetType();
                var supportTargetName = type.GetProperty("TargetColumn");
                IsTargetColumnVisible = (supportTargetName != null);

                var supportTargetDataType = type.GetProperty("TargetType");
                IsDataTypeVisible = (supportTargetDataType != null);
            }
        }
        ITransform selectedTransform;

        public string TargetColumnName
        {
            get
            {
                return targetColumnName;
            }
            set
            {
                targetColumnName = value;
                OnPropertyChanged("TargetColumnName");
            }
        }
        string targetColumnName;

        public bool IsTargetColumnEditable
        {
            get
            {
                return isTargetColumnEditable;
            }
            set
            {
                isTargetColumnEditable = value;
                OnPropertyChanged("IsTargetColumnEditable");
            }
        }
        bool isTargetColumnEditable = true;

        public bool IsTargetColumnVisible
        {
            get
            {
                return isTargetColumnVisible;
            }
            set
            {
                isTargetColumnVisible = value;
                OnPropertyChanged("IsTargetColumnVisible");
            }
        }
        bool isTargetColumnVisible = true;
	
        public ObservableCollection<DataType> AvailableDataTypes
        {
            get
            {
                if (availableDataTypes == null)
                {
                    availableDataTypes = new ObservableCollection<DataType>();
                    foreach (var dt in Enum.GetValues(typeof(DataType)).OfType<DataType>())
                        availableDataTypes.Add(dt);
                }
                return availableDataTypes;
            }
        }
        ObservableCollection<DataType> availableDataTypes;

        public DataType SelectedDataType
        {
            get
            {
                return selectedDataType;
            }
            set
            {
                selectedDataType = value;
                OnPropertyChanged("SelectedDataType");
            }
        }
        DataType selectedDataType;

        public bool IsDataTypeVisible
        {
            get
            {
                return isDataTypeVisible;
            }
            set
            {
                isDataTypeVisible = value && IsTargetColumnEditable;
                OnPropertyChanged("IsDataTypeVisible");
            }
        }
        bool isDataTypeVisible = true;

        public RelayCommand DeleteCommand
        {
            get
            {
                return deleteCommand ?? (deleteCommand = new RelayCommand(c => OnDelete(),
                                                                          c => CanDelete()));
            }
        }
        RelayCommand deleteCommand;

        #endregion

        #region Command Handlers

        bool CanDelete()
        {
            return IsTargetColumnEditable && Host != null;
        }

        void OnDelete()
        {
            if( Host != null)
                Host.Delete(this);
        }

        #endregion

        #region Transform Support Methods

        public ITransformViewModelHost Host { get; set; }

        public ITransform Build()
        {
            if (SelectedTransform == null)
                return null;

            // Check object support
            var type = SelectedTransform.GetType();
            var supportTargetName = type.GetProperty("TargetColumn");
            var supportTargetDataType = type.GetProperty("TargetType");

            // Create the instance
            var instance = SelectedTransform.Clone() as ITransform;
            if( IsTargetColumnVisible && supportTargetName != null )
                supportTargetName.SetValue(instance, TargetColumnName, null);

            if( IsDataTypeVisible && supportTargetDataType != null )
                supportTargetDataType.SetValue(instance, SelectedDataType, null);

            return instance;
        }

        #endregion
        
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised whenever a property is changed in the view model
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handles the logic of raising the PropertyChanged event
        /// </summary>
        /// <param name="info">The name of the property that was changed</param>
        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }

            if (Host != null)
            {
                Host.Updated(this);
            }
        }

        #endregion

        #region IValidation Members

        public string ErrorMessage
        {
            get;
            private set;
        }

        public bool IsValid()
        {
            StringBuilder sb = new StringBuilder();

            if (SelectedTransform == null)
                sb.AppendLine("A transform must be selected");

            if( IsTargetColumnVisible && string.IsNullOrEmpty(TargetColumnName) )
                sb.AppendLine("Target column name cannot be empty");

            if (IsTargetColumnVisible && TargetColumnName != null && TargetColumnName.Contains(" "))
                sb.AppendLine(string.Format("Target column name '{0}' cannot contain spaces", TargetColumnName));

            ErrorMessage = sb.ToString();
            return string.IsNullOrEmpty(ErrorMessage);
        }

        #endregion
    }
}
