using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace Lynx.Models
{
    public class Graph : BidirectionalGraph<Entity, LinkAsEdge>
    {
        #region Factory Methods
        static public Graph Build(LinkSet set)
        {
            // Bouncer code
            if (set == null) return null;

            var graph = new Graph();
            foreach (LinkAsEdge link in set)
            {
                Entity entity = link.Source;
                if (!graph.Vertices.Contains(entity))
                    graph.AddVertex(entity);

                entity = link.Target;
                if (!graph.Vertices.Contains(entity))
                    graph.AddVertex(entity);

                graph.AddEdge(link);
            }

            return graph;
        }

        static public UndirectedBidirectionalGraph<Entity, LinkAsEdge> BuildUndirected(LinkSet set)
        {
            // Bouncer code
            if (set == null) return null;

            return new UndirectedBidirectionalGraph<Entity, LinkAsEdge>(Build(set));
        }

        // Builds a subgraph where all the connections are internal
        static public Graph BuildSubgraph(LinkSet set, IList<Entity> entities)
        {
            // Bouncer code
            if (set == null) return null;
            if (entities == null) return null;

            var graph = new Graph();

            // Already know the subgraph vertices
            graph.AddVertexRange(entities);

            // Add the edges
            foreach (LinkAsEdge link in set)
            {
                Entity source = link.Source;
                Entity target = link.Target;

                if( entities.Contains(source) && entities.Contains(target) )
                    graph.AddEdge(link);
            }

            return graph;
        }

        // Builds a subgraph from a spanning tree
        static public Graph BuildSubgraph(IList<LinkAsEdge> spanningTree)
        {
            // Bouncer code
            if (spanningTree == null) return null;

            var graph = new Graph();
            foreach (LinkAsEdge link in spanningTree)
            {
                Entity entity = link.Source;
                if (!graph.Vertices.Contains(entity))
                    graph.AddVertex(entity);

                entity = link.Target;
                if (!graph.Vertices.Contains(entity))
                    graph.AddVertex(entity);

                graph.AddEdge(link);
            }

            return graph;
        }
        #endregion

        public string Name { get; set; }

        /// <summary>
        /// Remove self-links and multiple links between same nodes
        /// </summary>
        public void ReduceToSimple()
        {
            // Find self links 
            var selfLinks = new List<LinkAsEdge>();
            foreach (var edge in Edges)
            {
                if (edge.IsSelfEdge<Entity, LinkAsEdge>())
                    selfLinks.Add(edge);
            }

            // Remove self links
            foreach (var edge in selfLinks)
                RemoveEdge(edge);

            // TODO: Remove duplicate links
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
