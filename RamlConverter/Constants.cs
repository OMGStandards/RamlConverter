namespace RamlConverter
{
    internal static class ErrorMessages
    {
        internal const string NoParametersProvided = "No input parameters were provided.";
        internal const string UnrecognizedParameter = "Unrecognized parameter: {0}.";
        internal const string FileNameNotDefined = "Input file name is not defined.";
        internal const string InputFileDirNotDefined = "Neither Input Directory no Input file was not defined.";
        internal const string FileDoesNotExist = "Input file {} does not exist.";
    }

    internal static class WarningMessages
    {
        internal const string InputDirectoryIgnored = "Input file name was specified so input directory is ignored.";
    }

    internal static class ConfigurationStrings
    {
        internal const string XmlSectionName = "XmlSchema";
        internal const string JsonSectionName = "JsonSchema";
        internal const string CSharpSectionName = "CSharp";
        internal const string TypeScriptSectionName = "TypeScript";
    }

    internal static class FileExtensions
    {
        internal const string Raml = "raml";
        internal const string JsonSchema = "json";
        internal const string XmlSchema = "xsd";
        internal const string CSharp = "cs";
        internal const string TypeScript = "ts";
    }

    internal static class Namespaces
    {
        internal const string Omg = "http://wwww.omg.org/spec";
        internal const string XmlSchema = "http://www.w3.org/2001/XMLSchema";
    }

    internal static class RamlDataTypes
    {
        internal const string Object = "object";
        internal const string String = "string";
        internal const string Number = "number";
        internal const string Integer = "integer";
        internal const string Boolean = "boolean";
        internal const string DateOnly = "date-only";
        internal const string TimeOnly = "time-only";
        internal const string DateTimeOnly = "datetime-only";
        internal const string DateTime = "datetime";
        internal const string Nil = "nil";
    }
    internal static class XmlDataTypes
    {
        internal const string String = "string";
        internal const string Decimal = "decimal";
        internal const string Integer = "integer";
        internal const string Boolean = "boolean";
        internal const string Date = "date";
        internal const string Time = "time";
        internal const string DateTime = "dateTime";
    }

    internal static class CSharpDataTypes
    {
        internal const string String = "string";
        internal const string Decimal = "decimal";
        internal const string Integer = "int";
        internal const string Boolean = "bool";        
        internal const string DateTime = "DateTime";
    }

    internal static class JsonDataTypes
    {
        internal const string Object = "object";
        internal const string String = "string";
        internal const string Number = "number";
        internal const string Boolean = "boolean";
        internal const string Null = "null";
    }

    internal static class TypeScriptDataTypes
    {
        internal const string Object = "object";
        internal const string String = "string";
        internal const string Number = "number";
        internal const string Boolean = "boolean";
        internal const string Any = "any";
        internal const string Nil = "nil";
    }

    internal static class SpecialTypes
    {
        internal const string DecimalString = "DecimalString";
        internal const string Collection = "Collection";
    }


    internal static class RamlKeywords
    {
        internal const string Array = "array";
        internal const string ArrayBrackets = "[]";
        internal const string Description = "description";
        internal const string Default = "default";
        internal const string Type = "type";
        internal const string Required = "required";
        internal const string AdditionalProperties = "additionalProperties";
        internal const string Pattern = "pattern";
        internal const string Properties = "properties";
        internal const string MinProperties = "minProperties";
        internal const string MaxProperties = "maxProperties";
        internal const string Usage = "usage";
        internal const string Uses = "uses";
        internal const string Items = "items";
        internal const string Enum = "enum";
        internal const string False = "false";
        internal const string True = "true";
        internal const string QuestionMark = "?";

    }

    internal static class StringConstants
    {
        internal const string Collection = "Collection";        
        internal const string Definitions = "definitions";
        internal const string ItemName = "itemName";
        internal const string QuestionMark = "?";
    }

}
