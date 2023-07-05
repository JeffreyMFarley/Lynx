using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lynx.Models;

namespace Lynx.Interfaces
{
    public interface IPasteProcess : IEtlProcess
    {
        /// <summary>
        /// Determines if the clipboard contains data in a format understood by the repository
        /// </summary>
        /// <returns>TRUE if the clipboard contains known data formats, FALSE otehrwise</returns>
        bool ContainsData();

        Type ClipboardElementType { get; }

        Type DestinationSetType { get; }
    }
}
