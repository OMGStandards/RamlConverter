using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RamlConverter.CSharp
{
    public class CSharpSchema
    {
        public List<string> Usings { get; set; }
        public string CSharpNamespace { get; set; }
        public string XmlNamespace { get; set; }
        public List<CSharpType> Types { get; set; }

        public void Write (StreamWriter writer)
        {
            WriteUsings(writer);

            WriteNamespaceOpen(writer);

            if(this.Types != null)
            {
                WriteTypes(writer);
            }

            WriteNamespaceClose(writer);
        }

        private void WriteTypes(StreamWriter writer)
        {
            if (this.Types == null)
                return;

            if (this.Types.Count == 0)
                return;

            foreach(CSharpType type in this.Types)
            {
                WriteType(writer, type);
            }
        }

        private void WriteType(StreamWriter writer, CSharpType type)
        {
            if (!string.IsNullOrEmpty(type.Comment))
            {
                WriteTypeComment(writer, type.Comment);
            }

            if (type.Array != null)
            {
                WriteArrayTypeHeader(writer, type);
            } 
            else if (type.Enum != null)
            {
                WriteEnumTypeHeader(writer, type);
                if (type.Enum.EnumValues != null)
                {
                    for (int i = 0; i < type.Enum.EnumValues.Count; i++)
                    {
                        bool last = i == type.Enum.EnumValues.Count - 1;
                        var enumMember = type.Enum.EnumValues[i];
                        WriteEnumMember(writer, enumMember, last);
                    }
                }

            }
            else
            {
                WriteTypeHeader(writer, type);

                if (type.Properties != null)
                {
                    for (int i = 0; i < type.Properties.Count; i++)
                    {
                        var property = type.Properties[i];
                        WriteProperty(writer, property, i);
                    }
                }          
            }           

            WriteTypeFooter(writer);
        }

        private void WriteEnumTypeHeader(StreamWriter writer, CSharpType type)
        {
            writer.WriteLine(Tab(1) + "[DataContract(Namespace = \"{0}\")]", this.XmlNamespace);
            writer.WriteLine(Tab(1) + "[JsonConverter(typeof(StringEnumConverter))]");            
            writer.WriteLine(Tab(1) + "public enum {0}", type.Name);
            writer.WriteLine(Tab(1) + "{");
        }

        private void WriteEnumMember(StreamWriter writer, string enumMember, bool last)
        {
            string endOfEnum = last ? "" : ",";
            writer.WriteLine(Tab(2) + "[EnumMember(Value = \"{0}\")]", enumMember);
            writer.WriteLine(Tab(2) + "{0}{1}", enumMember, endOfEnum);
        }

        private void WriteArrayTypeHeader(StreamWriter writer, CSharpType type)
        {
            writer.WriteLine(Tab(1) + "[CollectionDataContract(Namespace = \"{0}\", ItemName = \"{1}\")]", this.XmlNamespace, type.Array.ItemName);
            writer.WriteLine(Tab(1) + "public class {0}", type.Name);
            writer.WriteLine(Tab(1) + "{");
        }

        private void WriteProperty(StreamWriter writer, CSharpProperty property, int propertyOrder)
        {
            if (!string.IsNullOrEmpty(property.Comment))
            {
                WritePropertyComment(writer, property.Comment);
            }

            var optionalDataType =
                (
                    property.Type == CSharpDataTypes.Boolean ||
                    property.Type == CSharpDataTypes.DateTime ||
                    property.Type == CSharpDataTypes.Decimal ||
                    property.Type == CSharpDataTypes.Integer
                ) ? property.Type + "?" : property.Type;

            if (property.Required)
            {
                writer.WriteLine(Tab(2) + "[DataMember(IsRequired = true, Order = {0})]", propertyOrder);
                writer.WriteLine(Tab(2) + "[JsonProperty(Required = Required.Always, Order = {0})]", propertyOrder);
                writer.WriteLine(Tab(2) + "public {0} {1} {{ get; set; }}", property.Type, property.Name);
            }
            else
            {
                writer.WriteLine(Tab(2) + "[DataMember(EmitDefaultValue = false, Order = {0})]", propertyOrder);
                writer.WriteLine(Tab(2) + "[JsonProperty(NullValueHandling = NullValueHandling.Ignore, Order = {0})]", propertyOrder);
                writer.WriteLine(Tab(2) + "public {0} {1} {{ get; set; }}", optionalDataType, property.Name);
            }
            writer.WriteLine();
        }

        private void WritePropertyComment(StreamWriter writer, string comment)
        {
            writer.WriteLine(Tab(2) + "/// <summary>");
            var commentLines = SplitComment(comment);
            foreach (string commentLine in commentLines)
            {
                writer.WriteLine(Tab(2) + "/// {0}", commentLine);
            }
            writer.WriteLine(Tab(2) + "/// </summary>");
        }

        private void WriteTypeFooter(StreamWriter writer)
        {
            writer.WriteLine(Tab(1) + "}");
            writer.WriteLine();
        }

        private void WriteTypeHeader(StreamWriter writer, CSharpType type)
        {
            writer.WriteLine(Tab(1) + "[DataContract(Namespace = \"{0}\")]", this.XmlNamespace);
            writer.WriteLine(Tab(1) + "public class {0}", type.Name);
            writer.WriteLine(Tab(1) + "{");            
        }

        private void WriteTypeComment(StreamWriter writer, string comment)
        {
            writer.WriteLine(Tab(1) + "/// <summary>");
            var commentLines = SplitComment(comment);
            foreach(string commentLine in commentLines)
            {
                writer.WriteLine(Tab(1) + "/// " + commentLine);
            }           
            writer.WriteLine(Tab(1) + "/// </summary>");
        }

        private string[] SplitComment(string comment)
        {
            char[] slashDelimiter = { '\n' };

            return comment.Split(slashDelimiter, StringSplitOptions.RemoveEmptyEntries);

        
        }

        private void WriteNamespaceOpen(StreamWriter writer)
        {
            if (string.IsNullOrEmpty(this.CSharpNamespace))
                return;

            writer.WriteLine("namespace {0}", this.CSharpNamespace);
            writer.WriteLine("{");
        }

        private void WriteNamespaceClose(StreamWriter writer)
        {
            writer.WriteLine("}");
        }

        private void WriteUsings(StreamWriter writer)
        {
            if (Usings == null)
                return;

            if (Usings.Count == 0)
                return;

            foreach(string usingString in Usings)
            {
                writer.WriteLine("using {0};", usingString);

            }
            writer.WriteLine();
        }

        private string Tab(int numberOfTabs)
        {
            return new string('\t', numberOfTabs);
        }

    }
}
