using System;
using System.Collections.Generic;
using Lynx.Models;

namespace Lynx.Interfaces
{
    /// <summary>
    /// Defines the properties and methods required for implementing a Paste ETL
    /// </summary>
    /// <typeparam name="TClipboardElement">The type of element in the clipbard</typeparam>
    /// <typeparam name="TLoadSet">The type of dataset for the clipboard elements</typeparam>
    public interface IPasteProcess<TClipboardElement, TLoadSet> : IPasteProcess
        where TLoadSet : BaseSet
    {
        /// <summary>
        /// The implementer that will iterate through the clipbard elements
        /// </summary>
        IEnumerable<TClipboardElement> ElementExtractor { get; set; }

        /// <summary>
        /// The dataset that will receive the elements
        /// </summary>
        TLoadSet DestinationSet { get; set; }
    }
}
