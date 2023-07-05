using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Esoteric.Hollerith.Presentation;

namespace Lynx.UI.Dialogs
{
    public enum BoardFillOptions
    {
        None = 0,
        GenerateBinsFromData = 1,
        PopulateBins = 2
    }

    public class HollerithUsage : INotifyPropertyChanged
    {
        public HollerithUsage(string fieldName, Type fieldType)
        {
            FieldName = fieldName;
            FieldType = fieldType;
        }

        #region XAML Binding Properties

        public string FieldName { get; private set; }

        public Type FieldType { get; private set; }

        public ObservableCollection<SortingPanelToken> AvailablePanels
        {
            get
            {
                if (availablePanels == null)
                {
                    availablePanels = new ObservableCollection<SortingPanelToken>();
                    foreach (var p in SortingPanelFactory.AvailablePanels(FieldType))
                        availablePanels.Add(p);
                }
                return availablePanels;
            }
        }
        ObservableCollection<SortingPanelToken> availablePanels;

        public SortingPanelToken SelectedPanel
        {
            get
            {
                return selectedPanel;
            }
            set
            {
                selectedPanel = value;
                OnPropertyChanged("SelectedPanel");
                OnPropertyChanged("Use");

            }
        }
        SortingPanelToken selectedPanel = SortingPanelFactory.NoBoard;

        public ObservableCollection<BoardFillOptions> AvailableFillOptions
        {
            get
            {
                if (availableFillOptions == null)
                {
                    availableFillOptions = new ObservableCollection<BoardFillOptions>();
                    foreach (var e in Enum.GetValues(typeof(BoardFillOptions)).Cast<BoardFillOptions>())
                    {
                        availableFillOptions.Add(e);
                    }
                }
                return availableFillOptions;
            }
        }
        ObservableCollection<BoardFillOptions> availableFillOptions;

        public BoardFillOptions SelectedFillOption
        {
            get
            {
                return selectedFillOption;
            }
            set
            {
                selectedFillOption = value;
                OnPropertyChanged("SelectedFillOption");
            }
        }
        BoardFillOptions selectedFillOption = BoardFillOptions.PopulateBins;

        public bool Use
        {
            get
            {
                return SelectedPanel != SortingPanelFactory.NoBoard;
            }
            set
            {
                if (value && AvailablePanels.Count > 1)
                    SelectedPanel = AvailablePanels[1];
                else if( !value )
                    SelectedPanel = SortingPanelFactory.NoBoard;
                OnPropertyChanged("Use");
            }
        }
        bool use;
	

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Raised whenever a property is changed in the view model
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Handles the logic of raising the PropertyChanged event
        /// </summary>
        /// <param name="info">The name of the property that was changed</param>
        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
