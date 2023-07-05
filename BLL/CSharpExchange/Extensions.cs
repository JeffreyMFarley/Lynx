using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace Lynx.CSharpExchange
{
    static public class Extensions
    {
        static public string DisplayName(this Type type)
        {
            var result = new StringBuilder(type.Name);

            if (type.Name.Contains("Attribute") )
            {
                var t = result.Capacity;
            }

            if (type.IsGenericType)
            {
                var prefix = "<";
                foreach (var argument in type.GetGenericArguments())
                {
                    result.Append(prefix);
                    if (argument.IsGenericParameter || argument.IsGenericType)
                        result.Append(argument.DisplayName());
                    else
                        result.Append(argument.Name);
                    prefix = ",";
                }
                result.Append(">");
            }

            if (type.IsGenericParameter)
            {
                var prefix = " : ";
                foreach (var constraint in type.GetGenericParameterConstraints())
                {
                    result.Append(prefix);
                    result.Append(constraint.Name);
                    prefix = ",";
                }
            }

            return result.ToString();
        }
    }
}
