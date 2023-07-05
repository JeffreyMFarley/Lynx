using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Esoteric.DAL.Interface;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.XsdExchange
{
    public class XsdToDomainTransform : IVisitor<XElement>, IVisitor<GenericLink<XElement>>
    {
        #region Constants
        const string EntitySet_XsdData = "Nodes";
        #endregion

        #region Private Fields
        string[] ModelColumns = new string[] {  "KindName",
                                                "Namespace"
                                               };

        #endregion

        #region Private Properties
        EntitySet ModelSet
        {
            get
            {
                if (_modelSet == null)
                {
                    _modelSet = EntitySetRepository.Get(EntitySet_XsdData);
                    if (_modelSet == null)
                        _modelSet = CreateEntitySet();
                }

                return _modelSet;
            }
        }
        EntitySet _modelSet;
        #endregion

        #region Load Properties
        public IRandomAccessRepository<EntitySet, string> EntitySetRepository { get; set; }

        public IRandomAccessRepository<LinkSet, string> LinkSetRepository { get; set; }
        #endregion

        #region IVisitor<XElement> Members

        public bool Accept(XElement instance)
        {
            try
            {
                var entity = Search(instance);
                if (entity == null)
                {
                    entity = ModelSet.NewRow() as Entity;
                    ModelSet.Add(entity);

                    entity[Domain.IDColumn] = Guid.Parse(instance.Attribute("lynxid").Value);
                    entity["KindName"] = instance.Name.LocalName;

                    XAttribute attr = instance.Attribute("name");
                    if (attr != null)
                    {
                        entity[Domain.NameColumn] = attr.Value;
                        entity["Namespace"] = instance.Name.Namespace;
                    }
                    else
                        entity[Domain.NameColumn] = "Unnamed";
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion

        #region IVisitor<GenericLink<XElement>> Members

        public bool Accept(GenericLink<XElement> link)
        {
            var linkSet = LinkSetRepository.Get(link.LinkType);
            if (linkSet == null)
                linkSet = CreateLinkSet(link.LinkType);

            var source = Search(link.Source);
            var target = Search(link.Target);

            if (source == null || target == null)
                return false;

            var domainLink = (from p in linkSet
                              where p.SourceID == source.ID
                                 && p.TargetID == target.ID
                                 && p.Name == link.Name
                              select p).FirstOrDefault();
            if (domainLink == null)
            {
                domainLink = linkSet.NewRow() as Link;
                domainLink.SourceID = source.ID;
                domainLink.TargetID = target.ID;
                domainLink.Name = link.Name;
                domainLink.Description = link.Context;
                linkSet.Add(domainLink);
            }

            return true;
        }

        #endregion

        #region Link Support
        #endregion

        #region Helper functions
        EntitySet CreateEntitySet()
        {
            EntitySet set = new EntitySet(EntitySet_XsdData);

            foreach (string name in ModelColumns)
            {
                set.AddColumn(name, typeof(string));
            }

            EntitySetRepository.Add(set);

            return set;
        }

        LinkSet CreateLinkSet(string name)
        {
            LinkSet set = new LinkSet(ModelSet, ModelSet, name);
            LinkSetRepository.Add(set);

            return set;
        }

        Entity Search(XElement instance)
        {
            if (instance == null)
                return null;

            XAttribute attr = instance.Attribute("lynxid");
            if( attr == null )
                return null;

            Guid id = Guid.Parse(attr.Value);

            return ModelSet.FirstOrDefault(e => e.ID == id);
        }
        #endregion
    }
}
