using System.ComponentModel.Composition;
using Esoteric.UI;
using Lynx.Models;
using Lynx.Interfaces;

namespace Lynx.UI.Actions
{
    [Export]
    public class NewDomain : AvalentAction
    {
        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region AvalentAction members
        protected override void OnDialogCommand()
        {
            // TODO: Prompt to save changes

            OnNoDialogCommand();
        }

        protected override void OnNoDialogCommand()
        {
            ActiveDomain.Activate(new Domain());
        }
        #endregion
    }
}
