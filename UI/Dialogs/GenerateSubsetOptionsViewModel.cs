using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Esoteric.UI;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.UI.Dialogs
{
    public class GenerateSubsetOptionsViewModel : ViewModelBase, IDialogViewModel
    {
        #region Constructor
        public GenerateSubsetOptionsViewModel()
            : base()
        {
            CompositionInitializer.SatisfyImports(this);

            // Notify the view that this is the view model to use
            View = new GenerateSubsetOptionsView { Model = this, Owner = Application.Current.MainWindow };
        }
        #endregion

        #region IoC Properties
        [ImportMany]
        protected List<IGenerateSubset> Generators { get; set; }

        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region XAML Binding Properties
        public string Title
        {
            get
            {
                return title ?? (title = string.Empty);
            }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }
        string title;

        public string ErrorMessage
        {
            get
            {
                return errorMessage ?? (errorMessage = string.Empty);
            }
            set
            {
                errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }
        string errorMessage;

        public ObservableCollection<IGenerateSubset> AvailableGenerators
        {
            get
            {
                if (availableGenerators == null)
                {
                    availableGenerators = new ObservableCollection<IGenerateSubset>();
                    foreach (var generator in Generators)
                        availableGenerators.Add(generator);
                }
                return availableGenerators;
            }
        }
        ObservableCollection<IGenerateSubset> availableGenerators;

        public IGenerateSubset SelectedGenerator
        {
            get
            {
                return selectedGenerator;
            }
            set
            {
                selectedGenerator = value;

                if (selectedGenerator != null)
                {
                    selectedGenerator.TableName = TableName;
                    selectedGenerator.Relationships = ActiveDomain.Manager.LinkSets.Get(SelectedLinkSet);
                }

                OnPropertyChanged("SelectedGenerator");
                OnPropertyChanged("GeneratorOptions");
            }
        }
        IGenerateSubset selectedGenerator;

        public ObservableCollection<string> AvailableLinkSets
        {
            get
            {
                if (availableLinkSets == null)
                {
                    availableLinkSets = new ObservableCollection<string>();
                    foreach (var linkSet in ActiveDomain.Manager.LinkSets)
                        availableLinkSets.Add(linkSet.TableName);
                }
                return availableLinkSets;
            }
        }
        ObservableCollection<string> availableLinkSets;

        public string SelectedLinkSet
        {
            get
            {
                return selectedLinkSet;
            }
            set
            {
                selectedLinkSet = value;

                if( selectedGenerator != null )
                    selectedGenerator.Relationships = ActiveDomain.Manager.LinkSets.Get(SelectedLinkSet);

                OnPropertyChanged("SelectedLinkSet");
            }
        }
        string selectedLinkSet;

        public string TableName
        {
            get
            {
                return tableName ?? (tableName = string.Empty);
            }
            set
            {
                tableName = value;

                if (selectedGenerator != null)
                    selectedGenerator.TableName = value;

                OnPropertyChanged("TableName");
            }
        }
        string tableName;

        public UserControl GeneratorOptions
        {
            get
            {
                if (SelectedGenerator == null)
                    return null;

                return SelectedGenerator.View as UserControl;
            }
        }
        #endregion

        #region IDialogViewModel Members
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => OnOK(p as Window), p => CanOK(p as Window));
                }
                return _okCommand;
            }
        }
        RelayCommand _okCommand;

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(p => OnCancel(p as Window), p => CanCancel(p as Window));
                }
                return _cancelCommand;
            }
        }
        RelayCommand _cancelCommand;
        #endregion

        #region Command Handlers
        protected virtual bool CanOK(Window f)
        {
            return f != null;
        }

        protected virtual void OnOK(Window f)
        {
            Debug.Assert(f != null);
            if (IsValid())
                f.DialogResult = true;
            else
            {
                string s = ErrorMessage;
                if (!string.IsNullOrEmpty(s))
                    MessageBox.Show(f, s, f.Title);
            }
        }

        protected virtual bool CanCancel(Window f)
        {
            return f != null;
        }

        protected virtual void OnCancel(Window f)
        {
            Debug.Assert(f != null);
            f.DialogResult = false;
        }
        #endregion

        #region DialogViewModelBase members
        public virtual bool IsValid()
        {
            var sb = new StringBuilder();

            if (SelectedGenerator == null)
            {
                sb.AppendLine("A generator must be selected");
            }
            else if( !SelectedGenerator.IsValid() )
            {
                sb.AppendLine(SelectedGenerator.ErrorMessage);
            }

            ErrorMessage = sb.ToString();

            return string.IsNullOrEmpty(ErrorMessage);
        }

        public bool ShowDialog()
        {
            Nullable<bool> b = (View as Window).ShowDialog();
            return b == true;
        }
        #endregion
    }
}