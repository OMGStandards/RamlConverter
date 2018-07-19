using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace RamlConverter.CSharp
{
    public class CSharpConverter : ConverterBase<CSharpSchema, CSharpType>
    {
        public CSharpConverter(RamlFile file) : base(file) { }

        #region Implementatin of abstract methods from the base class

        protected override void AddSchemaTypeToSchema(CSharpSchema schema, CSharpType schemaType)
        {
            if (schemaType == null)
                return;

            if (schema.Types == null)
            {
                schema.Types = new List<CSharpType>();
            }

            schema.Types.Add(schemaType);
        }

        protected override void InitializeSchema(CSharpSchema schema, ConversionOptions options)
        {
            schema.Usings = new List<string>();
            schema.Usings.Add("System");
            schema.Usings.Add("System.Runtime.Serialization");
            schema.Usings.Add("System.Collections.Generic");
            schema.Usings.Add("Newtonsoft.Json");
            schema.Usings.Add("Newtonsoft.Json.Converters");

            if (!string.IsNullOrEmpty(options.CSharpNamespace))
            {
                schema.CSharpNamespace = options.CSharpNamespace;
            }

            if (!string.IsNullOrEmpty(options.XmlNamespace))
            {
                schema.XmlNamespace = options.XmlNamespace;
            }
        }

        protected override void FinalizeSchema(CSharpSchema schema, ConversionOptions options)
        {
        }

        protected override void ProcessRootType(RamlType ramlType, ConversionOptions options)
        {
        }

        protected override CSharpType RamlTypeToSchemaType(RamlType ramlType, ConversionOptions options)
        {
            if (ramlType == null)
                return null;

            var cSharpType = new CSharpType();
            cSharpType.Name = ramlType.Name;

            // check if it is enum
            if (ramlType.Enum != null)
            {
                cSharpType.Enum = new CSharpEnum();

                var enumTypeName = RamlDataTypeToCSharpDataType(ramlType.Enum.ItemsTypeName);

                if (enumTypeName == null)
                {
                    enumTypeName = GetRefDataType(ramlType.Enum.ItemsTypeName);
                }

                cSharpType.Enum.EnumValues = new List<string>();

                foreach (string enumValue in ramlType.Enum.EnumValues)
                {
                    cSharpType.Enum.EnumValues.Add(enumValue);
                }

                return cSharpType;
            }

            // check if it is array
            if (ramlType.Array != null)
            {
                cSharpType.Array = new CSharpArray();

                cSharpType.Array.ItemName = ramlType.Array.ItemName;

                var arrayElementTypeName = RamlDataTypeToCSharpDataType(ramlType.Array.ItemsTypeName);

                if (arrayElementTypeName == null)
                {
                    arrayElementTypeName = GetRefDataType(ramlType.Array.ItemsTypeName);
                }

                if (arrayElementTypeName != null)
                {
                    cSharpType.Array.ItemsTypeName = arrayElementTypeName;
                }

                return cSharpType;
            }

            if (ramlType.Description != null && options.GenerateDescriptions)
            {
                cSharpType.Comment = ramlType.Description;
            }           

            if (ramlType.Properties == null || ramlType.Properties.Count == 0)
            {
                return cSharpType;
            }

            cSharpType.Properties = new List<CSharpProperty>();

            foreach (RamlProperty ramlProperty in ramlType.Properties)
            {
                var csharpProperty = new CSharpProperty();
                csharpProperty.Name = ramlProperty.Name;

                var typeName = RamlDataTypeToCSharpDataType(ramlProperty.Type);

                if (typeName == null)
                {
                    typeName = GetRefDataType(ramlProperty.Type);
                }

                if (typeName != null)
                {
                    csharpProperty.Type = typeName;
                }

                if (ramlProperty.Description != null && options.GenerateDescriptions)
                {
                    csharpProperty.Comment = ramlProperty.Description;
                }

                csharpProperty.Required = ramlProperty.Required;

                cSharpType.Properties.Add(csharpProperty);
            }

            return cSharpType;
        }

        protected override void AddExternalReferences(CSharpSchema schema)
        {
            return;
        }

        protected override void WriteSchemaFile(CSharpSchema schema, ConversionOptions opitons)
        {
            var fileName = Path.GetFileNameWithoutExtension(_ramlFile.FullPath) + "." + FileExtensions.CSharp;

            string fullPath = Path.Combine(opitons.OutputDirectory, fileName);

            using (var streamWriter = new StreamWriter(fullPath))
            {
                schema.Write(streamWriter);
            }
        }

        #endregion

        private string RamlDataTypeToCSharpDataType(string ramlDataType)
        {
            switch (ramlDataType)
            {
                case RamlDataTypes.Boolean:
                    return CSharpDataTypes.Boolean;
                case RamlDataTypes.DateOnly:
                    return CSharpDataTypes.String;
                case RamlDataTypes.DateTime:
                    return CSharpDataTypes.String;
                case RamlDataTypes.DateTimeOnly:
                    return CSharpDataTypes.String;
                case RamlDataTypes.Integer:
                    return CSharpDataTypes.Integer;
                case RamlDataTypes.Nil:
                    return CSharpDataTypes.String;
                case RamlDataTypes.Number:
                    return CSharpDataTypes.Decimal;
                case RamlDataTypes.String:
                    return CSharpDataTypes.String;
                case RamlDataTypes.TimeOnly:
                    return CSharpDataTypes.String;
                case null:
                    return CSharpDataTypes.String;
                default:
                    return null;
            }
        }

       
    }
}
