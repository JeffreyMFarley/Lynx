using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using Esoteric.BLL.Interfaces;
using Esoteric.DAL;
using Esoteric.UI;
using Esoteric.UI.Desktop.Presentation;
using Lynx.Interfaces;
using Lynx.Models;
using Lynx.UI.PasteWizard.ETL;

namespace Lynx.UI.PasteWizard
{
    public class PasteNewEntityViewModel : DialogViewModelBase, ITransformViewModelHost, IDisposable, IProgressUI
    {
        public PasteNewEntityViewModel()
        {
            TextOptionsView = new TextOptionsView { Model = this };
            DestinationView = new NewDestinationView { Model = this };
            MappingOptionsView = new MapNewEntityView { Model = this };
            View = new MainView { Model = this };

            DialogFrame.Width = 480;
            DialogFrame.Height = 480;

            ApplyDefaultMapping();
            LoadTransformPipeline();
            TheProcess.Build(this);
        }

        #region XAML Binding Properties (Input)

        public IView TextOptionsView
        {
            get;
            set; 
        }

        public IView DestinationView
        {
            get;
            set;
        }

        public bool DataHasHeaders
        {
            get
            {
                return InputOptions.DataHasHeaders;
            }
            set
            {
                InputOptions.DataHasHeaders = value;
                OnPropertyChanged("DataHasHeaders");
                ResetInputPreview();
            }
        }

        public bool DelimiterComma
        {
            get
            {
                return InputOptions.DelimiterComma;
            }
            set
            {
                InputOptions.DelimiterComma = value;
                OnPropertyChanged("DelimiterComma");
                ResetInputPreview();
            }
        }

        public bool DelimiterOther
        {
            get
            {
                return InputOptions.DelimiterOther;
            }
            set
            {
                InputOptions.DelimiterOther = value;
                OnPropertyChanged("DelimiterOther");
                ResetInputPreview();
            }
        }

        public bool DelimiterSemicolon
        {
            get
            {
                return InputOptions.DelimiterSemicolon;
            }
            set
            {
                InputOptions.DelimiterSemicolon = value;
                OnPropertyChanged("DelimiterSemicolon");
                ResetInputPreview();
            }
        }

        public bool DelimiterSpace
        {
            get
            {
                return InputOptions.DelimiterSpace;
            }
            set
            {
                InputOptions.DelimiterSpace = value;
                OnPropertyChanged("DelimiterSpace");
                ResetInputPreview();
            }
        }

        public bool DelimiterTab
        {
            get
            {
                return InputOptions.DelimiterTab;
            }
            set
            {
                InputOptions.DelimiterTab = value;
                OnPropertyChanged("DelimiterTab");
                ResetInputPreview();
            }
        }

        public string OtherDelimiters
        {
            get
            {
                return InputOptions.OtherDelimiters;
            }
            set
            {
                InputOptions.OtherDelimiters = value;
                OnPropertyChanged("OtherDelimiters");
                ResetInputPreview();
            }
        }

        public DataView InputPreview
        {
            get
            {
                return TheProcess.Source.View;
            }
        }

        #endregion

        #region XAML Binding Properties (Entity)

        public string TargetName
        {
            get
            {
                return TheProcess.TargetName;
            }
            set
            {
                TheProcess.TargetName = value;
                OnPropertyChanged("TargetName");
            }
        }

        #endregion

        #region XAML Binding Properties (Mapping)

        public IView MappingOptionsView
        {
            get;
            set;
        }

        public ObservableCollection<TransformViewModel> RequiredColumns
        {
            get
            {
                return requiredColumns ?? (requiredColumns = new ObservableCollection<TransformViewModel>());
            }
        }
        ObservableCollection<TransformViewModel> requiredColumns;

        public ObservableCollection<TransformViewModel> OtherColumns
        {
            get
            {
                return otherColumns ?? (otherColumns = new ObservableCollection<TransformViewModel>());
            }
        }
        ObservableCollection<TransformViewModel> otherColumns;

        public RelayCommand AddTransformCommand
        {
            get
            {
                return addTransformCommand ?? (addTransformCommand = new RelayCommand(p => OnAddTransformCommand()));
            }
        }
        RelayCommand addTransformCommand;

        public DataView OutputPreview
        {
            get
            {
                return TheProcess.Target.View;
            }
        }

        #endregion

        #region Models

        public ProcessNewEntity<FormattedClipboardText> TheProcess
        {
            get
            {
                if (theProcess == null)
                {
                    theProcess = new ProcessNewEntity<FormattedClipboardText>();
                }
                return theProcess;
            }
        }
        ProcessNewEntity<FormattedClipboardText> theProcess;
	
