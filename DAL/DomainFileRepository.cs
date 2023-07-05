using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Esoteric.DAL.Interface;
using Lynx.Models;

namespace Lynx.DAL
{
    [Export(typeof(IRandomAccessRepository<Domain, FileInfo>))]
    public class DomainFileRepository : RandomAccessRepository<Domain, FileInfo>
    {
        #region IoC Properties
        [Import]
        protected FileRepository FileIterator { get; set; }
        #endregion

        #region RandomAccessRepository members
        public override IEnumerator<Domain> GetEnumerator()
        {
            FileIterator.SearchPattern = "*.lynx";

            IEnumerable<FileInfo> files = from fi in FileIterator
                                          select fi;

            foreach (FileInfo fi in files)
                yield return Get(fi);
        }

        public override Domain Get(FileInfo key)
        {
            Domain result = new Domain();

            // First populate an untyped DataSet with the xml document
            using (DataSet ds = new DataSet())
            {
                using (FileStream fs = new FileStream(key.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    ds.ReadXml(fs, XmlReadMode.ReadSchema);
                }

                // Then fill the typed DataSet from the untyped DataSet
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    var inputTable = ds.Tables[i];
                    string tableType = inputTable.ExtendedProperties[Domain.TableTypeExtendedProperty] as string;

                    if (tableType == Domain.TableTypeLinkSet)
                    {
                        result.Tables.Add(new LinkSet(inputTable));
                    }
                    else
                    {
                        result.Tables.Add(new EntitySet(inputTable));
                    }
                }
                result.AcceptChanges();
            }

            return result;
        }

        public override FileInfo GetKey(Domain item)
        {
            return FileIterator.Get(string.Format("c:\\{0}.lynx", item.DataSetName));
        }

        public override Domain Set(Domain item, FileInfo key)
        {
            item.WriteXml(key.FullName, XmlWriteMode.WriteSchema);
            return item;
        }

        public override bool Delete(FileInfo key)
        {
            return FileIterator.Delete(key.FullName);
        }

        public override Domain Add(Domain item)
        {
            return Set(item, GetKey(item));
        }
        #endregion
    }
}
