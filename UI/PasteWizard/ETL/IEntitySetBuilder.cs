using Lynx.Interfaces;
using Lynx.Models;
using Esoteric.BLL.Interfaces;

namespace Lynx.UI.PasteWizard.ETL
{
    public interface IEntitySetBuilder : IEtlProcess
    {
        EntitySet Target { get; }

        bool Build(IProgressUI progress);
    }
}
