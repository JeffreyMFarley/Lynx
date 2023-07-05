using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls;
using Esoteric.UI;
using Lynx.Models;
using Lynx.Interfaces;

namespace Lynx.UI.Dialogs
{
    public class NewLinkSetViewModel : GetNameViewModel
    {
        #region Constructor
        public NewLinkSetViewModel()
            : base()
        {
            CompositionInitializer.SatisfyImports(this);

            DetailsPanel = new NewLinkSetView { Model = this };
        }
        #endregion

        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region XAML Binding Properties
        public UserControl DetailsPanel
        {
            get;
            set;
        }

        public ObservableCollection<EntitySet> AvailableEntities
        {
            get
            {
                if (availableEntities == null)
                {
                    availableEntities = new ObservableCollection<EntitySet>();
                    foreach (var set in ActiveDomain.Manager.EntitySets)
                        availableEntities.Add(set);
                }
                return availableEntities;
            }
        }
        ObservableCollection<EntitySet> availableEntities;

        public EntitySet Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
                OnPropertyChanged("Source");
            }
        }
        EntitySet source;

        public EntitySet Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
                OnPropertyChanged("Target");
            }
        }
        EntitySet target;
        #endregion

        #region Overrides
        public override bool IsValid()
        {
            // Call the base class
            base.IsValid();

            StringBuilder sb = new StringBuilder(ErrorMessage);

            if (Source == null)
                sb.AppendLine("A source must be selected");

            if (Target == null)
                sb.AppendLine("A target must be selected");

            ErrorMessage = sb.ToString();

            return string.IsNullOrEmpty(ErrorMessage);
        }
        #endregion
    }
}
