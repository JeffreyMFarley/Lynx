using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Lynx.UI.PasteWizard.ETL
{
    public class TagsToFlags : TransformBase
    {
        readonly char[] separators = new char[] {','};

        public TagsToFlags(string sourceColumn)
        {
            SourceColumn = sourceColumn;
        }

        public string SourceColumn { get; private set; }

        SortedSet<string> Flags
        {
            get
            {
                if (flags == null)
                {
                    flags = new SortedSet<string>();
                }
                return flags;
            }
        }
        SortedSet<string> flags;
	
        public override string DisplayName
        {
            get
            {
                return string.Format("Tags to Flags: {0}", SourceColumn);
            }
        }

        string CleanString(string s)
        {
            return new string(s.Trim().Replace(' ', '_').ToCharArray()
                                      .Where(x => char.IsLetterOrDigit(x) || x == '_')
                                      .ToArray());
        }

        public override void Beginning(DataView extractTable, DataTable loadTable)
        {
            Flags.Clear();

            foreach (DataRowView source in extractTable)
            {
                var o = source[SourceColumn];
                var csv = (o != null) ? o.ToString() : string.Empty;
                if (string.IsNullOrEmpty(csv))
                    continue;

                foreach (var s in csv.Split(separators, StringSplitOptions.RemoveEmptyEntries))
                {
                    var cleansed = CleanString(s);
                    if( !string.IsNullOrEmpty(cleansed) )
                        Flags.Add(cleansed);
                }
            }

            foreach (var flag in Flags)
            {
                if (!loadTable.Columns.Contains(flag))
                    loadTable.Columns.Add(new DataColumn(flag, typeof(bool)) { DefaultValue = false });
            }
        }


        public override void Transform(DataRow source, DataRow target)
        {
            var o = source[SourceColumn];
            var csv = (o != null) ? o.ToString() : string.Empty;
            if (string.IsNullOrEmpty(csv))
                return;

            foreach (var s in csv.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                var cleansed = CleanString(s);
                if (!string.IsNullOrEmpty(cleansed))
                    target[cleansed] = true;
            }
        }
    }
}
