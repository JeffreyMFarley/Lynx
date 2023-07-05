using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Esoteric;
using Esoteric.DAL.Interface;

namespace Lynx.XsdExchange
{
    public class XsdRepository : IRandomAccessRepository<XElement, XName>, IDisposable
    {
        internal const string xsAsXNamePrefix = "{http://www.w3.org/2001/XMLSchema}";
        Dictionary<XName, XElement> Store
        {
            get
            {
                if (store == null)
                {
                    store = new Dictionary<XName, XElement>();
                }
                return store;
            }
        }
        Dictionary<XName, XElement> store;

        #region IRandomAccessRepository<XElement,XName> Members
        public XName GetKey(XElement item)
        {
            return item.Name;
        }

        public XElement Set(XElement item, XName key)
        {
            // Bouncer code
            if (item == null)
                return null;

            if (item.Name.LocalName == "element")
            {
                XAttribute id = new XAttribute("lynxid", Guid.NewGuid());
                item.Add(id);
                XName collisionLess = "e" + id.Value;
                Store[collisionLess] = item;
            }

            else if (!Store.ContainsKey(key))
            {
                XAttribute id = new XAttribute("lynxid", Guid.NewGuid());
                item.Add(id);
                Store.Add(key, item);
            }
            else
                throw new InvalidOperationException();

            return item;
        }

        public bool Delete(XName key)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IReadOnlyRepository<XElement,XName> Members

        public XElement Get(XName key)
        {
            XElement result = null;
            Store.TryGetValue(key, out result);
            return result;
        }

        public XElement Get(XElement node)
        {
            XAttribute attr = node.Attribute("name");
            if (attr == null)
                return null;

            var result = Get(attr.Value.Replace("xs:", xsAsXNamePrefix));
            if (result != null)
                return result;

            attr = node.Attribute("lynxid");
            if (attr == null)
                return null;

            return Get("e" + attr.Value);
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

        #region IEnumerable<XElement> Members

        public IEnumerator<XElement> GetEnumerator()
        {
            return Store.Values.GetEnumerator();
        }

        #endregion


        #region ISerialRepository<XElement> Members

        public XElement Add(XElement item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGetByCriteria<XElement> Members

        public List<XElement> GetByCriteria(QueryBase<XElement> query)
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

        ~XsdRepository()
        {
            Dispose(true);
        }
        #endregion
    }
}
