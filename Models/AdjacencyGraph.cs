using System;
using System.Collections.Generic;
using System.Linq;
using Esoteric.Collections;
using System.Text;

namespace Lynx.Models
{
    public class AdjacencyGraph
    {
        #region Factory Methods
        static public AdjacencyGraph Build(LinkSet links, bool undirected = false)
        {
            if (links.SourceSet != links.TargetSet)
                throw new InvalidOperationException("This only works on links between the same entity right now");

            var result = new AdjacencyGraph();

            var set = (from r in links.SourceSet orderby r.Name select r).ToList();
            result.Names = set.Select(r => r.Name).ToArray();
            result.Matrix = new MatrixInt(set.Count, set.Count);

            var p = links.AsParallel().Select(l => l);
            p.ForAll(link => 
            {
                int row = set.FindIndex(r => r.ID == link.SourceID);
                int col = set.FindIndex(r => r.ID == link.TargetID);
                result.Matrix[row, col] += 1;

                if (undirected)
                    result.Matrix[col, row] += 1;
            });

            return result;
        }
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                return (Names != null) ? Names.Count : 0;
            }
        }

        public IList<string> Names
        {
            get;
            private set;
        }

        public MatrixInt Matrix
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        
        #endregion
    }
}
