using System.Collections.Generic;

namespace RamlConverter.CSharp
{
    public class CSharpType
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public List<CSharpProperty> Properties { get; set; }
        public List<string> BaseTypeName { get; set; }  // no support for multiple inheritance       
        public CSharpArray Array { get; set; }
        public CSharpEnum Enum { get; set; }        
    }
}