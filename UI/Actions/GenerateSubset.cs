using System.ComponentModel.Composition;
using Esoteric.UI;
using Lynx.Interfaces;

namespace Lynx.UI.Actions
{
    [Export]
    public class GenerateSubset : AvalentAction
    {
        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region AvalentAction members
        protected override bool CanDialogCommand()
        {
            return ActiveDomain.Manager != null && ActiveDomain.Manager.LinkSets.Count >= 1;
        }

        protected override void OnDialogCommand()
        {
            var dialog = new Dialogs.GenerateSubsetOptionsViewModel();
            dialog.Title = "Generate Subset";

            if (!dialog.ShowDialog())
                return;

            SubsetGenerator = dialog.SelectedGenerator;

            OnNoDialogCommand();
        }

        protected override void OnNoDialogCommand()
        {
            var result = SubsetGenerator.Generate();
            if (result != null)
                ActiveDomain.Manager.LinkSets.Add(result);
        }
        #endregion

        #region Options
        public IGenerateSubset SubsetGenerator { get; set; }
        #endregion
    }
}
