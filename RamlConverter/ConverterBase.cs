using System;
using System.Collections.Generic;
using System.IO;

namespace RamlConverter
{
    public abstract class ConverterBase < TSchema, TType> where TSchema : new()
    {
        protected RamlFile _ramlFile;

        public ConverterBase(RamlFile ramlFile)
        {
            _ramlFile = ramlFile;
        }
        public virtual void ConvertRaml (ConversionOptions options) 
        {
            TSchema schema = new TSchema();

            InitializeSchema(schema, options);

            foreach (RamlType ramlType in _ramlFile.Types)
            {
                TType schemaType = RamlTypeToSchemaType(ramlType, options);
                AddSchemaTypeToSchema(schema, schemaType);
                if (ramlType.IsRootType)
                {
                    ProcessRootType(ramlType, options);
                }
            }

            FinalizeSchema(schema, options);

            if (_ramlFile.Uses != null && _ramlFile.Uses.Count > 0)
            {
                AddExternalReferences(schema);
            }

            
            WriteSchemaFile(schema, options);
        }

        

        protected abstract void InitializeSchema(TSchema schema, ConversionOptions options);
        protected abstract void FinalizeSchema(TSchema schema, ConversionOptions options);
        protected abstract TType RamlTypeToSchemaType(RamlType ramlType, ConversionOptions options);
        protected abstract void AddSchemaTypeToSchema(TSchema schema, TType schemaType);
        protected abstract void AddExternalReferences(TSchema schema);
        protected abstract void WriteSchemaFile(TSchema schema, ConversionOptions options);
        protected abstract void ProcessRootType(RamlType ramlType, ConversionOptions options);


        protected string ChangeFileNameExtension(string fileName, string newExtension)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var newFileName = Path.GetFileNameWithoutExtension(fileName);

            if (string.IsNullOrEmpty(newExtension))
            {
                return newFileName;
            }

            newFileName = newFileName + "." + newExtension;

            return newFileName;
        }        
        protected string GetRefDataType(string ramlType)
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

        protected bool IsDecimalString(string ramlType)
        {

            if(string.IsNullOrEmpty(ramlType))
            {
                return false;
            }

            char[] dotDelimiter = { '.' };

            var referenceAndTypeName = ramlType.Split(dotDelimiter, StringSplitOptions.RemoveEmptyEntries);

            string typeName = null;

            typeName = referenceAndTypeName[referenceAndTypeName.Length - 1];

            if (typeName == SpecialTypes.DecimalString)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
