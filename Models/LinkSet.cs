using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Runtime.Serialization;
using Esoteric.DAL.Interface;

namespace Lynx.Models
{
    [Serializable]
    sealed public class LinkSet : BaseSet, ISerialRepository<Link>
    {
        #region Constructors
        LinkSet() : base() { }

        public LinkSet(EntitySet source, EntitySet target, string tableName)
        {
            SourceSet = source;
            TargetSet = target;

            TableName = tableName;

            ExtendedProperties.Add(Domain.SourceExtendedProperty, source.TableName);
            ExtendedProperties.Add(Domain.TargetExtendedProperty, target.TableName);
            ExtendedProperties.Add(Domain.TableTypeExtendedProperty, Domain.TableTypeLinkSet);

            Columns.Add(new DataColumn(Domain.SourceIDColumn, typeof(Guid)));
            Columns.Add(new DataColumn(Domain.TargetIDColumn, typeof(Guid)));
            Columns.Add(new DataColumn(Domain.NameColumn, typeof(string)));
            Columns.Add(new DataColumn(Domain.DescriptionColumn, typeof(string)));
        }

        public LinkSet(DataTable table)
            : base(table)
        {
        }

        protected LinkSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Overrides
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new Link(builder);
        }

        public new LinkSet Clone()
        {
            var clone = new LinkSet();
            CloneTo(this, clone);
            return clone;
        }
        #endregion

        #region Linq Methods
        public Func<LinkAsEdge, double> WeightingFunction
        {
            get
            {
                return Weight;
            }
        }

        // TODO: think more about how to weight an edge in relation to the whole link set
        double Weight(LinkAsEdge edge)
        {
            Link link = edge;

            // Count the usage of either end within the link set
            return this.Count(l => l.SourceID == link.SourceID 
                           || l.SourceID == link.TargetID
                           || l.TargetID == link.SourceID
                           || l.TargetID == link.TargetID);
        }

        #endregion

        #region Typed Methods
        public void Add(Entity source, Entity target)
        {
            var newRow = NewRow() as Link;

            newRow.SourceID = source.ID;
            newRow.TargetID = target.ID;
            Rows.Add(newRow);
        }

        public EntitySet SourceSet
        {
            get
            {
                if (sourceSet == null)
                {
                    string sourceName = ExtendedProperties[Domain.SourceExtendedProperty] as string;
                    sourceSet = DataSet.Tables[sourceName] as EntitySet;
                }
                return sourceSet;
            }
            set { sourceSet = value; }
        }
        EntitySet sourceSet;

        public EntitySet TargetSet
        {
            get
            {
                if (targetSet == null)
                {
                    string targetName = ExtendedProperties[Domain.TargetExtendedProperty] as string;
                    targetSet = DataSet.Tables[targetName] as EntitySet;
                }
                return targetSet;
            }
            set { targetSet = value; }
        }
        EntitySet targetSet;

        public Entity GetSource(Guid id)
        {
            return SourceSet.Get(id);
        }

        public Entity GetTarget(Guid id)
        {
            return TargetSet.Get(id);
        }
        #endregion

        #region ISerialRepository<Link> Members

        public Link Add(Link item)
        {
            Debug.Assert(item.Table.TableName == this.TableName);
            Rows.Add(item);
            return item;
        }

        #endregion

        #region IRepository Members

        public int? Count
        {
            get { return this.Rows.Count; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Link>).GetEnumerator();
        }

        #endregion

        #region IEnumerable<Link> Members

        IEnumerator<Link> IEnumerable<Link>.GetEnumerator()
        {
            for (int i = 0; i < Rows.Count; i++)
                yield return Rows[i] as Link;
        }

        #endregion
    }
}
