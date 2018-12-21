using System.Collections.Generic;

namespace RamlConverter.TypeScript
{
    public class TypeScriptType
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public List<TypeScriptProperty> Properties { get; set; }
        public List<string> BaseTypeName { get; set; }  // no support for multiple inheritance       
        public TypeScriptArray Array { get; set; }
        public TypeScriptEnum Enum { get; set; }
        public bool IsRootType { get; set; }
    }
}