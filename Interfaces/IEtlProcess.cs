using Esoteric.BLL.Interfaces;

namespace Lynx.Interfaces
{
    public interface IEtlProcess
    {
        bool Initialize();
        void Execute(IProgressUI progress);
        void Finish();
    }
}
