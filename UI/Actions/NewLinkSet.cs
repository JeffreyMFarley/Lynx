using System.ComponentModel.Composition;
using System.Linq;
using Esoteric.UI;
using Lynx.Models;
using Lynx.Interfaces;

namespace Lynx.UI.Actions
{
    [Export]
    public class NewLinkSet : AvalentAction
    {
        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region AvalentAction members
        protected override bool CanDialogCommand()
        {
            return ActiveDomain.Manager != null && ActiveDomain.Manager.EntitySets.Count > 0;
        }

        protected override void OnDialogCommand()
        {
            var dialog = new Dialogs.NewLinkSetViewModel();
            dialog.Name = "Link";
            dialog.Title = "New Link";

            if (!dialog.ShowDialog())
                return;

            TableName = dialog.Name;
            Source = dialog.Source;
            Target = dialog.Target;

            OnNoDialogCommand();
        }

        protected override void OnNoDialogCommand()
        {
            LinkSet set = new LinkSet(Source, Target, TableName);
            ActiveDomain.Manager.LinkSets.Add(set);
        }
        #endregion

        #region Options
        public EntitySet Source
        {
            get;
            set;
        }

        public EntitySet Target
        {
            get;
            set;
        }

        public string TableName
        {
            get;
            set;
        }
        #endregion
    }
}
