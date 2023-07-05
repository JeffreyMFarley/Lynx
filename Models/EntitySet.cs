using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Runtime.Serialization;
using Esoteric;
using Esoteric.DAL.Interface;

namespace Lynx.Models
{
    [Serializable]
    sealed public class EntitySet : BaseSet, IRandomAccessRepository<Entity, Guid>
    {
        #region Constructors
        public EntitySet()
            : base()
        {
            AddRequiredColumns();
        }

        public EntitySet(string tableName)
            : base(tableName)
        {
            AddRequiredColumns();
        }

        public EntitySet(DataTable table)
            : base(table)
        { 
        }

        protected EntitySet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        void AddRequiredColumns()
        {
            Columns.Add(new DataColumn(Domain.IDColumn, typeof(Guid)));
            Columns.Add(new DataColumn(Domain.NameColumn, typeof(string)));
            Columns.Add(new DataColumn(Domain.DescriptionColumn, typeof(string)));

            this.PrimaryKey = new DataColumn[] { Columns[Domain.IDColumn] };

            ExtendedProperties.Add(Domain.TableTypeExtendedProperty, Domain.TableTypeEntitySet);
        }
        #endregion

        #region Overrides
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new Entity(builder);
        }
        #endregion

        #region Typed Methods
        public Entity Add(string name, string description)
        {
            var newRow = NewRow() as Entity;

            newRow.Name = name;
            newRow.Description = description;
            Rows.Add(newRow);

            return newRow;
        }
        #endregion

        #region IRandomAccessRepository<Entity,Guid> Members

        public Guid GetKey(Entity item)
        {
            return item.Field<Guid>(Domain.IDColumn);
        }

        public Entity Set(Entity item, Guid key)
        {
            throw new NotSupportedException();
        }

        public bool Delete(Guid key)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IReadOnlyRepository<Entity,Guid> Members
        public Entity Get(Guid id)
        {
            return this.Where(p => p.ID == id).FirstOrDefault();
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
            return (this as IEnumerable<Entity>).GetEnumerator();
        }

        #endregion

        #region IEnumerable<Entity> Members

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator()
        {
            for (int i = 0; i < Rows.Count; i++)
                yield return Rows[i] as Entity;
        }

        #endregion

        #region ISerialRepository<Entity> Members

        public Entity Add(Entity item)
        {
            Debug.Assert(item.Table.TableName == this.TableName);
            Rows.Add(item);
            return item;
        }

        #endregion

        #region IGetByCriteria<Entity> Members

        public List<Entity> GetByCriteria(QueryBase<Entity> query)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
