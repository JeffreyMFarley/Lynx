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
    public class SingleVertex : ViewModelBase, IGenerateSubset
    {
        #region Constructor
        public SingleVertex()
        {
            View = new SingleVertexOptionsView { Model = this };
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
        #endregion

        #region IGenerateSubset Members
        virtual public string GeneratorName 
        {
            get
            {
                return "From Single Vertex";
            }
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

            if( Relationships == null )
                sb.AppendLine("The LinkSet must be specfied");

            if( string.IsNullOrEmpty(TableName) )
                sb.AppendLine("Table Name must be specified");

            if( TableName.Contains(" ") )
                sb.AppendLine("Table Name cannot contain spaces");

            if( string.IsNullOrEmpty(SelectedVertex) )
                sb.AppendLine("A vertex must be selected");

            ErrorMessage = sb.ToString();
            return string.IsNullOrEmpty(ErrorMessage);
        }

        virtual public LinkSet Generate()
        {
            // Create a clone of the existing table
            var subset = Relationships.Clone();
            subset.TableName = TableName;
            subset.SourceSet = Relationships.SourceSet;
            subset.TargetSet = Relationships.TargetSet;

            // Get the subset of records that exclusively include subgraph vertices
            foreach (Link link in Relationships)
            {
                LinkAsEdge asEdge = link;
                Entity source = asEdge.Source;
                Entity target = asEdge.Target;

                if (source.Name == SelectedVertex || target.Name == SelectedVertex)
                {
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

            return subset;
        }

        #endregion
    }
}
