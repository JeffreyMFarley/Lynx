using System.ComponentModel.Composition;
using Esoteric.UI;
using Lynx.Models;
using Lynx.Interfaces;

namespace Lynx.UI.Actions
{
    [Export]
    public class NewEntitySet : AvalentAction, IAdjunctAction<string>
    {
        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region AvalentAction members
        protected override bool CanDialogCommand()
        {
            return ActiveDomain.Manager != null;
        }

        protected override void OnDialogCommand()
        {
            var dialog = new Dialogs.GetNameViewModel();
            dialog.Name = "Table";
            dialog.Title = "New Entity";

            if (!dialog.ShowDialog())
                return;

            Options = dialog.Name;

            OnNoDialogCommand();
        }

        protected override void OnNoDialogCommand()
        {
            EntitySet set = new EntitySet(Options);
            ActiveDomain.Manager.EntitySets.Add(set);
        }
        #endregion

        #region IAdjunctAction<string> Members
        public string Options
        {
            get;
            set;
        }
        #endregion
    }
}
