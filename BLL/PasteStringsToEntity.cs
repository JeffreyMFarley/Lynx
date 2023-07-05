using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using Esoteric.BLL.Interfaces;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.BLL
{
    [Export(typeof(IPasteProcess))]
    [Export(typeof(IPasteProcess<string[], EntitySet>))]
    public class PasteStringsToEntity : IPasteProcess<string[], EntitySet>
    {
        #region IPasteProcess Members

        public bool ContainsData()
        {
            return Clipboard.ContainsText();
        }

        public Type ClipboardElementType
        {
            get { return typeof(string[]); }
        }

        public Type DestinationSetType
        {
            get { return typeof(EntitySet); }
        }

        #endregion

        #region IPasteProcess<string[],EntitySet> Members

        public IEnumerable<string[]> ElementExtractor
        {
            get;
            set;
        }

        public EntitySet DestinationSet
        {
            get;
            set;
        }

        #endregion

        #region IEtlProcess Members

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void Execute(IProgressUI progress)
        {
            throw new NotImplementedException();
        }

        public void Finish()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
