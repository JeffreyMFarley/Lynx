using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows;
using Esoteric.UI;

namespace Lynx.UI.ViewModels
{
    [Export]
    public class EditMenuViewModel : ViewModelBase
    {
        [ImportingConstructor]
        public EditMenuViewModel([Import] Actions.PasteNewEntity pasteNewEntityAction)
        {
            // Save the actions
            PasteNewEntityAction = pasteNewEntityAction;

            CommandManager.RegisterClassInputBinding(typeof(FrameworkElement), 
                                                     new InputBinding(PasteNewEntity, 
                                                                      new KeyGesture(Key.E, ModifierKeys.Control)));
        }

        protected IScriptableAction PasteNewEntityAction { get; set; }

        public ICommand PasteNewEntity
        {
            get
            {
                return PasteNewEntityAction.DialogCommand;
            }
        }
           
    }
}
