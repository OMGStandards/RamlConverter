using System;
using System.Collections.Generic;
using System.Text;

namespace RamlConverter
{
    public class ConversionOptions
    {
        public string XmlNamespace { get; set; }
        public string CSharpNamespace { get; set; }
        public bool GenerateDescriptions { get; set; }   
        public string OutputDirectory { get; set; }
        public int? IndentSize { get; set; }
        public bool DisableTSLint { get; set; }
    }
}
