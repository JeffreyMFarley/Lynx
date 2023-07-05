using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Esoteric.BLL.Interfaces;
using Esoteric.UI;

namespace Lynx.UI.Dialogs
{
    /// <summary>
    /// This class provides the generic implementation of the <see cref="IProgressUI"/> interface
    /// </summary>
    /// <remarks>
    /// This class also provides a thread-safe implmentation of <see cref="INotifyPropertyChanged"/>
    /// </remarks>
    [Export(typeof(IProgressUI))]
    public class ProgressViewModel : IViewModel, IProgressUI, INotifyPropertyChanged
    {
        #region Timing Members
        private DateTime startTime;

        public DispatcherTimer Timer
        {
            get
            {
                if (timer == null)
                {
                    timer = new DispatcherTimer();
                    timer.Tick += new EventHandler((o, e) => RefreshElapsedTime());
                    timer.Interval = new TimeSpan(0, 0, 1);
                }
                return timer;
            }
        }
        DispatcherTimer timer;

        void RefreshElapsedTime()
        {
            OnPropertyChanged("ElapsedTime");
        }

        public string ElapsedTime
        {
            get
            {
                return string.Format("{0:mm}:{0:ss}", DateTime.Now - startTime);
            }
        }
        #endregion

        #region Data Properties
        /// <summary>
        /// Holds the visibility status of the error message
        /// </summary>
        public Visibility IsErrorVisible
        {
            get { return isErrorVisible; }
            private set { isErrorVisible = value; OnPropertyChanged("IsErrorVisible"); }
        }
        Visibility isErrorVisible = Visibility.Collapsed;

        /// <summary>
        /// Holds the minimum value of the progress bar
        /// </summary>
        public int ProgressMin
        {
            get
            {
                return progressMin;
            }
            set
            {
                progressMin = value;
                OnPropertyChanged("ProgressMin");
            }
        }
        int progressMin;

        /// <summary>
        /// Holds the maximum value of the progress bar
        /// </summary>
        public int ProgressMax
        {
            get
            {
                return progressMax;
            }
            set
            {
                progressMax = value;
                OnPropertyChanged("ProgressMax");
            }
        }
        int progressMax;

        /// <summary>
        /// Holds the current position of the progress bar
        /// </summary>
        public int ProgressValue
        {
            get
            {
                return progressValue;
            }
            set
            {
                progressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }
        int progressValue;

        /// <summary>
        /// Holds the message to display to the user
        /// </summary>
        public string Message
        {
            get
            {
                return message ?? (message = string.Empty);
            }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }
        string message;
        #endregion

        #region IProgressUI Members
        /// <summary>
        /// Called at the beginning of the process. Reset the values and prepares for progress updates.
        /// </summary>
        virtual public void Beginning()
        {
            (CancelEvent as ManualResetEvent).Reset();
            ProgressValue = ProgressMin = ProgressMax = 0;
            IsErrorVisible = Visibility.Collapsed;

            // Start the timing
            startTime = DateTime.Now;
            Timer.Start();

            // Show the view
            var loadView = new Action(delegate()
            {
                var view = new ProgressView { Model = this, Owner = Application.Current.MainWindow };
                View = view;
                view.ShowDialog();
            });
            Application.Current.Dispatcher.BeginInvoke(loadView, DispatcherPriority.Background);
        }

        /// <summary>
        /// Called to display a message to the user
        /// </summary>
        /// <param name="caption">The message to display</param>
        virtual public void SetMessage(string caption)
        {
            Message = caption;
        }

        /// <summary>
        /// Called to set the minimum and maximum values of the progress bar
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        virtual public void SetMinAndMax(int min, int max)
        {
            ProgressMin = ProgressValue = min;
            ProgressMax = max;
        }

        /// <summary>
        /// Called to indicate a step has been completed
        /// </summary>
        virtual public void Increment()
        {
            ProgressValue += 1;
        }

        /// <summary>
        /// Called to indicate a step has been completed
        /// </summary>
        virtual public void IncrementTo(int value)
        {
            ProgressValue = value;
        }

        /// <summary>
        /// Provides a way of signaling the process should be cancelled
        /// </summary>
        public WaitHandle CancelEvent
        {
            get
            {
                return cancelEvent ?? (cancelEvent = new ManualResetEvent(false));
            }
        }
        ManualResetEvent cancelEvent;

        /// <summary>
        /// Called at the end of the process. Finalizes any progress updates
        /// </summary>
        virtual public void Finished()
        {
            ProgressValue = ProgressMin = ProgressMax = 0;

            // Stop timing
            Timer.Stop();

            // Hide the view
            var closeView = new Action(delegate()
                {
                    var window = View as Window;
                    if (window != null && window.IsVisible)
                        window.DialogResult = true;

                    View = null;
                });
            Application.Current.Dispatcher.BeginInvoke(closeView, DispatcherPriority.Background);
        }
        #endregion

        #region IViewModel Members
        /// <summary>
        /// Holds the user control or window associated with this view model
        /// </summary>
        public IView View
        {
            get;
            set;
        }
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
        /// <remarks>
        /// This call is forwarded to the UI thread
        /// </remarks>
        protected void OnPropertyChanged(string info)
        {
            Application.Current.Dispatcher.Invoke(new AsyncOnPropertyChanged(UISafeOnPropertyChanged), info);
        }

        delegate void AsyncOnPropertyChanged(string info);
        /// <summary>
        /// Raises the PropertyChanged event on the UI thread
        /// </summary>
        /// <param name="info">The name of the property that was changed</param>
        void UISafeOnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                // Make sure this happens on the UI thread
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
