using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace RamlConverter.Xml
{
    public class XmlConverter : ConverterBase<XmlSchema, XmlSchemaType>
    {
        public XmlConverter(RamlFile file) : base(file) { }

        #region Implementatin of abstract methods from the base class

        protected override void AddSchemaTypeToSchema(XmlSchema schema, XmlSchemaType schemaType)
        {
            schema.Items.Add(schemaType);
        }

        protected override void InitializeSchema(XmlSchema schema, ConversionOptions options)
        {
            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            schema.AttributeFormDefault = XmlSchemaForm.Unqualified;

            if (!string.IsNullOrEmpty(options.XmlNamespace))
            {
                schema.TargetNamespace = options.XmlNamespace;
                schema.Namespaces.Add("", options.XmlNamespace);
            }
        }

        protected override void FinalizeSchema(XmlSchema schema, ConversionOptions options)
        {
        }

        protected override void ProcessRootType(RamlType ramlType, ConversionOptions options)
        {
            var settings = new XmlWriterSettings();
            settings.Indent = true;

            var fileName =  System.Char.ToLowerInvariant(ramlType.Name[0]) + ramlType.Name.Substring(1) +
                "." + FileExtensions.XmlSchema;

            string fullPath = Path.Combine(options.OutputDirectory, fileName);

            XmlSchema schema = CreateRootElementSchema(ramlType, options);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                var xmlWriter = XmlWriter.Create(fileStream, settings);
                schema.Write(xmlWriter);
            }
        }

        private XmlSchema CreateRootElementSchema(RamlType ramlType, ConversionOptions options)
        {
            var schema = new XmlSchema();

            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            schema.AttributeFormDefault = XmlSchemaForm.Unqualified;

            if (!string.IsNullOrEmpty(options.XmlNamespace))
            {
                schema.TargetNamespace = options.XmlNamespace;
                schema.Namespaces.Add("", options.XmlNamespace);
            }

            var fileName = Path.GetFileNameWithoutExtension(_ramlFile.FullPath) + "." + FileExtensions.XmlSchema;

            var include = new XmlSchemaInclude();           
            include.SchemaLocation = fileName;
            schema.Includes.Add(include);

            var element = new XmlSchemaElement();
            element.Name = System.Char.ToLowerInvariant(ramlType.Name[0]) + ramlType.Name.Substring(1);
            element.SchemaTypeName = new XmlQualifiedName(ramlType.Name, Namespaces.XmlSchema); 

            schema.Items.Add(element);

            return schema;
        }

        protected override XmlSchemaType RamlTypeToSchemaType(RamlType ramlType, ConversionOptions options)
        {
            if (ramlType == null)
                return null;

            XmlSchemaType xmlSchemaType;

            // check if it is enum
            if (ramlType.Enum != null)
            {
                xmlSchemaType = new XmlSchemaSimpleType();
                xmlSchemaType.Name = ramlType.Name;

                var restriction = new XmlSchemaSimpleTypeRestriction();


                var enumElementTypeName = RamlDataTypeToXmlSchemaDataType(ramlType.Enum.ItemsTypeName);

                if (enumElementTypeName == null)
                {
                    enumElementTypeName = GetXmlRefDataType(ramlType.Enum.ItemsTypeName, options.XmlNamespace);
                }

                if (enumElementTypeName != null)
                {
                    restriction.BaseTypeName = enumElementTypeName;
                }

                foreach (string enumValue in ramlType.Enum.EnumValues)
                {
                    var enumerationFacet = new XmlSchemaEnumerationFacet();
                    enumerationFacet.Value = enumValue;
                    restriction.Facets.Add(enumerationFacet);
                }

                ((XmlSchemaSimpleType)xmlSchemaType).Content = restriction;

                return xmlSchemaType;
            }

            xmlSchemaType = new XmlSchemaComplexType();
            xmlSchemaType.Name = ramlType.Name;

            // check if it is array
            if (ramlType.Array != null)
            {
                var arraySequence = new XmlSchemaSequence();
                var arrayElement = new XmlSchemaElement();

                
                arrayElement.Name = ramlType.Array.ItemName;
                
                arrayElement.MaxOccursString = "unbounded";

                var arrayElementTypeName = RamlDataTypeToXmlSchemaDataType(ramlType.Array.ItemsTypeName);

                if (arrayElementTypeName == null)
                {
                    arrayElementTypeName = GetXmlRefDataType(ramlType.Array.ItemsTypeName, options.XmlNamespace);
                }

                if (arrayElementTypeName != null)
                {
                    arrayElement.SchemaTypeName = arrayElementTypeName;
                }

                arraySequence.Items.Add(arrayElement);

                ((XmlSchemaComplexType)xmlSchemaType).Particle = arraySequence;

                return xmlSchemaType;
            }

            // check if it has base
            if (ramlType.Base != null)
            {
                // assume simple type for now
                xmlSchemaType = new XmlSchemaSimpleType();
                xmlSchemaType.Name = ramlType.Name;

                var restriction = new XmlSchemaSimpleTypeRestriction();


                var baseTypeName = RamlDataTypeToXmlSchemaDataType(ramlType.Base.Name);

                if (baseTypeName == null)
                {
                    baseTypeName = GetXmlRefDataType(ramlType.Base.Name, options.XmlNamespace);
                }

                if (baseTypeName != null)
                {
                    restriction.BaseTypeName = baseTypeName;
                }

                if (!string.IsNullOrEmpty(ramlType.Base.Pattern))
                {
                    var pattern = new XmlSchemaPatternFacet();
                    pattern.Value = ramlType.Base.Pattern;
                    restriction.Facets.Add(pattern);
                }               

                ((XmlSchemaSimpleType)xmlSchemaType).Content = restriction;

                return xmlSchemaType;
            }

            var annotation = GetXmlAnnotattion(ramlType.Description);

            if (annotation != null && options.GenerateDescriptions)
            {
                xmlSchemaType.Annotation = annotation;
            }

            var sequence = new XmlSchemaSequence();
            ((XmlSchemaComplexType)xmlSchemaType).Particle = sequence;

            if (ramlType.Properties == null || ramlType.Properties.Count == 0)
            {
                return xmlSchemaType;
            }

            foreach (RamlProperty ramlProperty in ramlType.Properties)
            {
                var xmlSchemaProperty = new XmlSchemaElement();
                xmlSchemaProperty.Name = ramlProperty.Name;

                var typeName = RamlDataTypeToXmlSchemaDataType(ramlProperty.Type);

                if (typeName == null)
                {
                    typeName = GetXmlRefDataType(ramlProperty.Type, options.XmlNamespace);
                }

                if (typeName != null)
                {
                    xmlSchemaProperty.SchemaTypeName = typeName;
                }

                var propertyAnnotation = GetXmlAnnotattion(ramlProperty.Description);

                if (propertyAnnotation != null && options.GenerateDescriptions)
                {
                    xmlSchemaProperty.Annotation = propertyAnnotation;
                }

                sequence.Items.Add(xmlSchemaProperty);

                if (!ramlProperty.Required)
                {
                    xmlSchemaProperty.MinOccurs = 0;
                    xmlSchemaProperty.IsNillable = true;
                }

            }
            return xmlSchemaType;
        }

        protected override void AddExternalReferences(XmlSchema schema)
        {
            foreach (string key in _ramlFile.Uses.Keys)
            {
                var include = new XmlSchemaInclude();
                var location = ChangeFileNameExtension(_ramlFile.Uses[key] as string, "xsd");
                include.SchemaLocation = location;
                schema.Includes.Add(include);
            }
        }

        protected override void WriteSchemaFile(XmlSchema schema, ConversionOptions opitons)
        {
            var settings = new XmlWriterSettings();
            settings.Indent = true;

            var fileName = Path.GetFileNameWithoutExtension(_ramlFile.FullPath) + "." + FileExtensions.XmlSchema;

            string fullPath = Path.Combine(opitons.OutputDirectory, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                var xmlWriter = XmlWriter.Create(fileStream, settings);
                schema.Write(xmlWriter);
            }
        }

        #endregion

        private XmlQualifiedName RamlDataTypeToXmlSchemaDataType(string ramlDataType)
        {
            switch (ramlDataType)
            {
                case RamlDataTypes.Boolean:
                    return new XmlQualifiedName(XmlDataTypes.Boolean, Namespaces.XmlSchema);
                case RamlDataTypes.DateOnly:
                    return new XmlQualifiedName(XmlDataTypes.Date, Namespaces.XmlSchema);
                case RamlDataTypes.DateTime:
                    return new XmlQualifiedName(XmlDataTypes.DateTime, Namespaces.XmlSchema);
                case RamlDataTypes.DateTimeOnly:
                    return new XmlQualifiedName(XmlDataTypes.DateTime, Namespaces.XmlSchema);
                case RamlDataTypes.Integer:
                    return new XmlQualifiedName(XmlDataTypes.Integer, Namespaces.XmlSchema);
                case RamlDataTypes.Nil:
                    return new XmlQualifiedName(XmlDataTypes.String, Namespaces.XmlSchema);
                case RamlDataTypes.Number:
                    return new XmlQualifiedName(XmlDataTypes.Decimal, Namespaces.XmlSchema);
                case RamlDataTypes.String:
                    return new XmlQualifiedName(XmlDataTypes.String, Namespaces.XmlSchema);
                case RamlDataTypes.TimeOnly:
                    return new XmlQualifiedName(XmlDataTypes.Time, Namespaces.XmlSchema);
                case null:
                    return new XmlQualifiedName(XmlDataTypes.String, Namespaces.XmlSchema);
                default:
                    return null;
            }
        }        

        private XmlSchemaAnnotation GetXmlAnnotattion(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var annotation = new XmlSchemaAnnotation();
            var documentation = new XmlSchemaDocumentation();
            documentation.Markup = TextToNodeArray(text);
            annotation.Items.Add(documentation);

            return annotation;
        }

        private XmlQualifiedName GetXmlRefDataType(string ramlType, string nameSpace)
        {
            XmlQualifiedName refDatatype = null;

            char[] dotDelimiter = { '.' };

            var referenceAndTypeName = ramlType.Split(dotDelimiter, StringSplitOptions.RemoveEmptyEntries);

            string typeName = null;

            typeName = referenceAndTypeName[referenceAndTypeName.Length - 1];

            if (typeName != null)
            {
                refDatatype = new XmlQualifiedName(typeName, nameSpace);
            }
            else
            {
                return null;
            }

            return refDatatype;
        }

        private XmlNode[] TextToNodeArray(string text)
        {
            XmlDocument doc = new XmlDocument();
            return new XmlNode[1] {
                  doc.CreateTextNode(text)};
        }

        
    }
}
