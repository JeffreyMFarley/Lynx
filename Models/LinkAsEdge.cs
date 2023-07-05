using System;
using System.Data;
using QuickGraph;

namespace Lynx.Models
{
    public class LinkAsEdge : IEdge<Entity>
    {
        Link _link;

        #region Constructors
        public LinkAsEdge(Link link_)
        {
            _link = link_;
        }
        #endregion

        #region Operators
        public static implicit operator Link(LinkAsEdge source)
        {
            return source._link;
        }

        public static implicit operator LinkAsEdge(Link source)
        {
            return new LinkAsEdge(source);
        }
        #endregion

        #region IEdge<Entity> Members

        public Entity Source
        {
            get
            {
                if (source == null)
                {
                    source = (_link.Table as LinkSet).GetSource(_link.SourceID);
                }
                return source;
            }
        }
        Entity source;

        public Entity Target
        {
            get
            {
                if (target == null)
                {
                    target = (_link.Table as LinkSet).GetTarget(_link.TargetID);
                }
                return target;
            }
        }
        Entity target;

        #endregion
    }
}
