using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using Esoteric.DAL.Interface;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.CSharpExchange
{
    [Export]
    public class TypeToDomainTransform : IVisitor<Type>, IVisitor<GenericLink<Type>>
    {
        #region Constants
        const string EntitySet_Type = "Types";
        #endregion

        #region Private Fields
        string[] TypeProperties = new string[] { 
                                                "FullName", "Namespace", "IsAbstract", "IsClass", 
                                                "IsEnum", "IsInterface", "IsPublic", "IsSealed",
                                                "IsSerializable"
                                               };

        #endregion

        #region Private Properties
        EntitySet TypeSet
        {
            get
            {
                if (_typeSet == null)
                {
                    _typeSet = EntitySetRepository.Get(EntitySet_Type);
                    if (_typeSet == null)
                        _typeSet = CreateTypeEntitySet();
                }

                return _typeSet;
            }
        }
        EntitySet _typeSet;

        Dictionary<string, LinkHandler> Handlers
        {
            get 
            {
                if (handlers == null)
                {
                    handlers = new Dictionary<string, LinkHandler>();
                    handlers.Add(TypeExtractor.LinkType_Implements, HandleImplements);
                }
                return handlers; 
            }
        }
        Dictionary<string, LinkHandler> handlers;
        #endregion

        #region IVisitor<Type> Members

        public bool Accept(Type instance)
        {
            var entity = Search(instance);
            if (entity == null)
            {
                entity = TypeSet.NewRow() as Entity;
                TypeSet.Add(entity);

                entity[Domain.NameColumn] = instance.DisplayName();
                FillEntity(instance, entity);
            }

            return true;
        }

        #endregion

        #region IVisitor<GenericLink<Type>> Members
        public bool Accept(GenericLink<Type> link)
        {
            bool result = false;

            LinkHandler handler;
            if( Handlers.TryGetValue(link.LinkType, out handler))
                result = handler(link);

            result = HandleGenericSet(link, link.LinkType);

            // Create a "members" link set to handle files, properties and method returns
            if (link.LinkType == TypeExtractor.LinkType_Fields ||
                link.LinkType == TypeExtractor.LinkType_MethodReturns ||
                link.LinkType == TypeExtractor.LinkType_Properties)
            {
                link.Context = link.LinkType;
                result = HandleGenericSet(link, "Members");
            }

            if (!result)
                Console.WriteLine("Ignoring {0} on {1} -> {2} [{3} {4}]", link.LinkType, link.Source.Name, link.Target.Name, link.Name, link.Context);

            return result;
        }

        #endregion

        #region Load Properties

        public IRandomAccessRepository<EntitySet, string> EntitySetRepository { get; set; }

        public IRandomAccessRepository<LinkSet, string> LinkSetRepository { get; set; }

        #endregion

        #region Link Support
        delegate bool LinkHandler(GenericLink<Type> link);

        bool HandleImplements(GenericLink<Type> link)
        {
            // It turns out, that this will appear for interface implementation on the base class
            // so if the base class implements, the subclass is also shown to implement

            // See if the parent (or grandparent, etc.) are handling the interface
            Type current = link.Source.BaseType;
            Type parentImplements = null;
            while (current != null && parentImplements == null)
            {
                var interfaceName = link.Target.FullName ?? link.Target.Name;

                var iface = current.GetInterface(interfaceName);
                if (iface != null)
                    parentImplements = current;
                current = current.BaseType;
            }

            // Found a parent that implments the interface, return quietly
            if (parentImplements != null)
                return true;

            return HandleGenericSet(link, TypeExtractor.LinkType_Implements);
        }

        bool HandleGenericSet(GenericLink<Type> link, string name)
        {
            var linkSet = LinkSetRepository.Get(name);
            if (linkSet == null)
                linkSet = CreateLinkSet(name);

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

        #region Helper functions
        EntitySet CreateTypeEntitySet()
        {
            EntitySet set = new EntitySet(EntitySet_Type);

            foreach (string propName in TypeProperties)
            {
                var pi = typeof(Type).GetProperty(propName);
                set.AddColumn(pi.Name, pi.PropertyType);    
            }

            EntitySetRepository.Add(set);

            return set;
        }

        LinkSet CreateLinkSet(string name)
        {
            LinkSet set = new LinkSet(TypeSet, TypeSet, name);
            LinkSetRepository.Add(set);
            
            return set;
        }

        void FillEntity(Type instance, Entity entity)
        {
            foreach (string propName in TypeProperties)
            {
                if (propName == "FullName")
                {
                    entity.SetField<string>("FullName", instance.FullName ?? instance.ToString());
                }
                else
                {
                    var pi = typeof(Type).GetProperty(propName);
                    var value = pi.GetValue(instance, null);
                    entity[propName] = value;
                }
            }
        }

        Entity Search(Type instance)
        {
            return (from e in TypeSet
                    where e.Field<string>("FullName") == (instance.FullName ?? instance.ToString())
                    select e).FirstOrDefault();
        }
        #endregion
    }
}
