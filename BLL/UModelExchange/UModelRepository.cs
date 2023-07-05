using System;
using System.Collections;
using System.Collections.Generic;
using Esoteric;
using Esoteric.DAL.Interface;
using UModelLib;

namespace Lynx.UModelExchange
{
    public class UModelRepository : IRandomAccessRepository<IUMLData, string>, IDisposable
    {
        Dictionary<string,IUMLData> Store
        {
            get
            {
                return store ?? (store = new Dictionary<string,IUMLData>());
            }
        }
        Dictionary<string,IUMLData> store;

        #region IRandomAccessRepository<IUMLData,string> Members
        public string GetKey(IUMLData item)
        {
            return (item != null ) ? item.UUID : string.Empty;
        }

        public IUMLData Set(IUMLData item, string key)
        {
            // Bouncer code
            if (item == null)
                return null;

            if (!Store.ContainsKey(key))
                Store.Add(key, item);
            else
            {
                Store[key] = null;
                Store[key] = item;
            }

            return item;
        }

        public bool Delete(string key)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IReadOnlyRepository<IUMLData,string> Members

        public IUMLData Get(string key)
        {
            IUMLData result = null;
            Store.TryGetValue(key, out result);
            return result;
        }

        public bool Has(string key)
        {
            return Store.ContainsKey(key);
        }

        public bool Has(IUMLData item)
        {
            return Store.ContainsKey(GetKey(item));
        }
        #endregion

        #region IRepository Members

        public int? Count
        {
            get { return Store.Count; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Store.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable<IUMLData> Members

        public IEnumerator<IUMLData> GetEnumerator()
        {
            return Store.Values.GetEnumerator();
        }

        #endregion

        #region ISerialRepository<IUMLData> Members

        public IUMLData Add(IUMLData item)
        {
            // Bouncer code
            if (item == null)
                return null;

            var key = GetKey(item);

            if (!Store.ContainsKey(key))
                Store.Add(key, item);

            return item;
        }
        #endregion

        #region IGetByCriteria<IUMLData> Members

        public List<IUMLData> GetByCriteria(QueryBase<IUMLData> query)
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
                Store.Clear();
            }
        }

        ~UModelRepository()
        {
            Dispose(true);
        }
        #endregion
    }
}
