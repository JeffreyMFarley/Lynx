using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows;
using Esoteric.UI;

namespace Lynx.UI.ViewModels
{
    [Export]
    public class ProjectMenuViewModel : ViewModelBase
    {
        #region Constructor
        [ImportingConstructor]
        public ProjectMenuViewModel([Import] Actions.NewDomain newDomainAction,
                                    [Import] Actions.OpenDomain openDomainAction,
                                    [Import] Actions.SaveDomain saveDomainAction
                                   )
        {
            // Save the actions
            NewDomainAction = newDomainAction;
            OpenDomainAction = openDomainAction;
            SaveDomainAction = saveDomainAction;

            CommandManager.RegisterClassInputBinding(typeof(FrameworkElement), 
                                                     new InputBinding(NewDomain, 
                                                                      new KeyGesture(Key.N, ModifierKeys.Control)));

            CommandManager.RegisterClassInputBinding(typeof(FrameworkElement),
                                                     new InputBinding(OpenDomain,
                                                                      new KeyGesture(Key.O, ModifierKeys.Control)));

            CommandManager.RegisterClassInputBinding(typeof(FrameworkElement),
                                                     new InputBinding(SaveDomain,
                                                                      new KeyGesture(Key.S, ModifierKeys.Control)));
        }
        #endregion

        #region IoC Properties
        protected IScriptableAction NewDomainAction { get; set; }

        protected IScriptableAction OpenDomainAction { get; set; }

        protected IScriptableAction SaveDomainAction { get; set; }

        [Import(typeof(Actions.NewEntitySet))]
        protected IScriptableAction NewEntitySetAction { get; set; }

        [Import(typeof(Actions.NewLinkSet))]
        protected IScriptableAction NewLinkSetAction { get; set; }

        [Import(typeof(Actions.GenerateSubset))]
        protected IScriptableAction GenerateSubsetAction { get; set; }
        #endregion

        #region Xaml Binding Properties
        public ICommand NewDomain
        {
            get
            {
                return NewDomainAction.DialogCommand;
            }
        }
        
        public ICommand OpenDomain
        {
            get
            {
                return OpenDomainAction.DialogCommand;
            }
        }

        public ICommand SaveDomain
        {
            get
            {
                return SaveDomainAction.DialogCommand;
            }
        }

        public ICommand NewEntitySet
        {
            get
            {
                return NewEntitySetAction.DialogCommand;
            }
        }

        public ICommand NewLinkSet
        {
            get
            {
                return NewLinkSetAction.DialogCommand;
            }
        }

        public ICommand GenerateSubset
        {
            get
            {
                return GenerateSubsetAction.DialogCommand;
            }
        }
        #endregion
    }
}
