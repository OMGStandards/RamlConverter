using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace RamlConverter
{
    public class RamlFile
    {

        #region Constructors

        public RamlFile()
        {
        }

        public RamlFile(string fullPath)
        {
            this.FullPath = fullPath;      


            using (StreamReader reader = File.OpenText(fullPath))
            {
                var deserializer = new Deserializer();
                this.RamlStructure = deserializer.Deserialize(reader) as Dictionary<object, object>;

                ProcessRamlStructure();
            }
        }

        #endregion

        #region Properties

        public string FullPath { get; private set; }
        public Dictionary<object, object> RamlStructure { get; private set; }
        public string Usage { get; private set; }
        public Dictionary<object, object> Uses { get; private set; }
        public List<RamlType> Types { get; private set; }

        #endregion

        #region Private Methods

        private void ProcessRamlStructure()
        {
            if (RamlStructure == null)
                return;

            this.Usage = GetValueByKey(RamlStructure, "usage") as string;
            this.Uses = GetValueByKey(RamlStructure, "uses") as Dictionary<object, object>;

            var types = GetValueByKey(RamlStructure, "types") as Dictionary<object, object>;
            if (types != null)
            {
                this.Types = GetRamlTypes(types);
            }
        }

        private List<RamlType> GetRamlTypes(Dictionary<object, object> types)
        {
            if (types == null)
                return null;

            var ramlTypes = new List<RamlType>();

            foreach (object key in types.Keys)
            {
                var ramlType = new RamlType();
                var type = types[key] as Dictionary<object, object>;
                ramlType.Name = key as string;

                // do not generate special collection type
                if (ramlType.Name.Equals(StringConstants.Collection, StringComparison.InvariantCulture))
                    break;

                ramlType.Description = GetValueByKey(type, RamlKeywords.Description) as string;

                ramlType.AdditionalProperties = true;

                ramlType.IsRootType = IsRootType(ramlType.Name);                

                if (GetValueByKey(type, RamlKeywords.AdditionalProperties) as string == "false")
                {
                    ramlType.AdditionalProperties = false;
                }

                var baseTypeName = GetValueByKey(type, RamlKeywords.Type) as string;
                if (!string.IsNullOrEmpty(baseTypeName))
                {
                    // check for array
                    RamlArray ramlArray = null;

                    if (baseTypeName.Substring(baseTypeName.Length - 2).Equals(RamlKeywords.ArrayBrackets, StringComparison.InvariantCulture))
                    {
                        ramlArray = new RamlArray();
                        ramlArray.ItemsTypeName = baseTypeName.Substring(0, baseTypeName.Length - 2);
                    }
                    else if (baseTypeName.Equals(RamlKeywords.Array, StringComparison.Ordinal))
                    {
                        ramlArray = new RamlArray();
                        ramlArray.ItemsTypeName = GetValueByKey(type, RamlKeywords.Items) as string;

                    }
                    else if(baseTypeName.Length > StringConstants.Collection.Length)
                    {
                        if (baseTypeName.Substring(baseTypeName.Length - StringConstants.Collection.Length).Equals(StringConstants.Collection, StringComparison.InvariantCulture))
                        {
                            ramlArray = new RamlArray();
                            ramlArray.ItemsTypeName = GetValueByKey(type, RamlKeywords.Items) as string;
                            ramlArray.ItemName = GetValueByKey(type, StringConstants.ItemName) as string;
                        }
                    }                    

                    if (ramlArray != null)
                    {
                        if (string.IsNullOrEmpty(ramlArray.ItemName))
                        {
                            ramlArray.ItemName = GetCollectionItemName(ramlType.Name);
                        }
                        
                        ramlType.Array = ramlArray;
                        ramlTypes.Add(ramlType);
                        continue;
                    }

                    // it is not an array
                    if(!baseTypeName.Equals(RamlDataTypes.Object, StringComparison.InvariantCulture))
                    {
                        ramlType.Base = new RamlBase();
                        ramlType.Base.Name = baseTypeName;
                        ramlType.Base.Pattern = GetValueByKey(type, RamlKeywords.Pattern) as string;
                    }
                }


                // check for enum
                RamlEnum ramlEnum = null;

                var enumValues = GetValueByKey(type, RamlKeywords.Enum) as List<object>;

                if (enumValues != null)
                {
                    ramlEnum = new RamlEnum();

                    if (!string.IsNullOrEmpty(baseTypeName))
                    {
                        ramlEnum.ItemsTypeName = baseTypeName;
                    }
                    else
                    {
                        ramlEnum.ItemsTypeName = RamlDataTypes.String;
                    }

                    // get enum values
                    ramlEnum.EnumValues = new List<string>();

                    foreach (object enumValue in enumValues)
                    {
                        ramlEnum.EnumValues.Add(enumValue as string);
                    }

                    ramlType.Enum = ramlEnum;
                    ramlTypes.Add(ramlType);
                    continue;
                }

                var properties = GetValueByKey(type, RamlKeywords.Properties) as Dictionary<object, object>;
                if (properties != null)
                {
                    ramlType.Properties = GetRamlProperties(GetValueByKey(type, RamlKeywords.Properties) as Dictionary<object, object>);
                }

                ramlType.MinProperties = ConvertStringToInt(GetValueByKey(type, RamlKeywords.MinProperties) as string);
                ramlType.MaxProperties = ConvertStringToInt(GetValueByKey(type, RamlKeywords.MaxProperties) as string);

                ramlTypes.Add(ramlType);
            }

            return ramlTypes;
        }

        private bool IsRootType(string name)
        {
            foreach(var rootTypeName in Program.RootTypeNames)
            {
                if (rootTypeName.Equals(name, StringComparison.InvariantCulture))
                {
                    return true;
                }
            }

            return false;
        }

        private List<RamlProperty> GetRamlProperties(Dictionary<object, object> properties)
        {
            if (properties == null)
                return null;

            var ramlProperties = new List<RamlProperty>();

            foreach (object key in properties.Keys)
            {
                var ramlProperty = new RamlProperty();
                var property = properties[key] as Dictionary<object, object>;
                ramlProperty.Required = true; //default
                string propertyName = key as string;

                // if ends with ? then optional
                if (propertyName.Substring(propertyName.Length - 1).Equals(RamlKeywords.QuestionMark, StringComparison.InvariantCulture))
                {
                    propertyName = propertyName.Substring(0, propertyName.Length - 1);
                    ramlProperty.Required = false;
                }
                ramlProperty.Name = propertyName;

                if (property != null)
                {
                    ramlProperty.Description = GetValueByKey(property, RamlKeywords.Description) as string;
                    ramlProperty.Type = GetValueByKey(property, RamlKeywords.Type) as string;
                    ramlProperty.Default = GetValueByKey(property, RamlKeywords.Default) as string;

                    if (GetValueByKey(property, RamlKeywords.Required) as string == RamlKeywords.False)
                    {
                        ramlProperty.Required = false;
                    }
                }
                else
                {
                    ramlProperty.Type = properties[key] as string;
                }

                if(IsArray(ramlProperty.Type))
                {
                    // check if we have item name
                    var itemName = GetValueByKey(property, StringConstants.ItemName) as string;

                    if (string.IsNullOrEmpty(itemName))
                    {
                        var ramlPropertyTypeName = GetRefDataType(ramlProperty.Type);
                        if (!string.IsNullOrEmpty(ramlPropertyTypeName))
                        {
                            itemName = GetCollectionItemName(ramlPropertyTypeName);
                        }
                        
                    }
                    ramlProperty.ArrayItemName = itemName;
                }

                

                ramlProperties.Add(ramlProperty);
            }

            return ramlProperties;
        }

        protected string GetCollectionItemName(string collectionTypeName)
        {
            if (string.IsNullOrEmpty(collectionTypeName))
            {
                return "item";
            }            

            var lowerCaseName = System.Char.ToLowerInvariant(collectionTypeName[0]) + collectionTypeName.Substring(1);

            if (lowerCaseName.Substring(lowerCaseName.Length - 2).Equals(RamlKeywords.ArrayBrackets, StringComparison.InvariantCulture))
            {
                return lowerCaseName.Substring(0, lowerCaseName.Length - 2);
            }

            var index = collectionTypeName.IndexOf("Collection");

            if (index < 0)
            {
                return lowerCaseName + "Item";
            }
            else
            {
                return lowerCaseName.Substring(0, index);
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

        private bool IsArray(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return false;
            }

            if ((typeName.Substring(typeName.Length - 2).Equals(RamlKeywords.ArrayBrackets, StringComparison.InvariantCulture)) ||
               (typeName.Equals(RamlKeywords.Array, StringComparison.Ordinal)))
            {
                return true;
            }


            if (typeName.Length > StringConstants.Collection.Length)
            {
                if (typeName.Substring(typeName.Length - StringConstants.Collection.Length).Equals(StringConstants.Collection, StringComparison.InvariantCulture))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetRefDataType(string ramlType)
        {
            char[] dotDelimiter = { '.' };

            var referenceAndTypeName = ramlType.Split(dotDelimiter, StringSplitOptions.RemoveEmptyEntries);

            string typeName = null;

            typeName = referenceAndTypeName[referenceAndTypeName.Length - 1];

            if (typeName != null)
            {
                return typeName;
            }
            else
            {
                return null;
            }
        }


        private int? ConvertStringToInt(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            int j;
            if (Int32.TryParse(input, out j))
                return j;
            else
                return null;
        }

        #endregion

    }
} 