using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Esoteric.Hollerith.Presentation;
using Esoteric.UI;
using Esoteric.UI.Desktop.Presentation;
using Infragistics.Windows.DataPresenter;
using Lynx.Models;
using Lynx.UI.Dialogs;
using Lynx.UI.Hollerith;

namespace Lynx.UI.Actions
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CopyAsText : UnivalentAction<XamDataGrid>
    {
        protected override bool CanDialogCommand(XamDataGrid grid)
        {
            return true;
        }

        protected override void OnDialogCommand(XamDataGrid grid)
        {
            NoDialogCommand.CheckedExecute(grid);
        }

        protected override bool CanNoDialogCommand(XamDataGrid grid)
        {
            if (grid == null)
                return false;

            BaseSet dataSet = grid.DataSource as BaseSet;
            if (dataSet == null)
                return false;

            return true;
        }

        protected override void OnNoDialogCommand(XamDataGrid grid)
        {
            Debug.Assert(grid != null);

            BaseSet dataSet = grid.DataSource as BaseSet;
            if (dataSet == null)
                return;

            var sb = new StringBuilder();

            // Create the header
            for (int col = 0; col < dataSet.Columns.Count; col++)
            {
                if( col > 0 )   sb.Append("\t");
                sb.Append(dataSet.Columns[col].ColumnName);
            }
            sb.AppendLine();

            // Create the list of cards using the filter criteria in the grid
            foreach (var record in grid.RecordManager.GetFilteredInDataRecords())
            {
                var row = (record.DataItem as DataRowView).Row as DataRow;

                for (int col = 0; col < dataSet.Columns.Count; col++)
                {
                    if (col > 0) sb.Append("\t");
                    sb.Append(row[col].ToString());
                }
                sb.AppendLine();
            }

            Clipboard.SetText(sb.ToString());
        }
    }
}
