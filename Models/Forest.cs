using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Esoteric;
using Esoteric.DAL.Interface;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Collections;

namespace Lynx.Models
{
    public class Forest : IReadOnlyRepository<Graph, string>
    {
        #region Private Properties
        Dictionary<string, Graph> Graphs
        {
            get { return graphs ?? (graphs = new Dictionary<string, Graph>()); }
        }
        Dictionary<string, Graph> graphs;
        #endregion

        #region Factory Methods
        static public Forest Build(LinkSet linkSet)
        {
            return Build(linkSet, Graph.Build(linkSet));
        }

        static public Forest Build(LinkSet linkSet, Graph graph)
        {
            // Bouncer code
            if (linkSet == null) return null;
            if (graph == null) return null;

            // Have Quickgraph determine the unconnected sets
            var un = new UndirectedBidirectionalGraph<Entity, LinkAsEdge>(graph);
            var disjoint = un.ComputeDisjointSet();

            var trees = new List<Entity>();
            var belongs = new Dictionary<Entity, Guid>();

            // Sort the vertices
            foreach (var entity in graph.Vertices)
            {
                var set = disjoint.FindSet(entity);
                if (!trees.Contains(set))
                    trees.Add(set);
                belongs.Add(entity, set.ID);
            }

            // Make the forest object
            var forest = new Forest();

            // Make the graphs
            foreach (var tree in trees)
            {
                var entities = belongs.Where(kvp => kvp.Value == tree.ID).Select(kvp => kvp.Key).ToList();
                var subgraph = Graph.BuildSubgraph(linkSet, entities);
                if (subgraph != null)
                {
                    var set = disjoint.FindSet(tree);

                    // see if the name is already in the forest
                    if( forest.Graphs.ContainsKey(tree.Name) )
                        subgraph.Name = tree.Name + Guid.NewGuid().ToString();
                    else
                        subgraph.Name = tree.Name;
                    forest.Graphs.Add(subgraph.Name, subgraph);
                }
            }

            return forest;
        }
        #endregion

        #region IEnumerable<Graph> Members
        public IEnumerator<Graph> GetEnumerator()
        {
            return Graphs.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Graphs.Values.GetEnumerator();
        }
        #endregion

        #region IReadOnlyRepository<Graph,string> Members
        public Graph Get(string key)
        {
            return Graphs[key];
        }

        #endregion

        #region IRepository Members
        public int? Count
        {
            get { return Graphs.Count; }
        }
        #endregion

        #region IGetByCriteria<Graph> Members

        public List<Graph> GetByCriteria(QueryBase<Graph> query)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
