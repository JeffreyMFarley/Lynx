using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using Esoteric.UI;
using Lynx.Interfaces;
using Lynx.Models;
using Lynx.UI.AGLExtensions;

namespace Lynx.UI.ViewModels
{
    public class VisualizationViewModel : ViewModelBase
    {
        public VisualizationViewModel(IDomainManager domainManager)
        {
            // Save the interface
            DomainManager = domainManager;

            // Create the delegates
            domainTablesChanged = new CollectionChangeEventHandler(DomainTablesChanged);

            // Listen for some events
            DomainManager.TablesCollectionChanged += domainTablesChanged;

            // Add the link sets
            foreach (var linkSet in DomainManager.LinkSets)
                AvailableLinkSets.Add(linkSet.TableName);

            // Set the view
            View = new Views.VisualizationView { Model = this };
        }

        #region Private Properties

        IDomainManager DomainManager { get; set; }

        GraphHost AGLAdapter
        {
            get
            {
                return aglAdapter ?? (aglAdapter = new GraphHost());
            }
        }
        GraphHost aglAdapter;

        LinkSet CurrentLinkSet
        {
            get
            {
                return DomainManager.LinkSets.Get(selectedLinkSet);
            }
        }

        #endregion

        #region XAML Binding Properties

        public ImageSource Canvas
        {
            get
            {
                if (canvas == null)
                {
                    //
                    // Create the Geometry to draw.
                    //
                    GeometryGroup ellipses = new GeometryGroup();
                    ellipses.Children.Add(
                        new EllipseGeometry(new Point(5, 5), 4.5, 2.0)
                        );
                    ellipses.Children.Add(
                        new EllipseGeometry(new Point(5, 5), 2.0, 4.5)
                        );


                    //
                    // Create a GeometryDrawing.
                    //
                    GeometryDrawing aGeometryDrawing = new GeometryDrawing();
                    aGeometryDrawing.Geometry = ellipses;

                    // Paint the drawing with a gradient.
                    aGeometryDrawing.Brush =
                        new LinearGradientBrush(
                            Colors.Blue,
                            Color.FromRgb(204, 204, 255),
                            new Point(0, 0),
                            new Point(1, 1));

                    // Outline the drawing with a solid color.
                    aGeometryDrawing.Pen = new Pen(Brushes.Black, 1);

                    canvas = new DrawingImage(aGeometryDrawing);
                    canvas.Freeze();

                    //var i = new Image();
                    //i.Source = canvas;

                    //var r = new RenderTargetBitmap(300, 300, 96, 96, PixelFormats.Pbgra32);
                    //r.Render(i);

                    //var j = new JpegBitmapEncoder();
                    //j.Frames.Add(BitmapFrame.Create(r));

                }
                return canvas;
            }
        }
        ImageSource canvas;

        public FrameworkElement Host
        {
            get
            {
                return AGLAdapter.Element;
            }
            set
            {
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return refreshCommand ?? (refreshCommand = new RelayCommand(p => OnRefresh()));
            }
        }
        RelayCommand refreshCommand;

        public ObservableCollection<string> AvailableLinkSets
        {
            get
            {
                return availableLinkSets ?? (availableLinkSets = new ObservableCollection<string>());
            }
        }
        ObservableCollection<string> availableLinkSets;

        public string SelectedLinkSet
        {
            get
            {
                return selectedLinkSet ?? (selectedLinkSet = string.Empty);
            }
            set
            {
                selectedLinkSet = value;
                OnPropertyChanged("SelectedLinkSet");
            }
        }
        string selectedLinkSet;

        #endregion

        #region Event Handlers
        CollectionChangeEventHandler domainTablesChanged;

        void DomainTablesChanged(object sender, CollectionChangeEventArgs e)
        {
            var uiThread = Application.Current.Dispatcher.Thread.ManagedThreadId;
            var currentThread = Thread.CurrentThread.ManagedThreadId;
            if (currentThread != uiThread)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action<object, CollectionChangeEventArgs>(DomainTablesChanged),
                                                           DispatcherPriority.Background, sender, e);
                return;
            }

            if (e.Action == CollectionChangeAction.Add)
            {
                LinkSet linkSet = e.Element as LinkSet;
                if (linkSet != null)
                    AvailableLinkSets.Add(linkSet.TableName);
            }
        }

        void OnRefresh()
        {
            AGLAdapter.Render(CurrentLinkSet);
        }
        #endregion
    }
}
