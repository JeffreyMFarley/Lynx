using System.Collections.Generic;
using System.IO;
using Lynx.Models;

namespace Lynx.Interfaces
{
    public interface IFileIterator<T>
    {
        FileInfo Source { get; set; }

        IEnumerable<T> EnumerateEntities();

        IEnumerable<GenericLink<T>> EnumerateLinks();
    }
}
