using System;
using System.Collections.Generic;
using System.Text;

namespace RamlConverter
{
    public class AppConfig
    {
        public string InputDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public string InputFileName { get; set; }
        public bool? GenerateJsonSchema { get; set; }
        public bool? GenerateXmlSchema { get; set; }
        public bool? GenerateTypeScriptCode { get; set; }
        public bool? GenerateCSharpCode { get; set; }
        public bool? GenerateDescriptions { get; set; }
        public List<string> RootTypes { get; set; }
    }

    public class XmlSchemaConfig
    {
        public string OutputDirectory { get; set; }
        public string Namespace { get; set; }
    }

    public class JsonSchemaConfig
    {
        public string OutputDirectory { get; set; }
    }

    public class CSharpConfig
    {
        public string OutputDirectory { get; set; }
        public string Namespace { get; set; }
    }

    public class TypeScriptConfig
    {
        public string OutputDirectory { get; set; }
    }

}
