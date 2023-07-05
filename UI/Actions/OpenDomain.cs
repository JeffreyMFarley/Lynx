using System;
using System.ComponentModel.Composition;
using System.IO;
using Esoteric.DAL.Interfaces;
using Esoteric.UI;
using Lynx.Interfaces;
using Lynx.Models;
using Lynx.UI.ViewModels;
using Microsoft.Win32;

namespace Lynx.UI.Actions
{
    [Export]
    [Export(typeof(IShellOpen))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class OpenDomain : AvalentAction, IAdjunctAction<FileInfo>, IShellOpen
    {
        #region IoC Properties
        [Import]
        protected IRandomAccessRepository<Domain, FileInfo> DomainRepository { get; set; }

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
                    pathName =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Lynx");

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
        protected override void OnDialogCommand()
        {
            OpenFileDialog filedlg = new OpenFileDialog();
            filedlg.Title = "Open Lynx File";
            filedlg.InitialDirectory = PathName;
            filedlg.Filter = "Lynx Files {*.lynx)|*.lynx";
            filedlg.RestoreDirectory = true;
            if (filedlg.ShowDialog() ?? false)
            {
                PathName = Path.GetDirectoryName(filedlg.FileName);
                Options = new FileInfo(filedlg.FileName);

                OnNoDialogCommand();
            }
        }

        protected override void OnNoDialogCommand()
        {
            Domain domain = DomainRepository.Get(Options);
            if (domain != null)
                if (ActiveDomain.Activate(domain))
                    ActiveDomain.PathName = Options.FullName;
        }
        #endregion

        #region IAdjunctAction<FileInfo> Members
        public FileInfo Options
        {
            get;
            set;
        }
        #endregion

        #region IShellOpen Members

        public bool Open(FileInfo file)
        {
            Options = file;
            OnNoDialogCommand();
            return true;
        }

        #endregion
    }
}
