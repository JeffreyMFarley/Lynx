using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Esoteric.BLL;
using Esoteric.Collections;
using Esoteric.UI;
using Lynx.Interfaces;
using Lynx.Models;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.ConnectedComponents;
using QuickGraph.Algorithms.MinimumSpanningTree;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms.Search;
using QuickGraph.Algorithms.TopologicalSort;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;

namespace Lynx.UI.ViewModels
{
    public class QuickGraphViewModel : ViewModelBase
    {
        #region Constructor
        public QuickGraphViewModel(IDomainManager domainManager)
        {
            // Save the interface
            DomainManager = domainManager;

            // Create the delegates
            domainTablesChanged = new CollectionChangeEventHandler(DomainTablesChanged);

            // Listen for some events
            DomainManager.TablesCollectionChanged += domainTablesChanged;

            // Add the link sets
            foreach (var linkSet in DomainManager.LinkSets)
                AvailableLinkSets.Add(linkSet.TableName);

            // Set the view
            View = new Views.SvgView { Model = this };
        }
        #endregion

        #region Private Properties
        IDomainManager DomainManager { get; set; }

        bool HasSameEntity
        {
            get
            {
                var ls = CurrentLinkSet;
                return ls.SourceSet.TableName == ls.TargetSet.TableName;
            }
        }

        LinkSet CurrentLinkSet
        {
            get
            {
                return DomainManager.LinkSets.Get(selectedLinkSet);
            }
        }

        Dictionary<string, RenderGraphMethod> Renderers
        {
            get
            {
                if (renderers == null)
                {
                    renderers = new Dictionary<string, RenderGraphMethod>();
                    // Popular
                    renderers.Add("Dependency Sort (Most to Least)", TopologicalSortMostToLeast);
                    renderers.Add("Dependency Sort (Task Order)", TopologicalSortLeastToMost);
                    renderers.Add("Classify", Classify);
                    renderers.Add("Graphviz dot", GraphViz);
                    renderers.Add("Forests", Forests);
                    renderers.Add("Subgraph Cutter", SubgraphCutter);

                    renderers.Add("Tokens", Tokens);
                    renderers.Add("Adjacency", Adjacency);
                    renderers.Add("Triangles", Triangles);


                    // Vertex-Based
                    renderers.Add("Classification - Isolated", IsolatedVertices);
                    renderers.Add("Classification - Source", (() => ClassifiedVertices(ConnectivityClassificationType.Source)));
                    renderers.Add("Classification - Sink", (() => ClassifiedVertices(ConnectivityClassificationType.Sink)));
                    renderers.Add("Classification - Through", (() => ClassifiedVertices(ConnectivityClassificationType.Through)));
                    renderers.Add("Classification - Terminal", (() => ClassifiedVertices(ConnectivityClassificationType.Terminal)));
                    renderers.Add("Classification - Consolidate", (() => ClassifiedVertices(ConnectivityClassificationType.Consolidate)));
                    renderers.Add("Classification - Producer", (() => ClassifiedVertices(ConnectivityClassificationType.Producer)));
                    renderers.Add("Classification - Distribute", (() => ClassifiedVertices(ConnectivityClassificationType.Distribute)));
                    renderers.Add("Classification - Nexus", (() => ClassifiedVertices(ConnectivityClassificationType.Nexus)));
                    renderers.Add("All Vertices", Vertices);

                    // Edge Based
                    renderers.Add("Vertex Predecessor Path", VertexPredecessorPath);
                    renderers.Add("Eulerian Cycles", EulerianTrail);
                    
                    // Vertex Score
                    renderers.Add("Vertex Distance", VertexDistance);
                    renderers.Add("Connected", Connected);
                    renderers.Add("Strongly Connected", StronglyConnected);
                    renderers.Add("Weakly Connected", WeaklyConnected);

                    // Dot Graph
                    renderers.Add("Minimum Spanning Tree", MinimumSpanningTree);
                }
                return renderers;
            }
        }
        Dictionary<string, RenderGraphMethod> renderers;
        #endregion

