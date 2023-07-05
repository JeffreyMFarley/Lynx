using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lynx.Models
{
    public class GenericLink<T>
    {
        public T Source { get; set; }
        public T Target { get; set; }

        public string LinkType { get; set; }

        public string Name { get; set; }

        public string Context { get; set; }
    }
}
