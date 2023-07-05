using Lynx.Models;

namespace Lynx.Interfaces
{
    public interface IActiveDomain
    {
        IDomainManager Manager { get; }

        bool Activate(Domain newDomain);

        string PathName { get; set; }
    }
}