        #region Binding Properties
        public ICommand RefreshCommand
        {
            get
            {
                return refreshCommand ?? (refreshCommand = new RelayCommand(p => OnPropertyChanged("VisualizedGraph")));
            }
        }
        RelayCommand refreshCommand;

        public ObservableCollection<string> AvailableLinkSets
        {
            get
            {
                return availableLinkSets ?? (availableLinkSets = new ObservableCollection<string>());
            }
        }
        ObservableCollection<string> availableLinkSets;

        public string SelectedLinkSet
        {
            get
            {
                return selectedLinkSet ?? (selectedLinkSet = string.Empty);
            }
            set
            {
                selectedLinkSet = value;
                OnPropertyChanged("SelectedLinkSet");
                OnPropertyChanged("VisualizedGraph");
            }
        }
        string selectedLinkSet;

        public ReadOnlyCollection<string> AvailableRenderers
        {
            get
            {
                return availableRenderers ?? (availableRenderers = new ReadOnlyCollection<string>(Renderers.Keys.ToList()));
            }
        }
        ReadOnlyCollection<string> availableRenderers;

        public string SelectedRenderers
        {
            get
            {
                return selectedRenderers ?? (selectedRenderers = string.Empty);
            }
            set
            {
                selectedRenderers = value;
                OnPropertyChanged("SelectedRenderers");
                OnPropertyChanged("VisualizedGraph");
            }
        }
        string selectedRenderers;

        public object VisualizedGraph
        {
            get
            {
                if (string.IsNullOrEmpty(selectedLinkSet))
                    return "Nothing Selected";

                RenderGraphMethod renderer;
                if( Renderers.TryGetValue(SelectedRenderers, out renderer))
                    return renderer();

                return "No Renderer Selected";   
            }
        }

        #endregion

        #region Event Handlers
        CollectionChangeEventHandler domainTablesChanged;

        void DomainTablesChanged(object sender, CollectionChangeEventArgs e)
        {
            var uiThread = Application.Current.Dispatcher.Thread.ManagedThreadId;
            var currentThread = Thread.CurrentThread.ManagedThreadId;
            if (currentThread != uiThread)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action<object, CollectionChangeEventArgs>(DomainTablesChanged),
                                                           DispatcherPriority.Background, sender, e);
                return;
            }

