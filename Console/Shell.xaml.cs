using System.Windows;

namespace Lynx.Console
{
    /// <summary>
    /// The shell is the top-level main window of the application.
    /// </summary>
    /// <remarks>
    /// Initially, the shell should contain no application-specific UI elements.  
    /// These are added later injecting user controls into defined regions
    /// </remarks>
    public partial class Shell : Window
    {
        /// <summary>
        /// Initializes the controls
        /// </summary>
        public Shell()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called by the bootstrapper to see if the security model is valid to run this application
        /// </summary>
        /// <returns>TRUE if the application should continue, FALSE otherwise</returns>
        /// <remarks>
        /// It seems that the Shell must be created before running any sort of UI dialog. Otherwise the application will not
        /// run properly
        /// </remarks>
        public bool OkToRun()
        {
            return true;
        }
    }
}
