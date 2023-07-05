using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Esoteric.UI;
using Esoteric.Hollerith.Presentation;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.UI.Dialogs
{
    public class ChooseHollerithBoardsViewModel : ViewModelBase, IDialogViewModel
    {
        #region Constructor

        public ChooseHollerithBoardsViewModel()
        {
            // Notify the view that this is the view model to use
            View = new ChooseHollerithBoardsView { Model = this, Owner = Application.Current.MainWindow };
        }
        
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

        public ObservableCollection<HollerithUsage> Fields
        {
            get
            {
                return fields ?? (fields = new ObservableCollection<HollerithUsage>());
            }
        }
        ObservableCollection<HollerithUsage> fields;

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

        protected virtual bool CanOK(Window w)
        {
            var selected = Fields.Count(f => f.SelectedPanel != SortingPanelFactory.NoBoard);
            return selected > 0;
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
            return true;
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
            return true;
        }

        public bool ShowDialog()
        {
            Nullable<bool> b = (View as Window).ShowDialog();
            return b == true;
        }

        #endregion
    }
}
