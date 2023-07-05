using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Lynx.UI.PasteWizard.ETL
{
    public interface ITransform : ICloneable
    {
        string DisplayName { get; }

        void Beginning(DataView extractTable, DataTable loadTable);
        void Transform(DataRow source, DataRow target);
        void Ending(DataView extractTable, DataTable loadTable);
    }
}
