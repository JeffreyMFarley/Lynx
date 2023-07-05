using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Esoteric.UI;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.UI.SubsetGenerators
{
    [Export(typeof(IGenerateSubset))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SetMathematics : ViewModelBase, IGenerateSubset
    {
        const string DefaultOperation = "Complement A - B";

        #region Constructor
        public SetMathematics()
        {
            View = new SetMathematicsOptionsView { Model = this };
        }
        #endregion

        #region IoC Properties
        [Import]
        protected IActiveDomain ActiveDomain { get; set; }
        #endregion

        #region XAML Binding Properties
        public ObservableCollection<string> AvailableOperations
        {
            get
            {
                if (availableOperations == null ) 
                {
                    availableOperations = new ObservableCollection<string>();
                    availableOperations.Add(DefaultOperation);
                }
                return availableOperations;
            }
        }
        ObservableCollection<string> availableOperations;

        public string SelectedOperation
        {
            get
            {
                return selectedOperation ?? (selectedOperation = DefaultOperation);
            }
            set
            {
                selectedOperation = value;
                OnPropertyChanged("SelectedOperation");
            }
        }
        string selectedOperation;

        public ObservableCollection<string> AvailableOperands
        {
            get
            {
                return availableOperands ?? (availableOperands = new ObservableCollection<string>());
            }
        }
        ObservableCollection<string> availableOperands;

        public string SelectedOperand
        {
            get
            {
                return selectedOperand;
            }
            set
            {
                selectedOperand = value;
                OnPropertyChanged("SelectedOperand");
            }
        }
        string selectedOperand;
        #endregion

        #region IGenerateSubset Members
        virtual public string GeneratorName 
        {
            get
            {
                return "Set Mathematics";
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
                AvailableOperands.Clear();
                if (linkSet != null)
                {
                    var clause = new Func<LinkSet, bool>(s => s.SourceSet.TableName == linkSet.SourceSet.TableName
                                                           && s.TargetSet.TableName == linkSet.TargetSet.TableName
                                                           && s.TableName != linkSet.TableName);

                    foreach (var set in ActiveDomain.Manager.LinkSets.Where(clause))
                        AvailableOperands.Add(set.TableName);
                }

                // Tell the screen to update
                OnPropertyChanged("Relationships");
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

            if (string.IsNullOrEmpty(selectedOperation))
                sb.AppendLine("An operation must be specified");

            if (string.IsNullOrEmpty(SelectedOperand))
                sb.AppendLine("The other link set must be specified");

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

            // Get the selected link set
            var other = ActiveDomain.Manager.LinkSets.Get(SelectedOperand);

            // Create the comparer
            var comparer = new LinkComparer();

            Trace.TraceInformation("Nodes in A: {0}, Nodes in B: {1}", Relationships.Count, other.Count);
            Trace.TraceInformation("Distinct Nodes in A: {0}, Distinct Nodes in B: {1}", Relationships.Distinct(comparer).Count(), other.Distinct(comparer).Count());

            // Get the records that ___ the other link set
            IEnumerable<Link> results;
            results = Relationships.Except(other, comparer);

            // Add to the new set
            foreach (Link link in results)
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

            Trace.TraceInformation("Nodes in result: {0}", subset.Count);

            return subset;
        }

        #endregion
    }
}
