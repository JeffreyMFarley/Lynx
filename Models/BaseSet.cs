using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using Esoteric.DAL.Interfaces;

namespace Lynx.Models
{
    [Serializable]
    abstract public class BaseSet : DataTable, IViewProvider
    {
        #region Constructors
        protected BaseSet() : base() { }

        protected BaseSet(string tableName) : base(tableName) { }

        protected BaseSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public BaseSet(DataTable table)
            : base(table.TableName)
        {
            CopyTo(table, this);
        }
        #endregion

        #region Helper Methods
        public void AddColumn(string name, Type type)
        {
            DataColumn dc = new DataColumn(name, type);
            Columns.Add(dc);
        }

        static public bool IsSystemColumn(DataColumn dc)
        {
           return (
                   dc.ColumnName == Domain.SourceNameColumn
                || dc.ColumnName == Domain.TargetNameColumn
                || !string.IsNullOrEmpty(dc.Expression)
                  );
        }

        static protected List<string> CloneTo(DataTable source, BaseSet target)
        {
            // Copy over the extended properties
            foreach (DictionaryEntry pair in source.ExtendedProperties)
                target.ExtendedProperties.Add(pair.Key, pair.Value);

            var validColumns = new List<string>();

            // Copy over the columns
            for (int col = 0; col < source.Columns.Count; col++)
            {
                DataColumn srcCol = source.Columns[col];
                if (!IsSystemColumn(srcCol))
                {
                    DataColumn dc = srcCol.Clone();
                    target.Columns.Add(dc);
                    validColumns.Add(srcCol.ColumnName);
                }
            }

            return validColumns;
        }

        static protected void CopyTo(DataTable source, BaseSet target)
        {
            var validColumns = CloneTo(source, target);

            // Copy over the rows
            foreach (DataRow inputRow in source.Rows)
            {
                var row = target.NewRow();

                // Copy over the cells
                foreach (var colName in validColumns)
                    row[colName] = inputRow[colName];

                target.Rows.Add(row);
            }
        }
        #endregion

        #region IViewProvider Members

        public DataView View
        {
            get { return DefaultView; }
        }

        #endregion

    }
}