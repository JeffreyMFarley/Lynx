using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Lynx.UI.PasteWizard.ETL
{
    public class GenerateGuid : TransformBase
    {
        public string TargetColumn { get; set; }

        public override string DisplayName
        {
            get { return "Generate ID"; }
        }

        public override void Beginning(DataView extractTable, DataTable loadTable)
        {
            if (string.IsNullOrWhiteSpace(TargetColumn))
                return;

            if (!loadTable.Columns.Contains(TargetColumn))
                loadTable.Columns.Add(new DataColumn(TargetColumn, typeof(Guid)));
        }

        public override void Transform(DataRow source, DataRow target)
        {
            if (string.IsNullOrWhiteSpace(TargetColumn))
                return;

            target[TargetColumn] = Guid.NewGuid();
        }
    }
}
