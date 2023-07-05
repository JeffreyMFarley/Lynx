using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;

namespace Lynx.Models
{
    [Serializable]
    public class Domain : DataSet
    {
        #region Constants
        public const string IDColumn = "ID";
        public const string NameColumn = "Name";
        public const string DescriptionColumn = "Description";
        public const string SourceIDColumn = "Source";
        public const string SourceNameColumn = "SourceName";
        public const string TargetIDColumn = "Destination";
        public const string TargetNameColumn = "DestinationName";

        static public readonly IList<string> SystemColumnNames = new string[] { IDColumn, NameColumn, DescriptionColumn, SourceIDColumn, SourceNameColumn, TargetIDColumn, TargetNameColumn };

        public const string SourceExtendedProperty = "SourceSet";
        public const string TargetExtendedProperty = "TargetSet";

        public const string TableTypeExtendedProperty = "TableType";
        public const string TableTypeEntitySet = "EntitySet";
        public const string TableTypeLinkSet = "LinkSet";
        #endregion

        #region Constructor
        public Domain() : base("Lynx")
        {
            Tables.CollectionChanged += new CollectionChangeEventHandler(Tables_CollectionChanged);
        }

        protected Domain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Event Handlers
        void Tables_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            LinkSet set = e.Element as LinkSet;
            if (set == null)
                return;

            if (e.Action == CollectionChangeAction.Add)
            {
                // Handle the source
                DataColumn dcParent = set.SourceSet.Columns[Domain.IDColumn];
                DataColumn dcChild = set.Columns[Domain.SourceIDColumn];
                string relationName = set.TableName + "_Source";
                Relations.Add(relationName, dcParent, dcChild);
                set.Columns.Add(new DataColumn(Domain.SourceNameColumn, typeof(string), string.Format("Parent({0}).Name", relationName)));

                // Handle the target
                dcParent = set.TargetSet.Columns[Domain.IDColumn];
                dcChild = set.Columns[Domain.TargetIDColumn];
                relationName = set.TableName + "_Target";
                Relations.Add(relationName, dcParent, dcChild);
                set.Columns.Add(new DataColumn(Domain.TargetNameColumn, typeof(string), string.Format("Parent({0}).Name", relationName)));
            }
            else if (e.Action == CollectionChangeAction.Remove)
            {
            }
        }
        #endregion
    }
}
