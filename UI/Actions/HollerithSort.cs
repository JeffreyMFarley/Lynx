using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
    public class HollerithSort : UnivalentAction<XamDataGrid>
    {
        public IList<HollerithUsage> FieldsToSort
        {
            get { return fieldsToSort ?? (fieldsToSort = new List<HollerithUsage>()); }
        }
        List<HollerithUsage> fieldsToSort;

        bool IsSystemColumn(DataColumn dc)
        {
            return (Domain.SystemColumnNames.Contains(dc.ColumnName) || !string.IsNullOrEmpty(dc.Expression) );
        }

        protected override bool CanDialogCommand(XamDataGrid grid)
        {
            if (grid == null)
                return false;

            BaseSet dataSet = grid.DataSource as BaseSet;
            if (dataSet == null)
                return false;

            bool can = false;
            for (int i = 0; i < dataSet.Columns.Count && !can; i++)
            {
                can |= !IsSystemColumn(dataSet.Columns[i]);
            }

            return can;
        }

        protected override void OnDialogCommand(XamDataGrid grid)
        {
            Debug.Assert(grid != null);

            BaseSet dataSet = grid.DataSource as BaseSet;
            if (dataSet == null)
                return;

            var dialog = new ChooseHollerithBoardsViewModel
            {
                 Title = dataSet.TableName
            };

            for (int i = 0; i < dataSet.Columns.Count; i++)
            {
                var dc = dataSet.Columns[i];
                if( !IsSystemColumn(dc) )
                    dialog.Fields.Add(new HollerithUsage(dc.ColumnName, dc.DataType));
            }

            if (dialog.ShowDialog())
            {
                FieldsToSort.Clear();
                fieldsToSort.AddRange(dialog.Fields.Where(f => f.SelectedPanel != SortingPanelFactory.NoBoard));

                NoDialogCommand.CheckedExecute(grid);
            }
        }

        protected override bool CanNoDialogCommand(XamDataGrid grid)
        {
            return FieldsToSort.Count != 0;
        }

        protected override void OnNoDialogCommand(XamDataGrid grid)
        {
            Debug.Assert(grid != null);

            BaseSet dataSet = grid.DataSource as BaseSet;
            if (dataSet == null)
                return;

            var setFrame = new BoardSetFrame();

            // Create the list of cards using the filter criteria in the grid
            var cards = new List<Card>();
            foreach (var record in grid.RecordManager.GetFilteredInDataRecords())
            {
                var row = (record.DataItem as DataRowView).Row as DataRow;
                cards.Add(new Card(row));
            }

            // Create the boards
            foreach (var u in FieldsToSort)
            {
                var dc = dataSet.Columns[u.FieldName];
                Debug.Assert(dc != null);

                var board = new Board { Column = dc };
                board.Initialize(u, cards);

                var vm = SortingPanelFactory.CreatePanel<Card>(u.SelectedPanel);
                vm.Attach(board);

                setFrame.BoardSet.Boards.Add(vm);
            }

            setFrame.ShowDialog();
        }
    }

    internal class BoardSetFrame : DialogViewModelBase
    {
        public BoardSetFrame()
        {
            View = BoardSet.View;
        }

        public BoardSetViewModel<Card> BoardSet
        {
            get
            {
                return boardSet ?? (boardSet = new BoardSetViewModel<Card>());
            }
            set
            {
                boardSet = value;
            }
        }
        BoardSetViewModel<Card> boardSet;	

        protected override string Validate()
        {
            return string.Empty;
        }

        public override bool ShowDialog()
        {
            DialogFrame.Height = 600;
            DialogFrame.Width = 600;

            return base.ShowDialog();
        }
    }
}
