using System.ComponentModel.Composition;
using System.IO;
using Esoteric.UI;
using Lynx.Interfaces;
using Microsoft.Win32;

namespace Lynx.UI.Actions
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SaveDomain : AvalentAction, IAdjunctAction<FileInfo>
    {
        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region Public Properties
        /// <summary>
        /// Path for select a File.
        /// </summary>
        public string PathName
        {
            get
            {
                if (string.IsNullOrEmpty(pathName))
                    pathName = "C:\\";

                return pathName;
            }
            set
            {
                pathName = value;
            }
        }
        string pathName;
        #endregion

        #region AvalentAction members
        protected override bool CanDialogCommand()
        {
            return (ActiveDomain.Manager != null);
        }

        protected override void OnDialogCommand()
        {
            SaveFileDialog filedlg = new SaveFileDialog();
            filedlg.Title = "Save Lynx File";
            filedlg.InitialDirectory = PathName;
            filedlg.FileName = ActiveDomain.PathName;
            filedlg.Filter = "Lynx Files {*.lynx)|*.lynx";
            filedlg.RestoreDirectory = true;
            filedlg.OverwritePrompt = true;
            if (filedlg.ShowDialog() ?? false)
            {
                PathName = Path.GetDirectoryName(filedlg.FileName);
                Options = new FileInfo(filedlg.FileName);

                OnNoDialogCommand();
            }
        }

        protected override bool CanNoDialogCommand()
        {
            return (ActiveDomain.Manager != null);
        }

        protected override void OnNoDialogCommand()
        {
            // Perform what ever action is required without any user interaction
            ActiveDomain.Manager.Save(Options);
        }
        #endregion

        #region IAdjunctAction<FileInfo> Members
        public FileInfo Options
        {
            get;
            set;
        }
        #endregion
    }
}
