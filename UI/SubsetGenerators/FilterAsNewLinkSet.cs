using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Esoteric.UI;
using Infragistics.Windows.DataPresenter;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.UI.SubsetGenerators
{
    public class FilterAsNewLinkSet : ViewModelBase, IGenerateSubset
    {
        #region Constants
        static Regex captureRevision = new Regex("^(?<Title>.+?)(?<Revision>\\d+)$", RegexOptions.Compiled);
        #endregion

        #region Constructor
        public FilterAsNewLinkSet()
        {
           View = new TableNameView { Model = this };
        }
        #endregion

        #region Other Properties
        public XamDataGrid Grid
        {
            get
            {
                return grid;
            }
            set
            {
                grid = value;
                OnPropertyChanged("Grid");
            }
        }
        XamDataGrid grid;
        #endregion

        #region IGenerateSubset Members
        public string GeneratorName 
        {
            get
            {
                return "From Grid Filter";
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
                OnPropertyChanged("Relationships");
            }
        }
        LinkSet linkSet;

        public string TableName
        {
            get
            {
                return tableName ?? (tableName = string.Empty);
            }
            set
            {
                tableName = value;
                OnPropertyChanged("TableName");
            }
        }
        string tableName;

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

            if (Grid == null)
                sb.AppendLine("The grid must be specified");

            if( Relationships == null )
                sb.AppendLine("The LinkSet must be specfied");

            if( string.IsNullOrEmpty(TableName) )
                sb.AppendLine("Table Name must be specified");

            if( TableName.Contains(" ") )
                sb.AppendLine("Table Name cannot contain spaces");

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

            // Get the filtered set
            foreach (var record in Grid.RecordManager.GetFilteredInDataRecords())
            {
                var currentlink = (record.DataItem as DataRowView).Row as Link;
                var newLink = subset.NewRow() as Link;

                for (int col = 0; col < subset.Columns.Count; col++)
                {
                    DataColumn dc = subset.Columns[col];
                    if (dc.ColumnName != Domain.IDColumn && !LinkSet.IsSystemColumn(dc))
                    {
                        var value = currentlink[dc.ColumnName];
                        newLink[dc.ColumnName] = value;
                    }
                }

                subset.Rows.Add(newLink);
            }

            return subset;
        }

        #endregion
    }
}
