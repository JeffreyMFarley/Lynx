using System.ComponentModel.Composition;
using System.IO;
using Esoteric.BLL.Interfaces;
using Esoteric.UI;
using Lynx.Interfaces;
using Microsoft.Win32;

namespace Lynx.XsdExchange.UI
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ImportXsd : AvalentAction, IAdjunctAction<FileInfo>
    {
        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }

        [Import]
        protected IProgressUI ProgressUI { get; set; }
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
            return ActiveDomain != null && ActiveDomain.Manager != null;
        }

        protected override void OnDialogCommand()
        {
            OpenFileDialog filedlg = new OpenFileDialog();
            filedlg.Title = "Import XSD";
            filedlg.InitialDirectory = PathName;
            filedlg.Filter = "XSD Files {*.xsd)|*.xsd";
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
            // Initialize the instance variables
            using (var process = new ImportXsdToDomain { SourceFile = Options })
            {
                // Run the process
                process.RunProcessAsynch(ProgressUI);
            }
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