        public TextStructureOptions InputOptions
        {
            get
            {
                return TheProcess.Source.Formatter.Options;
            }
        }

        #endregion

        #region IValidation members

        protected override string Validate()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(TargetName))
                sb.AppendLine("Entity name cannot be empty");

            if (TargetName != null && TargetName.Contains(" "))
                sb.AppendLine("Entity name cannot have spaces");

            foreach (var vm in RequiredColumns.Union(OtherColumns))
            {
                if( !vm.IsValid() )
                    sb.AppendLine(vm.ErrorMessage);
            }

            return sb.ToString();
        }

        #endregion

        #region Triggers

        void ResetInputPreview()
        {
            var ipg = (View as MainView).InputPreviewGrid;
            if (BindingOperations.IsDataBound(ipg, DataGrid.ItemsSourceProperty))
                BindingOperations.ClearBinding(ipg, DataGrid.ItemsSourceProperty);

            ApplyDefaultMapping();
            ResetTargetPreview();

            var b = new Binding("InputPreview");
            BindingOperations.SetBinding(ipg, DataGrid.ItemsSourceProperty, b);
        }

        void ApplyDefaultMapping()
        {
            TheProcess.Source.Load();
            var view = TheProcess.Source.View;

            RequiredColumns.Clear();
            OtherColumns.Clear();

            RequiredColumns.Add(new TransformViewModel(view, Domain.IDColumn));
            RequiredColumns.Add(new TransformViewModel(view, Domain.NameColumn));
            RequiredColumns.Add(new TransformViewModel(view, Domain.DescriptionColumn));

            var sourceColumns = TheProcess.Source.Table.Columns.Cast<ObservantColumn>();

            var firstGuidColumn = sourceColumns.FirstOrDefault(x => x.IsGuid);
            var firstKeyColumn = sourceColumns.FirstOrDefault(x => x.IsKey && !x.IsGuid);
            if( firstKeyColumn == null )
                firstKeyColumn = sourceColumns.FirstOrDefault(x => !x.IsGuid);

            foreach (var c in sourceColumns)
            {
                if (firstGuidColumn != null && c.Ordinal == firstGuidColumn.Ordinal)
                    continue;

                if (firstKeyColumn != null && c.Ordinal == firstKeyColumn.Ordinal)
                    continue;

                OtherColumns.Add(new TransformViewModel(view, c.Ordinal));
            }
            
            // Set the callback trigger
            foreach(var vm in RequiredColumns.Union(OtherColumns))
                vm.Host = this;
        }

        void LoadTransformPipeline()
        {
            TheProcess.Pipeline.Clear();
            foreach (var vm in RequiredColumns.Union(OtherColumns).Where(x => x.IsValid()))
            {
                var t = vm.Build();
                if (t != null)
                    TheProcess.Pipeline.Add(t);
            }
        }

        void ResetTargetPreview()
        {
            var opg = (View as MainView).OutputPreviewGrid;
            if (BindingOperations.IsDataBound(opg, DataGrid.ItemsSourceProperty))
                BindingOperations.ClearBinding(opg, DataGrid.ItemsSourceProperty);

            LoadTransformPipeline();            
            TheProcess.Build(this);

            var b = new Binding("OutputPreview");
            BindingOperations.SetBinding(opg, DataGrid.ItemsSourceProperty, b);
        }

        #endregion

        #region Command Handlers

        void OnAddTransformCommand()
        {
            var vm = new TransformViewModel(TheProcess.Source.View, (int?)null) 
            { 
                Host = this 
            };

            OtherColumns.Add(vm);
            ResetTargetPreview();
        }

        #endregion

        #region ITransformViewModelHost Members

        public void Updated(TransformViewModel vm)
        {
            ResetTargetPreview();
        }

        public void Delete(TransformViewModel vm)
        {
            System.Diagnostics.Debug.Assert(OtherColumns.Remove(vm));
            ResetTargetPreview();
        }

        #endregion

        #region IProgressUI Members

        public void Beginning()
        {
        }

        public void SetMessage(string caption)
        {
        }

        public void SetMinAndMax(int min, int max)
        {
        }

        public void Increment()
        {
        }

        public void IncrementTo(int value)
        {
        }

        public System.Threading.WaitHandle CancelEvent
        {
            get { return null; }
        }

        public void Finished()
        {
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                theProcess.Dispose();
            }
        }

        #endregion

    }
}
