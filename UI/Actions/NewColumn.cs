using System;
using System.ComponentModel.Composition;
using Esoteric.UI;
using Lynx.Models;

namespace Lynx.UI.Actions
{
    [Export]
    public class NewColumn : UnivalentAction<BaseSet>
    {
        #region Other Properties
        public string ColumnName { get; set; }

        public Type ColumnType { get; set; }
        #endregion


        protected override void OnDialogCommand(BaseSet set)
        {
            var dialog = new Dialogs.NewColumnViewModel();
            dialog.Title = "New Column";
            dialog.Name = "Column";
            dialog.SelectedType = typeof(string);

            if (!dialog.ShowDialog())
                return;

            ColumnName = dialog.Name;
            ColumnType = dialog.SelectedType;
            OnNoDialogCommand(set);
        }

        protected override void OnNoDialogCommand(BaseSet set)
        {
            set.AddColumn(ColumnName, ColumnType);
        }
    }
}