            if (e.Action == CollectionChangeAction.Add)
            {
                LinkSet linkSet = e.Element as LinkSet;
                if (linkSet != null)
                    AvailableLinkSets.Add(linkSet.TableName);
            }
        }
        #endregion

        #region Algorithm Methods
        bool SafeCompute(IComputation algorithm)
        {
            bool result = false;
            try
            {
                algorithm.Compute();
                result = true;
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message);
            }

            return result;
        }

        delegate string RenderGraphMethod();

        string Connected()
        {
            var un = Graph.BuildUndirected(CurrentLinkSet);
            var algorithm = new ConnectedComponentsAlgorithm<Entity, LinkAsEdge>(un);
            if (!SafeCompute(algorithm))
                return "Cannot compute";
            return VertexScore(algorithm.Components, un.VertexCount);
        }

        string StronglyConnected()
        {
            IDictionary<Entity, int> results = new Dictionary<Entity, int>();
            var graph = Graph.Build(CurrentLinkSet);
            graph.StronglyConnectedComponents(out results);
            return VertexScore(results, graph.VertexCount);
        }

        string WeaklyConnected()
        {
            IDictionary<Entity, int> results = new Dictionary<Entity, int>();
            var graph = Graph.Build(CurrentLinkSet);
            graph.StronglyConnectedComponents(out results);
            graph.WeaklyConnectedComponents(results);
            return VertexScore(results, graph.VertexCount);
        }

        string TopoAlgorithm(bool reverseSort)
        {
            var graph = Graph.Build(CurrentLinkSet);
            graph.ReduceToSimple();

            var forest = Forest.Build(CurrentLinkSet, graph);

            var sb = new StringBuilder();

            foreach (var tree in forest)
            {
                sb.AppendLine();
                sb.AppendFormat("Topological sort of {0}", tree.Name);

                var algorithm = new TopologicalSortAlgorithm<Entity, LinkAsEdge>(tree);
                if (!SafeCompute(algorithm))
                {
                    sb.AppendFormat(" -- Cannot compute. {0} nodes contain at least one cycle.\r\n", tree.VertexCount);
                    sb.Append("The order discovered prior to the cycle:");
                    //continue;
                }

                sb.AppendLine();

                if (reverseSort)
                    sb.Append(Linear(algorithm.SortedVertices.Reverse(), tree.VertexCount));
                else
                    sb.Append(Linear(algorithm.SortedVertices, tree.VertexCount));
            }

            return sb.ToString();
        }

        string TopologicalSortMostToLeast()
        {
            return TopoAlgorithm(true);
        }

        string TopologicalSortLeastToMost()
        {
            return TopoAlgorithm(false);
        }

        string Vertices()
        {
            var graph = Graph.Build(CurrentLinkSet);
            return LinearByName(graph.Vertices, graph.VertexCount);
        }

        string ClassifiedVertices(ConnectivityClassificationType type)
        {
            var graph = Graph.Build(CurrentLinkSet);
            graph.ReduceToSimple();

            var results = new List<Entity>();
            foreach (var vertex in graph.Vertices)
                if( graph.ConnectivityClassification(vertex) == type )
                    results.Add(vertex);

            return Linear(results, graph.VertexCount);
        }

        string IsolatedVertices()
        {
            var graph = Graph.Build(CurrentLinkSet);

            var allEntities = CurrentLinkSet.SourceSet.ToList();
            allEntities.AddRange(CurrentLinkSet.TargetSet.ToList());

            var result = allEntities.Except(graph.Vertices).Distinct();
            return LinearByName(result, allEntities.Count);
        }

        string Forests()
        {
            var forest = Forest.Build(CurrentLinkSet);
            return Disjoint(forest);
        }

        string Classify()
        {
            var graph = Graph.Build(CurrentLinkSet);
            graph.ReduceToSimple();

            IDictionary<Entity, int> results = new Dictionary<Entity, int>();
            foreach (var vertex in graph.Vertices)
                results.Add(vertex, (int) graph.ConnectivityClassification(vertex));

            return VertexScore(results, graph.VertexCount);
        }

        string SubgraphCutter()
        {
            var forest = Forest.Build(CurrentLinkSet);
            var weightingFunc = CurrentLinkSet.WeightingFunction;

            var sb = new StringBuilder();

            foreach (var spanningTree in forest)
            {
                if (spanningTree.VertexCount < 7)
                {
                    sb.AppendFormat("Skipping {0} with a small vertex count of {1}\r\n", spanningTree.Name, spanningTree.VertexCount);
                    continue;
                }

                sb.AppendFormat("\r\nTrimming {0} with a vertex count of {1}\r\n\r\n", spanningTree.Name, spanningTree.VertexCount);

                //Graph spanningTree = null;

                //var un = new UndirectedBidirectionalGraph<Entity, LinkAsEdge>(tree);
                //var algorithm = new KruskalMinimumSpanningTreeAlgorithm<Entity, LinkAsEdge>(un, weightingFunc);
                //var observer = new EdgeRecorderObserver<Entity, LinkAsEdge>();
                //using (observer.Attach(algorithm))
                //{
                //    if (!SafeCompute(algorithm))
                //    {
                //        sb.AppendLine("Cannot compute");
                //        continue;
                //    }

                //    spanningTree = Lynx.Models.Graph.BuildSubgraph(observer.Edges);
                //}

                //if (spanningTree == null)
                //{
                //    sb.AppendLine("Cannot build spanning tree");
                //    continue;
                //}

                var nexuses = new Dictionary<Entity, int>();
                foreach (var vertex in spanningTree.Vertices)
                    nexuses.Add(vertex, spanningTree.Degree(vertex));

                // Get the highest degree count
                int maxDegree = nexuses.Max(kvp => kvp.Value);

                // Remove them from the graph
                if (maxDegree > 2)
                {
                    foreach (var nexus in nexuses.Where(kvp => kvp.Value >= maxDegree).Select(kvp => kvp.Key))
                    {
                        sb.AppendFormat("Removing Max Degree {1}\t{0}", nexus.Name, maxDegree);
                        sb.AppendLine();
                        spanningTree.RemoveVertex(nexus);
                    }
                }

                var terminals = new List<Entity>();
                var producers = new List<Entity>();
                foreach (var vertex in spanningTree.Vertices)
                {
                    switch( spanningTree.ConnectivityClassification(vertex) )
                    {
                        case ConnectivityClassificationType.Terminal:
                            terminals.Add(vertex);
                            break;

                        case ConnectivityClassificationType.Producer:
                            producers.Add(vertex);
                            break;
                    }
                }

                // Remove terminals from the graph
                foreach (var node in terminals)
                {
                    sb.AppendFormat("Removing Terminal\t{0}", node.Name);
                    sb.AppendLine();
                    spanningTree.RemoveVertex(node);
                }

                // Remove producers from the graph
                foreach (var node in producers)
                {
                    sb.AppendFormat("Removing Producer\t{0}", node.Name);
                    sb.AppendLine();
                    spanningTree.RemoveVertex(node);
                }

                // See what effect the removal has on the forest
                var cutForest = Forest.Build(CurrentLinkSet, spanningTree);

                sb.AppendLine();
                sb.AppendLine("Resulting forest");
                sb.AppendLine(Disjoint(cutForest));
            }

            return sb.ToString();
        }

        string MinimumSpanningTree()
        {
            var forest = Forest.Build(CurrentLinkSet);
            var weightingFunc = CurrentLinkSet.WeightingFunction;

            var sb = new StringBuilder();

            foreach (var tree in forest)
            {
                sb.AppendFormat("\r\nTree {0} with a vertex count of {1}\r\n\r\n", tree.Name, tree.VertexCount);

                var un = new UndirectedBidirectionalGraph<Entity, LinkAsEdge>(tree);
                var algorithm = new KruskalMinimumSpanningTreeAlgorithm<Entity, LinkAsEdge>(un, weightingFunc);
                var observer = new EdgeRecorderObserver<Entity, LinkAsEdge>();
                using (observer.Attach(algorithm))
                {
                    if (!SafeCompute(algorithm))
                        sb.AppendLine("Cannot compute");

                    var subgraph = Graph.BuildSubgraph(observer.Edges);
                    if( subgraph == null )
                        sb.AppendLine("Error Building Subgraph");

                    sb.AppendLine(OutputGraphViz(subgraph));
                }
            }

            return sb.ToString();
        }

        string VertexPredecessorPath()
        {
            var graph = Graph.Build(CurrentLinkSet);

            var algorithm = new DepthFirstSearchAlgorithm<Entity, LinkAsEdge>(graph);
            var observer = new VertexPredecessorPathRecorderObserver<Entity, LinkAsEdge>();
            using (observer.Attach(algorithm))
            {
                if (!SafeCompute(algorithm))
                    return "Cannot compute";

                var sb = new StringBuilder();
                sb.AppendLine("End Paths");
                sb.AppendLine(Linear(observer.EndPathVertices, graph.VertexCount));
                sb.AppendLine();
                sb.AppendLine("Parent/Child (not all parents are shown)");
                sb.AppendLine(VertexDictionaryOfEdges(observer.VertexPredecessors, graph.VertexCount));

                return sb.ToString();
            }
        }

        string VertexDistance()
        {
            var graph = Graph.Build(CurrentLinkSet);

            Func<LinkAsEdge, double> weightingFunction = new Func<LinkAsEdge,double>(l => 1);

            var sb = new StringBuilder();
            bool sameTable = HasSameEntity;

            var topo = new TopologicalSortAlgorithm<Entity, LinkAsEdge>(graph);
            if (!SafeCompute(topo))
            {
                sb.AppendFormat(" -- Cannot compute. {0} nodes contain at least one cycle.", graph.VertexCount);
                sb.AppendLine();
                return sb.ToString();
            }

            // Build a dictionary of paths
            var results = new Dictionary<Entity, int>();
            foreach (var node in topo.SortedVertices)
            {
                // Accumulate the predecessor counts
                int accum = 0;
                foreach (var inbound in graph.InEdges(node))
                    accum += results[inbound.Source] + 1;

                results[node] = accum;
            }

            // Write out the header
            if (sameTable)
            {
                sb.Append("Distance\tClassification\t");
                WriteHeader(sb, CurrentLinkSet.SourceSet);
            }

            foreach (var node in topo.SortedVertices)
            {
                sb.AppendFormat("{1}\t{0}\t", Convert.ToInt16(graph.ConnectivityClassification(node)), results[node]);
                WriteNode(sb, node, sameTable);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        string EulerianTrail()
        {
            var forest = Forest.Build(CurrentLinkSet);

            var sb = new StringBuilder();

            foreach (var tree in forest)
            {
                sb.AppendFormat("\r\nTree {0} with a vertex count of {1}\r\n\r\n", tree.Name, tree.VertexCount);

                var algorithm = new EulerianTrailAlgorithm<Entity, LinkAsEdge>(tree);
                var observer = new EdgeRecorderObserver<Entity, LinkAsEdge>();
                using (observer.Attach(algorithm))
                {
                    if (!SafeCompute(algorithm))
                        sb.AppendLine("Cannot compute");

                    sb.AppendLine(Edges(observer.Edges, tree.VertexCount));
                }
            }

            return sb.ToString();
        }

        string GraphViz()
        {
            return OutputGraphViz(Graph.Build(CurrentLinkSet));
        }

        string Tokens()
        {
            var graph = Graph.Build(CurrentLinkSet);
            graph.ReduceToSimple();

            var distribution = OneGramDistribution.Build();
            var segmenter = new WordSegmentation(distribution);

            Regex rgx = new Regex("[^a-zA-Z0-9]");

            var results = from v in graph.Vertices
                          group segmenter.Segment(rgx.Replace(v.Name, "")) by v.Name into g
                          orderby g.Key
                          select g;

            StringBuilder sb = new StringBuilder();
            foreach (var g in results)
            {
                foreach(var s0 in g.SelectMany(s => s))
                    sb.AppendFormat("{0}\t{1}{2}", g.Key, s0, Environment.NewLine);
            }

            return sb.ToString();
        }

        string Adjacency()
        {
            if (!HasSameEntity)
                return "Current Implementation is only on links sets where source = target";

            var a = AdjacencyGraph.Build(CurrentLinkSet);
            return Adjacency(a.Names, a.Matrix);
        }

        string Triangles()
        {
            if (!HasSameEntity)
                return "Current Implementation is only on links sets where source = target";

            var a1 = AdjacencyGraph.Build(CurrentLinkSet);

            var a2 = MatrixInt.Multiply(a1.Matrix, a1.Matrix);
            var a3 = MatrixInt.Multiply(a2, a1.Matrix);

            return Adjacency(a1.Names, a3); //Linear(cycle3List, set.Count);
        }
        #endregion

        #region Output Methods
        string OutputGraphViz(Graph graph)
        {
            var renderer = new GraphvizAlgorithm<Entity, LinkAsEdge>(graph);
            InitializeRender(renderer, graph);

            return renderer.Generate();
        }

        void WriteHeader(StringBuilder sb, EntitySet set)
        {
            sb.Append(Domain.NameColumn);

            DataColumn dc;
            for (int col = 0; col < set.Columns.Count; col++)
            {
                dc = set.Columns[col];
                if (dc.ColumnName != Domain.IDColumn && dc.ColumnName != Domain.NameColumn)
                {
                    sb.AppendFormat("\t{0}", dc.ColumnName);
                }
            }

            sb.AppendLine();
        }

        void WriteNode(StringBuilder sb, Entity node, bool sameTable)
        {
            if (!sameTable)
                sb.AppendFormat("{0}\t", node.Table.TableName);

            sb.Append(node.Name);

            if (sameTable)
            {
                DataColumn dc;
                for (int col = 0; col < node.Table.Columns.Count; col++)
                {
                    dc = node.Table.Columns[col];
                    if (dc.ColumnName != Domain.IDColumn && dc.ColumnName != Domain.NameColumn)
                    {
                        sb.AppendFormat("\t{0}", node[dc.ColumnName]);
                    }
                }
            }
        }

        string Adjacency(IList<string> set, MatrixInt adjacency)
        {
            StringBuilder sb = new StringBuilder();

            for (int r = 0; r < set.Count; r++)
            {
                sb.AppendFormat("\t{0}", set[r]);
            }
            sb.AppendLine();

            for (int r = 0; r < set.Count; r++)
            {
                sb.Append(set[r]);
                for (int c = 0; c < set.Count; c++)
                {
                    sb.AppendFormat("{0}\t", adjacency[r, c]);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        string Linear(IEnumerable<Entity> nodes, int totalNodes)
        {
            var sb = new StringBuilder();

            int nodeCount = nodes.Count();
            sb.AppendFormat("{0} of {1}", nodeCount, totalNodes);
            sb.AppendLine();
            sb.AppendLine();

            bool sameTable = HasSameEntity;

            // Write out the header
            if (sameTable)
                WriteHeader(sb, CurrentLinkSet.SourceSet);

            // Write the rows
            foreach (var node in nodes)
            {
                WriteNode(sb, node, sameTable);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        string LinearByName(IEnumerable<Entity> nodes, int totalNodes)
        {
            return Linear(nodes.OrderBy(e => e.Name), totalNodes);
        }

        string Edges(IEnumerable<LinkAsEdge> edges, int totalNodes)
        {
            var sb = new StringBuilder();

            var vertices = new List<Entity>();
            vertices.AddRange(edges.Select(v => v.Source));
            vertices.AddRange(edges.Select(v => v.Target));

            int nodeCount = vertices.Distinct().Count();
            sb.AppendFormat("{0} of {1}", nodeCount, totalNodes);
            sb.AppendLine();
            sb.AppendLine();

            bool showTables = !HasSameEntity;
            foreach (var edge in edges)
            {
                var node = edge.Source;
                if (showTables)
                    sb.AppendFormat("{0}\t", node.Table.TableName);
                sb.Append(node.Name);

                sb.Append("\t->\t");

                node = edge.Target;
                if (showTables)
                    sb.AppendFormat("{0}\t", node.Table.TableName);
                sb.Append(node.Name);
                
                sb.AppendLine();
            }

            return sb.ToString();
        }

        string VertexScore(IDictionary<Entity, int> nodes, int totalNodes)
        {
            var sb = new StringBuilder();

            var filter = new Func<KeyValuePair<Entity, int>, bool>(kvp => kvp.Value > 0);

            int nodeCount = nodes.Count(filter);
            sb.AppendFormat("{0} of {1}", nodeCount, totalNodes);
            sb.AppendLine();
            sb.AppendLine();

            bool sameTable = HasSameEntity;

            // Write out the header
            if (sameTable)
            {
                sb.Append("Score\t");
                WriteHeader(sb, CurrentLinkSet.SourceSet);
            }

            // Write the rows
            foreach (var node in nodes.Where(filter).OrderBy(kvp => kvp.Key.Name))
            {
                sb.AppendFormat("{0}\t", node.Value);
                WriteNode(sb, node.Key, sameTable);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        string VertexDictionaryOfEdges(IDictionary<Entity, LinkAsEdge> nodes, int totalNodes)
        {
            var sb = new StringBuilder();

            int nodeCount = nodes.Count();
            sb.AppendFormat("{0} of {1}", nodeCount, totalNodes);
            sb.AppendLine();
            sb.AppendLine();

            bool showTables = !HasSameEntity;
            foreach (var node in nodes)
            {

                Entity source;
                Entity target;

                if (node.Key.ID == node.Value.Source.ID)
                {
                    source = node.Key;
                    target = node.Value.Target;
                }
                else
                {
                    source = node.Value.Source;
                    target = node.Key;
                }

                if (showTables)
                    sb.AppendFormat("{0}\t", source.Table.TableName);
                sb.AppendFormat("{0}\t->\t", source.Name);
                if (showTables)
                    sb.AppendFormat("{0}\t", target.Table.TableName);
                sb.Append(target.Name);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        string Disjoint(Forest forest)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0} sets", forest.Count);
            sb.AppendLine();
            sb.AppendLine();

            bool showTables = !HasSameEntity;
            foreach (var graph in forest)
            {
                foreach (var node in graph.Vertices)
                {
                    sb.AppendFormat("{0}\t", graph.Name);
                    if (showTables)
                        sb.AppendFormat("{0}\t", node.Table.TableName);
                    sb.Append(node.Name);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
        #endregion

        #region GraphViz Render
        void InitializeRender(GraphvizAlgorithm<Entity, LinkAsEdge> renderer, Graph sourceGraph)
        {
            var graphFormat = renderer.GraphFormat;
            renderer.ImageType = GraphvizImageType.Svg;

            //graphFormat.ClusterRank = GraphvizClusterMode.Global;
            //graphFormat.IsCentered = true;
            //graphFormat.IsCompounded = true;
            //graphFormat.IsConcentrated = true;
            //graphFormat.OutputOrder = GraphvizOutputMode.BreadthFirst;
            //graphFormat.PageDirection = GraphvizPageDirection.LB;
            //graphFormat.PageSize = new System.Drawing.SizeF(1000, 1000);
            graphFormat.RankDirection = GraphvizRankDirection.LR;
            //graphFormat.Ratio = GraphvizRatioMode.Compress;

            var forest = Forest.Build(CurrentLinkSet, sourceGraph);

            renderer.FormatEdge += new FormatEdgeAction<Entity, LinkAsEdge>(renderer_FormatEdge);
            renderer.FormatVertex += new FormatVertexEventHandler<Entity>((o, e) => renderer_FormatVertex(o, e, forest));
        }

        void renderer_FormatVertex(object sender, FormatVertexEventArgs<Entity> e, Forest forest)
        {
            string groupName = e.Vertex.Table.TableName;
            foreach (var tree in forest)
            {
                if (tree.Vertices.Contains(e.Vertex))
                {
                    groupName = tree.Name;
                    break;
                }
            }

            e.VertexFormatter.Label = e.Vertex.Name;
            //e.VertexFormatter.ToolTip = e.Vertex.Description;
            e.VertexFormatter.Group = groupName;

            if (e.Vertex.Table.Columns.Contains("Shape"))
            {
                var shape = e.Vertex.Field<string>("Shape");
                if (!string.IsNullOrEmpty(shape))
                {
                    switch (shape.ToLower())
                    {
                        case "box": e.VertexFormatter.Shape = GraphvizVertexShape.Box; break;
                        case "circle": e.VertexFormatter.Shape = GraphvizVertexShape.Circle; break;
                        case "diamond": e.VertexFormatter.Shape = GraphvizVertexShape.Diamond; break;
                        case "doublecircle": e.VertexFormatter.Shape = GraphvizVertexShape.DoubleCircle; break;
                        case "ellipse": e.VertexFormatter.Shape = GraphvizVertexShape.Ellipse; break;
                        case "house": e.VertexFormatter.Shape = GraphvizVertexShape.House; break;
                        case "invhouse": e.VertexFormatter.Shape = GraphvizVertexShape.InvHouse; break;
                        case "octagon": e.VertexFormatter.Shape = GraphvizVertexShape.Octagon; break;
                    }
                }
            }
                
        }

        void renderer_FormatEdge(object sender, FormatEdgeEventArgs<Entity, LinkAsEdge> e)
        {
        }
        #endregion

        #region Destructor
        ~QuickGraphViewModel()
        {
            DomainManager.TablesCollectionChanged -= domainTablesChanged;
        }
        #endregion
    }
}
