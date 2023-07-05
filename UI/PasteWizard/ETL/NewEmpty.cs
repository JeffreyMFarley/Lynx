using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Lynx.UI.PasteWizard.ETL
{
    public class NewEmpty : TransformBase
    {
        public string TargetColumn { get; set; }

        public DataType TargetType
        {
            get { return targetType; }
            set 
            { 
                targetType = value;
                InnerType = targetType.ToType();
            }
        }
        DataType targetType;

        protected Type InnerType
        {
            get
            {
                return innerType ?? (innerType = typeof(string));
            }
            set
            {
                innerType = value;
            }
        }
        Type innerType;

        public override string DisplayName
        {
            get { return "New"; }
        }

        public override void Beginning(DataView extractTable, DataTable loadTable)
        {
            if (string.IsNullOrWhiteSpace(TargetColumn))
                return;

            if( !loadTable.Columns.Contains(TargetColumn) )
                loadTable.Columns.Add(new DataColumn(TargetColumn, InnerType));
        }

        public override void Transform(DataRow source, DataRow target)
        {
        }
    }
}
