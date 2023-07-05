using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Lynx.UI.PasteWizard.ETL
{
    public class Copy : NewEmpty
    {
        public Copy(string sourceColumn)
        {
            SourceColumn = sourceColumn;
        }

        public string SourceColumn { get; private set; }

        public override string DisplayName
        {
            get
            {
                return string.Format("Copy {0} To", SourceColumn);
            }
        }

        public override void Transform(DataRow source, DataRow target)
        {
            if (string.IsNullOrWhiteSpace(SourceColumn))
                return;

            if (string.IsNullOrWhiteSpace(TargetColumn))
                return;

            var o = source[SourceColumn];
            var s = (o != null) ? o.ToString() : string.Empty;
            if( string.IsNullOrEmpty(s) )
                return;

            if (TargetType == DataType.AsString)
            {
                target[TargetColumn] = s;
            }
            else if (TargetType == DataType.AsBoolean)
            {
                bool b;
                if (bool.TryParse(s, out b))
                    target[TargetColumn] = b;
            }
            else if (TargetType == DataType.AsDateTime)
            {
                DateTime t;
                if (DateTime.TryParse(s, out t))
                    target[TargetColumn] = t;
            }
            else if (TargetType == DataType.AsID)
            {
                Guid g;
                if (Guid.TryParse(s, out g))
                    target[TargetColumn] = g;
            }
            else if (TargetType == DataType.AsNumeric)
            {
                float f;
                if (float.TryParse(s, out f))
                    target[TargetColumn] = f;
            }
        }
    }
}
