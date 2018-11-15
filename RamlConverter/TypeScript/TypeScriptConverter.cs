using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace RamlConverter.TypeScript
{
    public class TypeScriptConverter : ConverterBase<TypeScriptSchema, TypeScriptType>
    {
        public TypeScriptConverter(RamlFile file) : base(file) { }

        #region Implementatin of abstract methods from the base class

        protected override void AddSchemaTypeToSchema(TypeScriptSchema schema, TypeScriptType schemaType)
        {
            if (schemaType == null)
                return;

            if (schema.Types == null)
            {
                schema.Types = new List<TypeScriptType>();
            }

            schema.Types.Add(schemaType);
        }

        protected override void InitializeSchema(TypeScriptSchema schema, ConversionOptions options)
        {
            if (options.IndentSize.HasValue)
            {
                schema.IndentSize = options.IndentSize;
            }
            schema.DisableTSLine = options.DisableTSLint;
        }

        protected override void FinalizeSchema(TypeScriptSchema schema, ConversionOptions options)
        {
        }

        protected override void ProcessRootType(RamlType ramlType, ConversionOptions options)
        {
        }

        protected override TypeScriptType RamlTypeToSchemaType(RamlType ramlType, ConversionOptions options)
        {
            if (ramlType == null)
                return null;

            var typeScriptType = new TypeScriptType();
            typeScriptType.Name = ramlType.Name;

            // check if it is enum
            if (ramlType.Enum != null)
            {
                typeScriptType.Enum = new TypeScriptEnum();

                var enumTypeName = RamlDataTypeToTypeScriptDataType(ramlType.Enum.ItemsTypeName);

                if (enumTypeName == null)
                {
                    enumTypeName = GetRefDataType(ramlType.Enum.ItemsTypeName);
                }

                typeScriptType.Enum.EnumValues = new List<string>();

                foreach (string enumValue in ramlType.Enum.EnumValues)
                {
                    typeScriptType.Enum.EnumValues.Add(enumValue);
                }

                return typeScriptType;
            }

            // check if it is array
            if (ramlType.Array != null)
            {
                typeScriptType.Array = new TypeScriptArray();


                typeScriptType.Array.ItemName = ramlType.Array.ItemName;


                var arrayElementTypeName = RamlDataTypeToTypeScriptDataType(ramlType.Array.ItemsTypeName);

                if (arrayElementTypeName == null)
                {
                    arrayElementTypeName = ramlType.Array.ItemsTypeName;
                }

                if (arrayElementTypeName != null)
                {
                    typeScriptType.Array.ItemsTypeName = arrayElementTypeName;
                }

                return typeScriptType;
            }

            if (ramlType.Description != null && options.GenerateDescriptions)
            {
                typeScriptType.Comment = ramlType.Description;
            }

            if (ramlType.Properties == null || ramlType.Properties.Count == 0)
            {
                return typeScriptType;
            }

            typeScriptType.Properties = new List<TypeScriptProperty>();

            foreach (RamlProperty ramlProperty in ramlType.Properties)
            {
                var typeScriptProperty = new TypeScriptProperty();
                typeScriptProperty.Name = ramlProperty.Name;

                var typeName = RamlDataTypeToTypeScriptDataType(ramlProperty.Type);

                if (typeName != null)
                {
                    typeScriptProperty.Type = typeName;
                }
                else
                {
                    typeScriptProperty.Type = ramlProperty.Type;
                }

                typeScriptProperty.ArrayItemName = ramlProperty.ArrayItemName;

                if (ramlProperty.Description != null && options.GenerateDescriptions)
                {
                    typeScriptProperty.Comment = ramlProperty.Description;
                }

                typeScriptProperty.Required = ramlProperty.Required;

                typeScriptType.Properties.Add(typeScriptProperty);
            }

            return typeScriptType;
        }

        protected override void AddExternalReferences(TypeScriptSchema schema)
        {
            if (_ramlFile.Uses != null)
            {
                schema.Imports = new Dictionary<string, string>();
            }

            foreach (string key in _ramlFile.Uses.Keys)
            {
                schema.Imports.Add(key, Path.GetFileNameWithoutExtension(_ramlFile.Uses[key] as string));
            }
        }

        protected override void WriteSchemaFile(TypeScriptSchema schema, ConversionOptions opitons)
        {
            var fileName = Path.GetFileNameWithoutExtension(_ramlFile.FullPath) + "." + FileExtensions.TypeScript;

            string fullPath = Path.Combine(opitons.OutputDirectory, fileName);

            using (var streamWriter = new StreamWriter(fullPath))
            {
                schema.Write(streamWriter);
            }
        }
        #endregion

        private string RamlDataTypeToTypeScriptDataType(string ramlDataType)
        {
            switch (ramlDataType)
            {
                case RamlDataTypes.Boolean:
                    return TypeScriptDataTypes.Boolean;
                case RamlDataTypes.DateOnly:
                    return TypeScriptDataTypes.String;
                case RamlDataTypes.DateTime:
                    return TypeScriptDataTypes.String;
                case RamlDataTypes.DateTimeOnly:
                    return TypeScriptDataTypes.String;
                case RamlDataTypes.Integer:
                    return TypeScriptDataTypes.Number;
                case RamlDataTypes.Nil:
                    return TypeScriptDataTypes.Nil;
                case RamlDataTypes.Number:
                    return TypeScriptDataTypes.Number;
                case RamlDataTypes.String:
                    return TypeScriptDataTypes.String;
                case RamlDataTypes.TimeOnly:
                    return TypeScriptDataTypes.String;
                case null:
                    return TypeScriptDataTypes.String;
                default:
                    return null;
            }
        }

    }   
        
}
