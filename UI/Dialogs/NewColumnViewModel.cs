using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;

namespace Lynx.UI.Dialogs
{
    public class NewColumnViewModel : GetNameViewModel
    {
        #region Constructor
        public NewColumnViewModel()
            : base()
        {
            DetailsPanel = new SelectTypeView { Model = this };
        }
        #endregion

        #region XAML Binding Properties
        public UserControl DetailsPanel
        {
            get;
            set;
        }

        public ObservableCollection<Type> AvailableTypes
        {
            get
            {
                if (availableTypes == null)
                {
                    availableTypes = new ObservableCollection<Type>();
                    availableTypes.Add(typeof(string));
                    availableTypes.Add(typeof(int));
                    availableTypes.Add(typeof(bool));
                    availableTypes.Add(typeof(decimal));
                    availableTypes.Add(typeof(double));
                    availableTypes.Add(typeof(float));
                    availableTypes.Add(typeof(Guid));
                }
                return availableTypes;
            }
        }
        ObservableCollection<Type> availableTypes;

        public Type SelectedType
        {
            get
            {
                return selectedType;
            }
            set
            {
                selectedType = value;
                OnPropertyChanged("SelectedType");
            }
        }
        Type selectedType;
        #endregion

        #region Overrides
        public override bool IsValid()
        {
            // Call the base class
            base.IsValid();

            StringBuilder sb = new StringBuilder(ErrorMessage);

            if (SelectedType == null)
                sb.AppendLine("A type must be selected");

            ErrorMessage = sb.ToString();

            return string.IsNullOrEmpty(ErrorMessage);
        }
        #endregion
    }
}
