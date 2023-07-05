using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.CSharpExchange
{
    [Export]
    public class TypeExtractor : IFileIterator<Type>
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

        #region Public Properties
        public BindingFlags DefaultBindingFlags
        {
            get
            {
                return bindingFlags;
            }
            set
            {
                bindingFlags = value;
            }
        }
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        #endregion

        #region IFileIterator<Type> Members
        public FileInfo Source
        {
            get;
            set;
        }

        public IEnumerable<Type> EnumerateEntities()
        {
            var set = new Dictionary<string, Type>();
            foreach (GenericLink<Type> link in EnumerateLinks())
            {
                UpdateSet(set, link.Source);
                UpdateSet(set, link.Target);
            }

            return set.Values.AsEnumerable();
        }

        public IEnumerable<GenericLink<Type>> EnumerateLinks()
        {
            var assem = Assembly.LoadFrom(Source.FullName);
            Type targetType;

            foreach (var type in assem.GetTypes())
            {
                // Filter out noise
                if (type.Name.StartsWith("<"))
                    continue;

                if( type.BaseType != null )
                    yield return new GenericLink<Type> { Source = type, Target = type.BaseType, LinkType = LinkType_Inherits };

                // MEF fields
                foreach (var export in type.GetCustomAttributes(typeof(ExportAttribute), false).Cast<ExportAttribute>())
                {
                    targetType = export.ContractType ?? type;
                    yield return new GenericLink<Type> { Source = type, Target = targetType, LinkType = LinkType_Exports, Name = export.ContractName };
                }

                foreach (var field in type.GetFields(DefaultBindingFlags))
                {
                    if (!field.Name.Contains("BackingField"))
                    {
                        yield return new GenericLink<Type> { Source = type, Target = field.FieldType, LinkType = LinkType_Fields, Name = field.Name };

                        foreach (var import in field.GetCustomAttributes(typeof(ImportAttribute), false).Cast<ImportAttribute>())
                        {
                            targetType = import.ContractType ?? field.FieldType ?? type;
                            yield return new GenericLink<Type> { Source = type, Target = targetType, LinkType = LinkType_Imports, Name = field.Name };
                        }
                    }
                    else
                    {
                        var o = field.Name;
                    }
                }

                if (type.IsGenericType)
                {
                    if (type.IsGenericTypeDefinition)
                        yield return new GenericLink<Type> { Source = type, Target = type.GetGenericTypeDefinition(), LinkType = LinkType_GenericTypeDefinitions };

                    foreach (var genericArg in type.GetGenericArguments())
                    {
                        yield return new GenericLink<Type> { Source = type, Target = genericArg, LinkType = LinkType_GenericParameters };

                        if (genericArg.IsGenericParameter && genericArg.IsGenericType)
                        {
                            foreach (var genericConstraint in type.GetGenericParameterConstraints())
                                yield return new GenericLink<Type> { Source = genericArg, Target = genericConstraint, LinkType = LinkType_GenericConstraints, Context = type.Name };
                        }
                    }
                }

                foreach(var iface in type.GetInterfaces() )
                    yield return new GenericLink<Type> { Source = type, Target = iface, LinkType = LinkType_Implements };

                foreach (var method in type.GetConstructors(DefaultBindingFlags))
                {
                    foreach (var parameter in method.GetParameters())
                        yield return new GenericLink<Type> { Source = type, Target = parameter.ParameterType, LinkType = LinkType_MethodParameters, Context = "ctor", Name = parameter.Name };

                    foreach (var import in method.GetCustomAttributes(typeof(ImportingConstructorAttribute), false).Cast<ImportingConstructorAttribute>())
                    {
                        foreach (var parameter in method.GetParameters())
                        {
                            if (parameter.GetCustomAttributes(typeof(ImportAttribute), false).Any())
                            {
                                targetType = parameter.ParameterType ?? type;
                                yield return new GenericLink<Type> { Source = type, Target = targetType, LinkType = LinkType_Imports, Context = "ctor", Name = parameter.Name };
                            }
                        }
                    }
                }

                foreach (var method in type.GetMethods(DefaultBindingFlags))
                {
                    if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_"))
                    {
                        if (method.ReturnType != null)
                            yield return new GenericLink<Type> { Source = type, Target = method.ReturnType, LinkType = LinkType_MethodReturns, Name = method.Name };

                        foreach (var parameter in method.GetParameters())
                            yield return new GenericLink<Type> { Source = type, Target = parameter.ParameterType, LinkType = LinkType_MethodParameters, Context = method.Name, Name = parameter.Name };

                        foreach (var export in method.GetCustomAttributes(typeof(ExportAttribute), false).Cast<ExportAttribute>())
                        {
                            targetType = export.ContractType ?? method.ReturnType ?? type;
                            yield return new GenericLink<Type> { Source = type, Target = targetType, LinkType = LinkType_Exports, Name = method.Name };
                        }
                        
                    }
                }

                foreach (var property in type.GetProperties(DefaultBindingFlags))
                {
                    yield return new GenericLink<Type> { Source = type, Target = property.PropertyType, LinkType = LinkType_Properties, Name = property.Name };

                    foreach (var import in property.GetCustomAttributes(typeof(ImportAttribute), false).Cast<ImportAttribute>())
                    {
                        targetType = import.ContractType ?? property.PropertyType ?? type;
                        yield return new GenericLink<Type> { Source = type, Target = targetType, LinkType = LinkType_Imports, Name = property.Name };
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        static void UpdateSet(Dictionary<string, Type> set, Type candidate)
        {
            // Bouncer code
            if (candidate == null)
                return;

            if (candidate.Name == "LookupConstant`2")
            {
                var o = candidate.BaseType;
            }

            var name = candidate.FullName ?? candidate.DisplayName();

            if (!set.ContainsKey(name))
                set.Add(name, candidate);

            // This is a generic type implementation, add the details
            if (candidate.IsGenericType)
            {
                foreach (var argument in candidate.GetGenericArguments())
                {
                    if( argument.IsGenericParameter && !argument.BaseType.Equals(candidate))
                        UpdateSet(set, argument.BaseType);
                    else
                        UpdateSet(set, argument);
                }
            }
        }
        #endregion
    }
}
