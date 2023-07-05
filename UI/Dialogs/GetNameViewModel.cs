using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using Esoteric.UI;

namespace Lynx.UI.Dialogs
{
    public class GetNameViewModel : ViewModelBase, IDialogViewModel
    {
        #region Constructor
        public GetNameViewModel()
        {
            // Notify the view that this is the view model to use
            View = new GetNameView { Model = this, Owner = Application.Current.MainWindow };
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

        public string Name
        {
            get
            {
                return name ?? (name = string.Empty);
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        string name;

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
        /// <summary>
        /// Holds the <see cref="ICommand" /> to call when the OK button is clicked
        /// </summary>
        /// <remarks>
        /// The default implementation creates a <see cref="RelayCommand"/> that calls
        /// <see cref="CanOK"/> and <see cref="OnOK"/>
        /// </remarks>
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

        /// <summary>
        /// Holds the <see cref="ICommand" /> to call when the Cancel button is clicked
        /// </summary>
        /// <remarks>
        /// The default implementation creates a <see cref="RelayCommand"/> that calls
        /// <see cref="CanCancel"/> and <see cref="OnCancel"/>
        /// </remarks>
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
        /// <summary>
        /// Determines if the OK command can be executed
        /// </summary>
        /// <param name="f">The Window that owns the OK button</param>
        /// <returns>TRUE if the OK command can be executed, FALSE otherwise</returns>
        /// <remarks>
        /// The default implementation returns TRUE if the window is not null
        /// </remarks>
        protected virtual bool CanOK(Window f)
        {
            return f != null;
        }

        /// <summary>
        /// Verifies the view model is in a valid state and closes the dialog box
        /// </summary>
        /// <param name="f">The Window that owns the OK button</param>
        /// <remarks>
        /// The default implementation perWindows the following actions:
        /// <list type="number">
        /// <item>Calls <see cref="IsValid"/></item>
        /// <item>If the result is true, the window's <see cref="Window.DialogResult">DialogResult</see> is set to TRUE</item>
        /// <item>Otherwise, the <see cref="ErrorMessage"/> is retrieved and displayed to the user</item>
        /// </list>
        /// </remarks>
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

        /// <summary>
        /// Determines if the Cancel command can be executed
        /// </summary>
        /// <param name="f">The Window that owns the OK button</param>
        /// <returns>TRUE if the Cancel command can be executed, FALSE otherwise</returns>
        /// <remarks>
        /// The default implementation returns TRUE if the window is not null
        /// </remarks>
        protected virtual bool CanCancel(Window f)
        {
            return f != null;
        }

        /// <summary>
        /// Allows for any cleanup or reversion before closing the dialog box
        /// </summary>
        /// <param name="f">The Window that owns the OK button</param>
        /// <remarks>
        /// The default implementation sets the window's <see cref="Window.DialogResult">DialogResult</see> to FALSE
        /// </remarks>
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

            if (string.IsNullOrEmpty(Name))
                sb.AppendLine("Name must be specified");

            if (Name.Contains(" "))
                sb.AppendLine("Name cannot have spaces");

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
