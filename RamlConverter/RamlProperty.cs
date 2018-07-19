using System;
using System.Collections.Generic;
using System.Text;

namespace RamlConverter
{
    public class RamlProperty
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string ArrayItemName { get; set; }
        public string Default { get; set; }
        public bool Required { get; set; }
    }
}
