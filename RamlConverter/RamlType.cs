using System;
using System.Collections.Generic;
using System.Text;

namespace RamlConverter
{
    public class RamlType
    {
        public RamlType ()
        {
            this.IsRootType = false;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RamlProperty> Properties { get; set; }
        public RamlBase Base { get; set; }  
        public bool AdditionalProperties { get; set; }
        public RamlArray Array { get; set; }
        public RamlEnum Enum { get; set; }

        public int? MinProperties { get; set; }
        public int? MaxProperties { get; set; }
        public bool IsRootType { get; set; }
    }

    public class RamlArray
    {
        public string ItemsTypeName { get; set; }
        public string ItemName { get; set; }
        public int minItems { get; set; }
        public int maxItems { get; set; }
    }

    public class RamlEnum
    {
        public string ItemsTypeName { get; set; }

        public List<string> EnumValues { get; set; }

    }

    public class RamlBase
    {
        public string Name { get; set; }
        public string Pattern { get; set; }      
    }
}
