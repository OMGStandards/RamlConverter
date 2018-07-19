using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace RamlConverter.Xml
{
    public class JsonConverter : ConverterBase<JSchema, JSchema>
    {
        private JObject _jsonObject = new JObject();

        public JsonConverter(RamlFile file) : base(file) { }

        #region Implementatin of abstract methods from the base class

        protected override void AddSchemaTypeToSchema(JSchema schema, JSchema schemaType)
        {
            this._jsonObject.Add(schemaType.Title, schemaType);
        }

        protected override void InitializeSchema(JSchema schema, ConversionOptions options)
        {
            if (options.GenerateDescriptions)
            {
                schema.Title = this._ramlFile.Usage;
            }
        }

        protected override void FinalizeSchema(JSchema schema, ConversionOptions options)
        {
           schema.ExtensionData.Add(StringConstants.Definitions, this._jsonObject);
        }

        protected override void ProcessRootType(RamlType ramlType, ConversionOptions options)
        {
            JSchema jsonSchema = new JSchema();
            var refFileName = Path.GetFileNameWithoutExtension(_ramlFile.FullPath) + "." + FileExtensions.JsonSchema;
            var refDatatype = refFileName + @"#/definitions/" + ramlType.Name;

            jsonSchema.ExtensionData.Add("$ref", refDatatype);

            var fileName = System.Char.ToLowerInvariant(ramlType.Name[0]) + ramlType.Name.Substring(1) +
                "." + FileExtensions.JsonSchema;
            
            string fullPath = Path.Combine(options.OutputDirectory, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonSchema.WriteTo(jsonWriter);
            }
        }

        protected override JSchema RamlTypeToSchemaType(RamlType ramlType, ConversionOptions options)
        {
            if (ramlType == null)
                return null;

            JSchema jsonSchema = new JSchema();

            jsonSchema.Title = ramlType.Name;

            if (ramlType.AdditionalProperties)
            {
                jsonSchema.AllowAdditionalProperties = true;
            }
            else
            {
                jsonSchema.AllowAdditionalProperties = false;
            }

            if (options.GenerateDescriptions)
            {
                jsonSchema.Description = ramlType.Description;
            }

            // check if it is enum
            if (ramlType.Enum != null)
            {

                var jsonDataType = RamlDataTypeToJsonSchemaDataType(ramlType.Enum.ItemsTypeName);

                if (jsonDataType == JSchemaType.None)
                {
                    var refDataType = GetJsonRefDataType(ramlType.Enum.ItemsTypeName);
                    jsonSchema.ExtensionData.Add("$ref", refDataType);
                }
                else
                {
                    jsonSchema.Type = jsonDataType;
                }

                foreach (string enumValue in ramlType.Enum.EnumValues)
                {
                    jsonSchema.Enum.Add(JToken.FromObject(enumValue));
                }

                return jsonSchema;
            }

            // check if it is array
            if (ramlType.Array != null)
            {
                jsonSchema.Type = JSchemaType.Array;

                var jsonSchemaItems = new JSchema();

                var jsonDataType = RamlDataTypeToJsonSchemaDataType(ramlType.Array.ItemsTypeName);

                if (jsonDataType == JSchemaType.None)
                {
                    var refDataType = GetJsonRefDataType(ramlType.Array.ItemsTypeName);
                    jsonSchemaItems.ExtensionData.Add("$ref", refDataType);
                }
                else
                {
                    jsonSchemaItems.Type = jsonDataType;
                }

                jsonSchema.Items.Add(jsonSchemaItems);

                return jsonSchema;
            }

            // check if it has base
            if (ramlType.Base != null)
            {
                var jsonDataType = RamlDataTypeToJsonSchemaDataType(ramlType.Base.Name);

                if (jsonDataType == JSchemaType.None)
                {
                    var refDataType = GetJsonRefDataType(ramlType.Enum.ItemsTypeName);
                    jsonSchema.ExtensionData.Add("$ref", refDataType);
                }
                else
                {
                    jsonSchema.Type = jsonDataType;
                }
                
                if (!string.IsNullOrEmpty(ramlType.Base.Pattern))
                {
                    jsonSchema.Pattern = ramlType.Base.Pattern;                    
                }

                return jsonSchema;
            }

            jsonSchema.Type = JSchemaType.Object;

            if (ramlType.Properties == null || ramlType.Properties.Count == 0)
            {
                return jsonSchema;
            }

            jsonSchema.MinimumProperties = ramlType.MinProperties;
            jsonSchema.MaximumProperties = ramlType.MaxProperties;

            foreach (RamlProperty ramlProperty in ramlType.Properties)
            {
                var jsonSchemaProperty = new JSchema();

                var jsonDataType = RamlDataTypeToJsonSchemaDataType(ramlProperty.Type);

                if (jsonDataType == JSchemaType.None)
                {
                    var refDataType = GetJsonRefDataType(ramlProperty.Type);
                    jsonSchemaProperty.ExtensionData.Add("$ref", refDataType);
                }
                else
                {
                    jsonSchemaProperty.Type = jsonDataType;
                }

                if (options.GenerateDescriptions)
                {
                    jsonSchemaProperty.Description = ramlProperty.Description;
                }

                jsonSchema.Properties.Add(ramlProperty.Name, jsonSchemaProperty);
                if (ramlProperty.Required)
                {
                    jsonSchema.Required.Add(ramlProperty.Name);
                }

            }

            return jsonSchema;
        }

        protected override void AddExternalReferences(JSchema schema)
        {
        }

        protected override void WriteSchemaFile(JSchema schema, ConversionOptions options)
        {
            var fileName = Path.GetFileNameWithoutExtension(_ramlFile.FullPath) + "." + FileExtensions.JsonSchema;

            string fullPath = Path.Combine(options.OutputDirectory, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                schema.WriteTo(jsonWriter);
            }
        }

        #endregion

        private string GetJsonRefDataType(string ramlType)
        {
            string refDatatype = null;

            char[] dotDelimiter = { '.' };

            var referenceAndTypeName = ramlType.Split(dotDelimiter, StringSplitOptions.RemoveEmptyEntries);

            string fileNameReference = null;
            string typeName = null;

            if (referenceAndTypeName.Length == 2)
            {
                fileNameReference = referenceAndTypeName[0];
                typeName = referenceAndTypeName[1];
            }
            else if (referenceAndTypeName.Length == 1) // referencing local type (no file prefix)
            {
                typeName = referenceAndTypeName[0];
            }

            string jsonFileName = "";

            if (fileNameReference != null)
            {
                string fileName = GetValueByKey(_ramlFile.Uses, fileNameReference) as string;
                if (fileName != null)
                {
                    jsonFileName = ChangeFileNameExtension(fileName, "json");
                }

            }

            if (jsonFileName != null)
            {
                refDatatype = jsonFileName + @"#/definitions/" + typeName;
            }

            return refDatatype;
        }

        private JSchemaType RamlDataTypeToJsonSchemaDataType(string ramlDataType)
        {
            switch (ramlDataType)
            {
                case RamlDataTypes.Boolean:
                    return JSchemaType.Boolean;
                case RamlDataTypes.DateOnly:
                    return JSchemaType.String;
                case RamlDataTypes.DateTime:
                    return JSchemaType.String;
                case RamlDataTypes.DateTimeOnly:
                    return JSchemaType.String;
                case RamlDataTypes.Integer:
                    return JSchemaType.Integer;
                case RamlDataTypes.Nil:
                    return JSchemaType.Null;
                case RamlDataTypes.Number:
                    return JSchemaType.Number;
                case RamlDataTypes.String:
                    return JSchemaType.String;
                case RamlDataTypes.TimeOnly:
                    return JSchemaType.String;
                // in RAML default type is string
                case null:
                    return JSchemaType.String;
                default:
                    return JSchemaType.None;
            }
        }

        private object GetValueByKey(Dictionary<object, object> dictionary, string key)
        {
            if (dictionary == null)
                return null;

            object value;

            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        
    }
}
