using System;
using System.IO;
using RamlConverter.Xml;
using RamlConverter.CSharp;
using RamlConverter.TypeScript;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace RamlConverter
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static List<string> RootTypeNames { get; set; }

        static void Main(string[] args)
        {
            // RAML files to be processed
            string[] inputFiles = null;

            // read config file
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            var appconfig = new AppConfig();
            Configuration.Bind(appconfig);            
            
            string fileName = appconfig.InputFileName;            
            string inputDirectory = appconfig.InputDirectory;
            string outputDirectory = appconfig.OutputDirectory;
            bool generateXml = appconfig.GenerateXmlSchema.HasValue ? appconfig.GenerateXmlSchema.Value : false;
            bool generateJson = appconfig.GenerateJsonSchema.HasValue ? appconfig.GenerateJsonSchema.Value : false;
            bool generateCSharp = appconfig.GenerateCSharpCode.HasValue ? appconfig.GenerateCSharpCode.Value : false;
            bool generateTypeScript = appconfig.GenerateTypeScriptCode.HasValue ? appconfig.GenerateTypeScriptCode.Value : false;
            bool generateDescriptions = appconfig.GenerateDescriptions.HasValue ? appconfig.GenerateDescriptions.Value : false;
            RootTypeNames = appconfig.RootTypes;

            if (inputDirectory == null)
            {
                inputDirectory = Directory.GetCurrentDirectory();
            }

            if (outputDirectory == null)
            {
                outputDirectory = inputDirectory;
            }

            string xmlNamespace = "http://tempuri.org/";
            string xmlOutputDirectory = null;
            string csNamespace = "DataContract";
            string csOutputDirectory = null;
            string jsonOutputDirectory = null;
            string tsOutputDirectory = null;

            var csconfig = new CSharpConfig();
            Configuration.GetSection(ConfigurationStrings.CSharpSectionName).Bind(csconfig);
            csNamespace = csconfig.Namespace;
            csOutputDirectory = csconfig.OutputDirectory;

            var tsonconfig = new TypeScriptConfig();
            Configuration.GetSection(ConfigurationStrings.TypeScriptSectionName).Bind(tsonconfig);
            tsOutputDirectory = tsonconfig.OutputDirectory;

            var jsonconfig = new JsonSchemaConfig();
            Configuration.GetSection(ConfigurationStrings.JsonSectionName).Bind(jsonconfig);
            jsonOutputDirectory = jsonconfig.OutputDirectory;

            var xmlconfig = new XmlSchemaConfig();
            Configuration.GetSection(ConfigurationStrings.XmlSectionName).Bind(xmlconfig);
            xmlNamespace = xmlconfig.Namespace;
            xmlOutputDirectory = xmlconfig.OutputDirectory;

            // override using input parameters
            foreach (string arg in args)
            {
                if (string.Compare(arg, 0, "/file:", 0, 6, true) == 0)
                    fileName = arg.Substring(6);
                else if (string.Compare(arg, 0, "/xmlNS:", 0, 7, true) == 0)
                    xmlNamespace = arg.Substring(7);
                else if (string.Compare(arg, 0, "/csNS:", 0, 74, true) == 0)
                    csNamespace = arg.Substring(6);
                else if (string.Compare(arg, 0, "/cs", 0, 3, true) == 0)
                    generateCSharp = true;
                else if (string.Compare(arg, 0, "/ts", 0, 3, true) == 0)
                    generateTypeScript = true;
                else if (string.Compare(arg, 0, "/inputDir:", 0, 10, true) == 0)
                    inputDirectory = arg.Substring(10);
                else if (string.Compare(arg, 0, "/outputDir:", 0, 11, true) == 0)
                    outputDirectory = arg.Substring(11);
                else if (string.Compare(arg, 0, "/xml", 0, 4, true) == 0)
                    generateXml = true;
                else if (string.Compare(arg, 0, "/json", 0, 5, true) == 0)
                    generateJson = true;
                else if (string.Compare(arg, 0, "/desc", 0, 5, true) == 0)
                    generateDescriptions = true;
                else
                {
                    Console.WriteLine(ErrorMessages.UnrecognizedParameter, arg);
                    ShowHelp();
                    return;
                }
            }
            
            if (!string.IsNullOrEmpty(fileName))  // do single file if it was specified
            {
                inputFiles = new string[1];
                inputFiles[0] = Path.GetFullPath(fileName);                
            }
            else
            {
                inputFiles = Directory.GetFiles(Path.GetFullPath(inputDirectory), "*." + FileExtensions.Raml);                
            }

            var options = new ConversionOptions()
            {
                GenerateDescriptions = generateDescriptions,
                XmlNamespace = xmlNamespace,
                CSharpNamespace = csNamespace,
                OutputDirectory = outputDirectory
            };


            foreach (string file in inputFiles)
            {
                var ramlFile = new RamlFile(file);

                if (generateJson)
                {
                    if (!string.IsNullOrEmpty(jsonOutputDirectory))
                    {
                        options.OutputDirectory = jsonOutputDirectory;
                    }                        

                    var jsonConverter = new JsonConverter(ramlFile);
                    jsonConverter.ConvertRaml(options);
                }

                if (generateXml)
                {
                    if (!string.IsNullOrEmpty(xmlOutputDirectory))
                    {
                        options.OutputDirectory = xmlOutputDirectory;
                    }

                    var xmlConverter = new XmlConverter(ramlFile);
                    xmlConverter.ConvertRaml(options);
                }
                if (generateCSharp)
                {
                    if (!string.IsNullOrEmpty(csOutputDirectory))
                    {
                        options.OutputDirectory = csOutputDirectory;
                    }

                    var cSharpConverter = new CSharpConverter(ramlFile);
                    cSharpConverter.ConvertRaml(options);
                }
                if (generateTypeScript)
                {
                    if (!string.IsNullOrEmpty(tsOutputDirectory))
                    {
                        options.OutputDirectory = tsOutputDirectory;
                    }

                    var typeScriptConverter = new TypeScriptConverter(ramlFile);
                    typeScriptConverter.ConvertRaml(options);
                }
            }
        }

        private static void WriteError(string errorMessage)
        {
            Console.WriteLine("ERROR: " + errorMessage);
        }

        private static void WriteWarning(string warningMessage)
        {
            Console.WriteLine("WARNING: " + warningMessage);
        }

        private static void ShowHeader()
        {
            Console.WriteLine("RamlToSchema utility to generate Xml and Json schemas from Raml types.");
            Console.WriteLine();
        }

        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Input parameters:");

            Console.WriteLine("/file:      - Specifies RAML input filename.");
            Console.WriteLine("/inputDir:  - Specifies directory with input files.");
            Console.WriteLine("/outputDir: - Specifies directory to write XML and/or JSON schema files.");
            Console.WriteLine("/json       - Specifies that JSON schemas should be generated.");
            Console.WriteLine("/xml        - Specifies that XML schemas should be generated.");
            Console.WriteLine("/cs         - Specifies that C sharp data contracts should be generated.");
            Console.WriteLine("/ts         - Specifies that TypeScript data contracts should be generated.");
            Console.WriteLine("/desc       - Specifies that descriptions should be generated.");
            Console.WriteLine("/xmlNS:     - Specifies XML namespace.");
            Console.WriteLine("/csNS:      - Specifies C sharp namespace.");

            Console.WriteLine();
        }
    }
}
