using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Lynx.Interfaces;
using Lynx.Models;

namespace Lynx.XsdExchange
{
    public class XsdExtractor : IFileIterator<XElement>, IDisposable
    {
        #region internal constants
        internal readonly XNamespace xs = "http://www.w3.org/2001/XMLSchema";
        #endregion

        #region Private Properties
        public XsdRepository Repository
        {
            get
            {
                return repository ?? (repository = new XsdRepository());
            }
        }
        XsdRepository repository;
        #endregion

        #region Public Methods
        public int ProgressMax
        {
            get;
            set;
        }

        public IList<string> ValidElementTypes
        {
            get
            {
                if (validElementTypes == null)
                {
                    validElementTypes = new string[] { "attribute", "complexContent", "complexType", "element", "simpleType" };
                }
                return validElementTypes;
            }
        }
        IList<string> validElementTypes;

        XElement Primitives
        {
            get 
            {
                if (primitives == null)
                {
                    primitives = new XElement(xs + "Root",
                                     new XAttribute(XNamespace.Xmlns + "xs", xs.NamespaceName),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "anyType")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "anyUri")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "base64Binary")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "boolean")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "date")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "dateTime")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "decimal")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "double")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "duration")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "float")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "gDay")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "gMonth")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "gMonthDay")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "gYear")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "gYearMonth")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "hexBinary")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "ID")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "int")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "integer")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "language")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "long")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "Name")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "NCName")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "negativeInteger")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "NMToken")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "nonNegativeInteger")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "nonPositiveInteger")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "normalizedString")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "positiveInteger")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "QName")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "short")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "string")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "time")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "token")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "unsignedByte")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "unsignedInt")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "unsignedLong")),
                                     new XElement(xs + "simpleType", new XAttribute("name", xs + "unsignedShort"))
                                             );
                }

                return primitives;
            }
        }
        XElement primitives;
        #endregion

        #region IFileIterator<XElement> Members
        public FileInfo Source
        {
            get;
            set;
        }

        public IEnumerable<XElement> EnumerateEntities()
        {
            // Return the primitives
            foreach (var primitive in Primitives.Descendants())
            {
                XAttribute attr = primitive.Attribute("name");
                if (attr != null)
                    Repository.Set(primitive, attr.Value);
            }

            var root = XElement.Load(Source.FullName);

            // Return the definitions from this schema
            var nodes = from n in root.Descendants()
                        where ValidElementTypes.Contains(n.Name.LocalName)
                        select n;

            foreach (var node in nodes)
            {
                XAttribute attr = node.Attribute("name");
                if (attr != null)
                    Repository.Set(node, attr.Value);
            }

            ProgressMax = Repository.Count.Value;

            foreach (var item in Repository)
                yield return item;
        }

        public IEnumerable<GenericLink<XElement>> EnumerateLinks()
        {
            foreach (var node in Repository)
            {
                XAttribute attr = node.Attribute("name");
                if (attr != null)
                {
                    if (node.Name.LocalName == "simpleType")
                    {
                        var restriction = node.Element(xs + "restriction");
                        if (restriction != null)
                        {
                            XAttribute b = restriction.Attribute("base");
                            if (b != null)
                            {
                                var baseType = Repository.Get(b.Value.Replace("xs:", XsdRepository.xsAsXNamePrefix));
                                if( baseType != null )
                                    yield return new GenericLink<XElement> { Source = node, Target = baseType, LinkType = "IsA", Context="Restriction" };
                            }
                        }
                    }

                    else if (node.Name.LocalName == "complexType")
                    {
                        var sequence = node.Element(xs + "sequence");
                        foreach (var sub in HandleSequence(node, sequence))
                            yield return sub;
                    }

                    else if (node.Name.LocalName == "complexContent")
                    {
                    }

                    else if (node.Name.LocalName == "element")
                    {
                        XAttribute t = node.Attribute("type");
                        if (t != null)
                        {
                            var baseType = Repository.Get(t.Value.Replace("xs:", XsdRepository.xsAsXNamePrefix));
                            if (baseType != null)
                                yield return new GenericLink<XElement> { Source = node, Target = baseType, LinkType = "IsA", Context = "Type" };
                        }

                        XElement complexType = node.Element(xs + "complexType");
                        if (complexType != null)
                        {
                            var sequence = complexType.Element(xs + "sequence");
                            foreach (var sub in HandleSequence(node, sequence))
                                yield return sub;
                        }
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        IEnumerable<GenericLink<XElement>> HandleSequence(XElement parent, XElement sequence)
        {
            if (sequence != null)
            {
                foreach (var subItem in sequence.Elements().Where(n => ValidElementTypes.Contains(n.Name.LocalName)))
                {
                    var child = Repository.Get(subItem);
                    if (child != null)
                        yield return new GenericLink<XElement> { Source = child, Target = parent, LinkType = "OwnedBy", Context = "Sequence" };
                }
            }
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
                Repository.Dispose();
            }
        }

        ~XsdExtractor()
        {
            Dispose(false);
        }
        #endregion
    }
}
