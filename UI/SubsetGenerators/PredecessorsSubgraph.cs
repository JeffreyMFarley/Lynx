using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using Esoteric.UI;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.UI.SubsetGenerators
{
    [Export(typeof(IGenerateSubset))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PredecessorsSubgraph : ViewModelBase, IGenerateSubset
    {
        #region Constructor
        public PredecessorsSubgraph()
        {
            View = new PredecessorsSubgraphOptions { Model = this };
        }
        #endregion

        #region XAML Binding Properties
        public ObservableCollection<string> AvailableVertices
        {
            get
            {
                return availableVertices ?? (availableVertices = new ObservableCollection<string>());
            }
        }
        ObservableCollection<string> availableVertices;

        public string SelectedVertex
        {
            get
            {
                return selectedVertex ?? (selectedVertex = string.Empty);
            }
            set
            {
                selectedVertex = value;
                OnPropertyChanged("SelectedVertex");
            }
        }
        string selectedVertex;

        public ObservableCollection<ConnectivityClassificationType> AvailableStoppingPoints
        {
            get
            {
                if (availableStoppingPoints == null)
                {
                    availableStoppingPoints = new ObservableCollection<ConnectivityClassificationType>();
                    availableStoppingPoints.Add(ConnectivityClassificationType.Source);
                    availableStoppingPoints.Add(ConnectivityClassificationType.Producer);
                    availableStoppingPoints.Add(ConnectivityClassificationType.Distribute);
                    availableStoppingPoints.Add(ConnectivityClassificationType.Nexus);
                }
                return availableStoppingPoints;
            }
        }
        ObservableCollection<ConnectivityClassificationType> availableStoppingPoints;

        public ConnectivityClassificationType SelectedStoppingPoint
        {
            get
            {
                return selectedStoppingPoint;
            }
            set
            {
                selectedStoppingPoint = value;
                OnPropertyChanged("SelectedStoppingPoint");
            }
        }
        ConnectivityClassificationType selectedStoppingPoint = ConnectivityClassificationType.Nexus;
	
        #endregion

        #region IGenerateSubset Members

        public string GeneratorName
        {
            get { return "Predecessors Subgraph"; }
        }

        public LinkSet Relationships
        {
            get
            {
                return linkSet;
            }
            set
            {
                linkSet = value;

                // Update the list of available vertices
                AvailableVertices.Clear();
                if (linkSet != null)
                {
                    var graph = Graph.Build(linkSet);
                    foreach (var entity in graph.Vertices.OrderBy(v => v.Name))
                        AvailableVertices.Add(entity.Name);
                }

                // Tell the screen to update
                OnPropertyChanged("Relationships");
                SelectedVertex = null;
                OnPropertyChanged("AvailableVertices");
            }
        }
        LinkSet linkSet;

        public string TableName
        {
            get;
            set;
        }

        public string ErrorMessage
        {
            get
            {
                return errorMessage ?? (errorMessage = string.Empty);
            }
            set
            {
                errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }
        string errorMessage;

        public bool IsValid()
        {
            var sb = new StringBuilder();

            if (Relationships == null)
                sb.AppendLine("The LinkSet must be specfied");

            if (string.IsNullOrEmpty(TableName))
                sb.AppendLine("Table Name must be specified");

            if (TableName.Contains(" "))
                sb.AppendLine("Table Name cannot contain spaces");

            if (string.IsNullOrEmpty(SelectedVertex))
                sb.AppendLine("A vertex must be selected");

            ErrorMessage = sb.ToString();
            return string.IsNullOrEmpty(ErrorMessage);
        }

        public LinkSet Generate()
        {
            // Create a clone of the existing table
            var subset = Relationships.Clone();
            subset.TableName = TableName;
            subset.SourceSet = Relationships.SourceSet;
            subset.TargetSet = Relationships.TargetSet;

            // Build a graph
            var graph = Graph.Build(Relationships);
            graph.ReduceToSimple();

            // Get the starting link
            var node = graph.Vertices.FirstOrDefault(v => v.Name == SelectedVertex);
            var queue = new Queue<Entity>();
            queue.Enqueue(node);

            // Build the backwards list
            while (queue.Count > 0)
            {
                node = queue.Dequeue();
                UpdateLinkSet(subset, node);

                if (graph.ContainsVertex(node))
                {
                    foreach (var edge in graph.InEdges(node))
                    {
                        if (graph.ConnectivityClassification(edge.Source) != SelectedStoppingPoint)
                            queue.Enqueue(edge.Source);
                    }
                }

                // Remove the node from the graph to avoid loops
                graph.RemoveVertex(node);
            }

            return subset;
        }

        void UpdateLinkSet(LinkSet subset, Entity e)
        {
            // build the query
            var query = Relationships.Where(l => l.TargetID == e.ID);

            // Get the subset of records that exclusively include subgraph vertices
            foreach (Link link in query)
            {
                LinkAsEdge asEdge = link;
                Entity source = asEdge.Source;
                Entity target = asEdge.Target;

                var newLink = subset.NewRow() as Link;

                for (int col = 0; col < subset.Columns.Count; col++)
                {
                    DataColumn dc = subset.Columns[col];
                    if (dc.ColumnName != Domain.IDColumn && !LinkSet.IsSystemColumn(dc))
                    {
                        var value = link[dc.ColumnName];
                        newLink[dc.ColumnName] = value;
                    }
                }

                subset.Rows.Add(newLink);
            }
        }

        #endregion
    }
}
