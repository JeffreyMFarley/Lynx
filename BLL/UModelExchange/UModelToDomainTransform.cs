using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using Esoteric.DAL.Interface;
using Lynx.Interfaces;
using Lynx.Models;
using UModelLib;

namespace Lynx.UModelExchange
{
    public class UModelToDomainTransform : IVisitor<IUMLData>, IVisitor<GenericLink<IUMLData>>
    {
        #region Constants
        const string EntitySet_UmlData = "Model";
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
                    _modelSet = EntitySetRepository.Get(EntitySet_UmlData);
                    if (_modelSet == null)
                        _modelSet = CreateEntitySet();
                }

                return _modelSet;
            }
        }
        EntitySet _modelSet;
        #endregion

        #region IVisitor<IUMLData> Members

        public bool Accept(IUMLData instance)
        {
            try
            {
                var entity = Search(instance);
                if (entity == null)
                {
                    entity = ModelSet.NewRow() as Entity;
                    ModelSet.Add(entity);

                    entity[Domain.IDColumn] = Guid.Parse(instance.UUID);
                    entity["KindName"] = instance.KindName;

                    IUMLNamedElement named = instance as IUMLNamedElement;
                    if (named != null)
                    {
                        entity[Domain.NameColumn] = named.Name;
                        entity["Namespace"] = named.QualifiedName;

                        if( string.IsNullOrEmpty(named.QualifiedName) && named.Namespace != null )
                            entity["Namespace"] = named.Namespace.Name;

                        if (string.IsNullOrEmpty(entity.Field<string>("Namespace")) && named.Owner != null)
                            entity["Namespace"] = named.Owner.KindName + "subelem";
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

        #region IVisitor<GenericLink<IUMLData>> Members

        public bool Accept(GenericLink<IUMLData> link)
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

        #region Load Properties
        public IRandomAccessRepository<EntitySet, string> EntitySetRepository { get; set; }

        public IRandomAccessRepository<LinkSet, string> LinkSetRepository { get; set; }
        #endregion

        #region Link Support
        #endregion

        #region Helper functions
        EntitySet CreateEntitySet()
        {
            EntitySet set = new EntitySet(EntitySet_UmlData);

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

        Entity Search(IUMLData instance)
        {
            if (instance == null)
                return null;

            Guid id = Guid.Parse(instance.UUID);
            return ModelSet.FirstOrDefault(e => e.ID == id);
        }
        #endregion
    }
}
