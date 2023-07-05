using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.IO;

namespace Lynx.Console
{
    /// <summary>
    /// The starting module of the application
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Called by .NET when the applcation is starting
        /// </summary>
        /// <remarks>
        /// In this implementation, the application:
        /// <list type="number">
        /// <item>Registers <see cref="HandleException"/> as a callback for <see cref="Application.DispatcherUnhandledException">unhandled exception</see> events.</item>
        /// <item>Creates the <see cref="Bootstrapper"/> which is used to coordinate application-level objects</item>
        /// </list>
        /// </remarks>
        /// <param name="e">Contains the command line arguments passed to the application from the OS</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(HandleException);
            base.OnStartup(e);

            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();

            if (e.Args.Length >= 1) //make sure an argument is passed
            {
                FileInfo file = new FileInfo(e.Args[0]);
                if (file.Exists) //make sure it's actually a file
                {
                    bootstrapper.Open(file);
                }
            }
        }

        /// <summary>
        /// Called when an error occurs that was not handled by the application.
        /// </summary>
        /// <remarks>
        /// The default implementation aggregates the exception infomration into one string and displays it to the user
        /// <para>It is important that the <see cref="DispatcherUnhandledExceptionEventArgs.Handled">Handled</see> property be set to TRUE. Otherwise, the default implementation may shut down the applcation.</para>
        /// <para>In the future, it is expected that the Logging, Exception and Policy application blocks will be used here</para>
        /// </remarks>
        /// <param name="sender">The sender of this event (should be <see cref="Application"/>)</param>
        /// <param name="e">Details about the exception</param>
        void HandleException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception local = e.Exception;
            StringBuilder message = new StringBuilder();
            string indent = "";

            while (local != null)
            {
                message.AppendFormat("{0}\n", local.Message);
                message.Append(indent);
                indent += "  ";
                local = local.InnerException;
            }

            // Show the message to the user
            string title = (App.Current != null && App.Current.MainWindow != null) ? App.Current.MainWindow.Title : "Hello Prizm";
            MessageBox.Show(message.ToString(), title);

            // We handled the message
            e.Handled = true;

#if DEBUG
            // Don't handle
#else
            // Check policy
#endif
        }
    }
}
