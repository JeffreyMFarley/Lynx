using System.ComponentModel.Composition;
using System.Windows;
using Esoteric.UI;
using Lynx.Interfaces;
using Lynx.Models;
using Lynx.UI.PasteWizard;
using Lynx.UI.PasteWizard.ETL;

namespace Lynx.UI.Actions
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PasteNewEntity : AvalentAction
    {
        #region IoC Properties
        
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }

        #endregion

        public IEntitySetBuilder ConstructedProcess { get; set; }

        protected override bool CanDialogCommand()
        {
            return Clipboard.ContainsText();
        }

        protected override void OnDialogCommand()
        {
            var vm = new PasteNewEntityViewModel();
            if (vm.ShowDialog())
            {
                ConstructedProcess = vm.TheProcess;

                OnNoDialogCommand();
            }
        }

        protected override void OnNoDialogCommand()
        {
            System.Diagnostics.Debug.Assert(ConstructedProcess != null);

            if (ActiveDomain.Manager == null)
                ActiveDomain.Activate(new Domain());

            if (ConstructedProcess.Build(null))
                ActiveDomain.Manager.EntitySets.Add(ConstructedProcess.Target);
        }
    }
}
