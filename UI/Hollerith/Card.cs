using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class Card : ViewModelBase, ICard
    {
        public Card(DataRow row)
        {
            Row = row;

            if (row is Entity)
                SourceName = row.Field<string>(Domain.NameColumn);

            else if (row is Link)
            {
                SourceName = row.Field<string>(Domain.SourceNameColumn);
                TargetName = row.Field<string>(Domain.TargetNameColumn);
            }

            // Fill in the cells collection
            var table = row.Table;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var dc = table.Columns[i];
                if (!IsSystemColumn(dc))
                    Cells.Add(new CellViewModel(dc, row));
            }
        }

        static bool IsSystemColumn(DataColumn dc)
        {
            return (Domain.SystemColumnNames.Contains(dc.ColumnName) || !string.IsNullOrEmpty(dc.Expression));
        }

        #region Properties

        DataRow Row
        {
            get;
            set;
        }

        public string this[string columnName]
        {
            get
            {
                var cell = Cells.FirstOrDefault(c => c.Name == columnName);
                return (cell != null) ? cell.Value : null;
            }
            set
            {
                var cell = Cells.FirstOrDefault(c => c.Name == columnName);
                if (cell != null)
                    cell.Value = value;
            }
        }
        #endregion

        #region XAML Binding Properties

        public string SourceName { get; private set; }
        public string TargetName { get; private set; }

        public ObservableCollection<CellViewModel> Cells
        {
            get
            {
                return cells ?? (cells = new ObservableCollection<CellViewModel>());
            }
        }
        ObservableCollection<CellViewModel> cells;
	
        #endregion

        #region ICard Members

        public Visual CreateFace()
        {
            if( Row is Entity )
                return new EntityCard { Model = this };

            if (Row is Link)
                return new LinkCard { Model = this };

            return null;
        }

        #endregion
    }
}
