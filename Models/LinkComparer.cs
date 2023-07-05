using System.Collections.Generic;

namespace Lynx.Models
{
    public class LinkComparer : IEqualityComparer<Link>
    {
        #region IEqualityComparer<Link> Members
        public bool Equals(Link x, Link y)
        {
            return (x.SourceID.Equals(y.SourceID) && x.TargetID.Equals(y.TargetID));
        }

        public int GetHashCode(Link obj)
        {
            return 0;
        }
        #endregion
    }
}
