using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lynx.Interfaces;
using Lynx.Models;
using UModelLib;

namespace Lynx.UModelExchange
{
    public class UModelExtractor : IFileIterator<IUMLData>, IDisposable
    {
        #region internal constants
        internal const string LinkType_Inherits = "Inherits";
        internal const string LinkType_Fields = "Fields";
        internal const string LinkType_GenericParameters = "GenericParameters";
        internal const string LinkType_GenericConstraints = "GenericConstraints";
        internal const string LinkType_GenericTypeDefinitions = "GenericTypeDefinitions";
        internal const string LinkType_Implements = "Implements";
        internal const string LinkType_MethodReturns = "MethodReturns";
        internal const string LinkType_MethodParameters = "MethodParameters";
        internal const string LinkType_Properties = "Properties";
        internal const string LinkType_Exports = "Exports";
        internal const string LinkType_Imports = "Imports";
        #endregion

        #region Private Properties
        public UModelRepository Repository
        {
            get
            {
                return repository ?? (repository = new UModelRepository());
            }
        }
        UModelRepository repository;
        #endregion

        #region Public Methods
        public int ProgressMax
        {
            get;
            set;
        }
        #endregion

        #region IFileIterator<IUMLData> Members
        public FileInfo Source
        {
            get;
            set;
        }

        public IEnumerable<IUMLData> EnumerateEntities()
        {
            var unknown = Activator.CreateInstance(Type.GetTypeFromProgID("UModel.Application"));

            var umodelApp = unknown as IApplication;
            if (umodelApp == null)
                yield break;

            var document = umodelApp.OpenDocument(Source.FullName);

            // Add the root to the queue
            Queue<IUMLData> queue = new Queue<IUMLData>();
            queue.Enqueue(document.RootPackage);

            // Process until empty
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                var asPackage = current as IUMLPackage;
                if (asPackage != null)
                {
                    ProcessList(queue, asPackage.OwnedElements);
                    continue;
                }
            }

            document = null;
            umodelApp = null;

            ProgressMax = Repository.Count.Value;

            foreach (var item in Repository)
                yield return item;
        }

        public IEnumerable<GenericLink<IUMLData>> EnumerateLinks()
        {
            foreach(var current in Repository)
            {
                var asClass = current as IUMLClassifier;
                if (asClass != null) {
                    var list = asClass.Generalizations;
                    for (int i = 1; i <= list.Count; i++)
                    {
                        var asGeneralized = list[i] as IUMLGeneralization;
                        if( asGeneralized != null )
                            yield return new GenericLink<IUMLData> { Source = asGeneralized.Specific, Target = asGeneralized.General, LinkType = LinkType_Inherits };
                    }
                    
                    var members = asClass.Members;
                    for (int i = 1; i <= members.Count; i++)
                    {
                        var asProperty = members[i] as IUMLProperty;
                        if (asProperty != null)
                            yield return new GenericLink<IUMLData> { Source = asClass, Target = asProperty.Type, Name = asProperty.Name, LinkType = LinkType_Properties };

                        var asMethod = members[i] as IUMLOperation;
                        if (asMethod != null) 
                        { 
                            // 'return' is counted as a parameter
                            var parameters = asMethod.Members;
                            for (int p = 1; p <= parameters.Count; p++)
                            {
                                var asParam = parameters[p] as IUMLParameter;
                                if( asParam != null )
                                    yield return new GenericLink<IUMLData> { Source = asClass, Target = asParam.Type, Name = asParam.Name, Context = asMethod.Name, LinkType = LinkType_MethodParameters };
                            }
                        }
                    }
                }

                //  var interfaces = asClass.InterfaceRealizations;
            }
        }
        #endregion

        #region Helper Methods
        void ProcessList(Queue<IUMLData> queue, UMLDataList list)
        {
            IUMLData node;
            for (int i = 1; i <= list.Count; i++)
            {
                node = list[i];
                if (IsContainerType(node))
                    queue.Enqueue(node);

                if (IsFilteredType(node) && !Repository.Has(node))
                    Repository.Add(node);

                node = null;
            }
        }

        static bool IsContainerType(IUMLData node)
        {
            var asPackage = node as IUMLPackage;
            if (asPackage != null)
                return true;

            return false;
        }

        static bool IsFilteredType(IUMLData node)
        {
            var asClassifier = node as IUMLClassifier;
            if (asClassifier == null)
                return false;

            switch( asClassifier.KindName )
            {
                case "Actor":
                case "Artifact":
                case "Class":
                case "Component":
                case "Device":
                case "Enumeration":
                case "ExecutionEnvironment":
                case "Interface":
                case "Node":
                    return true;
            }

            return false;
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (repository != null) repository.Dispose();
            }
        }

        ~UModelExtractor()
        {
            Dispose(false);
        }
        #endregion
    }
}
