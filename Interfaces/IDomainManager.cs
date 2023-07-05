using System.ComponentModel;
using System.IO;
using Esoteric.DAL.Interface;
using Lynx.Models;

namespace Lynx.Interfaces
{
    public interface IDomainManager
    {
        event CollectionChangeEventHandler TablesCollectionChanged;

        IRandomAccessRepository<EntitySet, string> EntitySets { get; }
        IRandomAccessRepository<LinkSet, string> LinkSets { get; }

        bool IsDirty { get; }

        bool Save(FileInfo file);
    }
}
