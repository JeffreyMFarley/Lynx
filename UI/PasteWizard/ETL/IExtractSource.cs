using System;
using System.Data;
using Esoteric.DAL.Interfaces;

namespace Lynx.UI.PasteWizard.ETL
{
    public interface IExtractSource : IRepository<DataRow>, IViewProvider, IDisposable
    {
        void Load();
    }
}
