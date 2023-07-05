using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using Esoteric.UI;
using Esoteric.Hollerith;
using Lynx.Models;

namespace Lynx.UI.Hollerith
{
    public class CellViewModel : INotifyPropertyChanged
    {
        public CellViewModel(DataColumn column, DataRow row)
        {
            Column = column;
            Row = row;
        }

        public DataColumn Column
        {
            get;
            private set;
        }

        public DataRow Row
        {
            get;
            private set;
        }

        #region XAML Binding Properties

        public string Name
        {
            get
            {
                return Column.ColumnName;
            }
            set
            {
                OnPropertyChanged("Name");
            }
        }

        public string Value
        {
            get
            {
                return Row[Column].ToString();
            }
            set
            {
                Type targetType = Nullable.GetUnderlyingType(Column.DataType) ?? Column.DataType;
                try
                {
                    Row[Column] = (value == null) ? null : Convert.ChangeType(value, targetType);
                }
                catch (FormatException) { }
                
                OnPropertyChanged("Value");
            }
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
        }

        #endregion
    }
}
