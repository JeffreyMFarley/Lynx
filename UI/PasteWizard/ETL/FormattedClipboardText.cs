using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Esoteric.DAL;

namespace Lynx.UI.PasteWizard.ETL
{
    public class FormattedClipboardText : IExtractSource
    {
        #region Public Properties

        public DataTableTextFormatter Formatter
        {
            get
            {
                if (formatter == null)
                {
                    formatter = new DataTableTextFormatter();
                    formatter.Options.DataHasHeaders = true;
                    formatter.Options.DelimiterTab = true;
                }
                return formatter;
            }
        }
        DataTableTextFormatter formatter;

        internal DataTable Table
        {
            get
            {
                if (table == null)
                {
                    table = new DataTable();
                }
                return table;
            }
        }
        DataTable table;

        #endregion

        #region IViewProvider Members

        public DataView View
        {
            get { return Table.DefaultView; }
        }

        #endregion

        #region IRepository Members
        
        public IEnumerator<DataRow> GetEnumerator()
        {
            foreach (var r in Table.Rows.Cast<DataRow>())
                yield return r;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int? Count
        {
            get { return Table.Rows.Count; }
        }

        #endregion

        #region IExtractSource Members

        public void Load()
        {
            Formatter.Deserialize(Clipboard.GetText(), Table);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing && table != null)
            {
                table.Dispose();
            }
        }

        #endregion

    }
}
