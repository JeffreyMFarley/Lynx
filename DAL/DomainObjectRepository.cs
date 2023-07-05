using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Esoteric;
using Esoteric.DAL.Interface;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.DAL
{
    sealed public class DomainObjectRepository : IDomainManager,
                                                 IRandomAccessRepository<EntitySet, string>, 
                                                 IRandomAccessRepository<LinkSet, string>,
                                                 IDisposable
    {
        #region Factory Methods
        [Export]
        static public IDomainManager BuildManager(Domain domain)
        {
            if (domain == null)
                return null;

            var manager = new DomainObjectRepository();
            manager.Domain = domain;

            return manager;
        }
        #endregion

        #region IDomainManager Members
        Domain Domain { get; set; }

        public bool IsDirty
        {
            get
            {
                return Domain.HasChanges();
            }
        }

        public event CollectionChangeEventHandler TablesCollectionChanged
        {
            add
            {
                Domain.Tables.CollectionChanged += value;
            }
            remove
            {
                Domain.Tables.CollectionChanged -= value;
            }
        }

        public IRandomAccessRepository<EntitySet, string> EntitySets
        {
            get { return this; }
        }

        public IRandomAccessRepository<LinkSet, string> LinkSets
        {
            get { return this; }
        }

        public bool Save(FileInfo file)
        {
            var fileRepository = new DomainFileRepository();
            fileRepository.Set(Domain, file);
            Domain.AcceptChanges();
            return true;
        }
        #endregion

        #region IRepository Members

        int? IRepository.Count
        {
            get { return Domain.Tables.Count; }
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Domain.Tables.GetEnumerator();
        }

        #endregion

        #region IRandomAccessRepository<EntitySet,string> Members

        string IRandomAccessRepository<EntitySet, string>.GetKey(EntitySet item)
        {
            return item.TableName;
        }

        EntitySet IRandomAccessRepository<EntitySet, string>.Set(EntitySet item, string key)
        {
            throw new NotSupportedException();
        }

        bool IRandomAccessRepository<EntitySet, string>.Delete(string key)
        {
            Domain.Tables.Remove(key);
            return true;
        }

        #endregion

        #region IReadOnlyRepository<EntitySet,string> Members

        EntitySet IReadOnlyRepository<EntitySet, string>.Get(string key)
        {
            var entitySet = Domain.Tables[key] as EntitySet;
            if( entitySet != null ) entitySet.AcceptChanges();
            return entitySet;
        }

        #endregion

        #region IEnumerable<EntitySet> Members

        IEnumerator<EntitySet> IEnumerable<EntitySet>.GetEnumerator()
        {
            for (int i = 0; i < Domain.Tables.Count; i++)
            {
                var table = Domain.Tables[i];
                string tableType = table.ExtendedProperties[Domain.TableTypeExtendedProperty] as string;

                if (tableType == Domain.TableTypeEntitySet || tableType == null )
                    yield return table as EntitySet;
            }
        }

        #endregion

        #region ISerialRepository<EntitySet> Members

        EntitySet ISerialRepository<EntitySet>.Add(EntitySet item)
        {
            Domain.Tables.Add(item);
            return item;
        }

        #endregion

        #region IGetByCriteria<EntitySet> Members

        public List<EntitySet> GetByCriteria(QueryBase<EntitySet> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IRandomAccessRepository<LinkSet,string> Members

        string IRandomAccessRepository<LinkSet, string>.GetKey(LinkSet item)
        {
            return item.TableName;
        }

        LinkSet IRandomAccessRepository<LinkSet, string>.Set(LinkSet item, string key)
        {
            throw new NotSupportedException();
        }

        bool IRandomAccessRepository<LinkSet, string>.Delete(string key)
        {
            Domain.Tables.Remove(key);
            return true;
        }

        #endregion

        #region IReadOnlyRepository<LinkSet,string> Members

        LinkSet IReadOnlyRepository<LinkSet, string>.Get(string key)
        {
            var linkSet = Domain.Tables[key] as LinkSet;
            if (linkSet != null) linkSet.AcceptChanges();
            return linkSet;
        }

        #endregion

        #region IEnumerable<LinkSet> Members

        IEnumerator<LinkSet> IEnumerable<LinkSet>.GetEnumerator()
        {
            for (int i = 0; i < Domain.Tables.Count; i++)
            {
                var table = Domain.Tables[i];
                string tableType = table.ExtendedProperties[Domain.TableTypeExtendedProperty] as string;

                if (tableType == Domain.TableTypeLinkSet)
                    yield return table as LinkSet;
            }
        }

        #endregion

        #region ISerialRepository<LinkSet> Members

        LinkSet ISerialRepository<LinkSet>.Add(LinkSet item)
        {
            Domain.Tables.Add(item);
            return item;
        }

        #endregion

        #region IGetByCriteria<LinkSet> Members

        public List<LinkSet> GetByCriteria(QueryBase<LinkSet> query)
        {
            throw new NotImplementedException();
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
            if (disposing)
            {
                Domain.Dispose();
            }
        }

        ~DomainObjectRepository()
        {
            Dispose(false);
        }
        #endregion
    }
}
