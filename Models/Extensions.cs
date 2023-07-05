using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using QuickGraph;

namespace Lynx.Models
{
    static public class Extensions
    {
        static public DataColumn Clone(this DataColumn source)
        {
            DataColumn destination = new DataColumn();

            try
            {
                foreach (var property in typeof(DataColumn).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    // Copy over scalar properties
                    object value = property.GetValue(source, null);
                    if (property.CanWrite)
                        property.SetValue(destination, value, null);
                }
            }
            catch
            {
                destination.Dispose();
                destination = null;
            }

            return destination;
        }

        #region Graph Extensions

        static public ConnectivityClassificationType ConnectivityClassification(this Graph graph, Entity vertex)
        {
            ConnectivityClassificationType classification = ConnectivityClassificationType.Isolated;
            int inDegree = graph.InDegree(vertex);
            int outDegree = graph.OutDegree(vertex);

            if (inDegree > 0)
                classification |= ConnectivityClassificationType.HasInput;
            if (inDegree > 1)
                classification |= ConnectivityClassificationType.MultipleInput;
            if (outDegree > 0)
                classification |= ConnectivityClassificationType.HasOutput;
            if (outDegree > 1)
                classification |= ConnectivityClassificationType.MultipleOutput;

            return classification;
        }

        #endregion
    }
}
