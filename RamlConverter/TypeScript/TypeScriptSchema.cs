﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RamlConverter.TypeScript
{
    public class TypeScriptSchema
    {
        public string XmlNamespace { get; set; }
        public int? IndentSize { get; set; }
        public bool DisableTSLine { get; set; }
        public Dictionary<string, string> Imports { get; set; }
       
        public List<TypeScriptType> Types { get; set; }

        public void Write (StreamWriter writer)
        {
            if(this.DisableTSLine)
            {
                WriteDisableTSLint(writer);
            }

            WriteImports(writer);

            if (this.Types != null)
            {
                WriteTypes(writer);
            }
        }

        private void WriteTypes(StreamWriter writer)
        {
            if (this.Types == null)
                return;

            if (this.Types.Count == 0)
                return;

            foreach(TypeScriptType type in this.Types)
            {
                // we do not create TypeScrips DecimalString type since you cannot extend
                // string type. Instead we just map DecimalString to string
                if (type.Name != SpecialTypes.DecimalString)
                {
                    WriteType(writer, type);
                }
            }
        }

        private void WriteType(StreamWriter writer, TypeScriptType type)
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
                if (type.IsRootType)
                {
                    WriteNamespaceHandler(writer, type);
                }
            }           

            WriteTypeFooter(writer);
        }

        private void WriteEnumTypeHeader(StreamWriter writer, TypeScriptType type)
        {
            writer.WriteLine("export enum {0} {{", type.Name);
        }

        private void WriteEnumMember(StreamWriter writer, string enumMember, bool last)
        {
            string endOfEnum = last ? "" : ",";            
            writer.WriteLine(Tab(1) + "{0} = \"{0}\"{1}", enumMember, endOfEnum);
        }

        private void WriteArrayTypeHeader(StreamWriter writer, TypeScriptType type)
        {
            writer.WriteLine("export class {0} extends Array<{1}> {{", type.Name, type.Array.ItemsTypeName);           
        }

        private void WriteProperty(StreamWriter writer, TypeScriptProperty property, int propertyOrder)
        {
            if (!string.IsNullOrEmpty(property.Comment))
            {
                WritePropertyComment(writer, property.Comment);
            }


            if (string.IsNullOrEmpty(property.ArrayItemName))
            {
                writer.WriteLine(Tab(1) + "@XMLChild({})");
            }
            else
            {
                writer.WriteLine(Tab(1) + "@XMLChild({");
                writer.WriteLine(Tab(2) + "name: \"{0}\",", property.ArrayItemName);
                writer.WriteLine(Tab(2) + "implicitStructure: \"{0}.$\"", property.Name);
                writer.WriteLine(Tab(1) + "})");
            }

            var propertyName = property.Required ? property.Name : property.Name + StringConstants.QuestionMark;
            writer.WriteLine(Tab(1) + "public {0}: {1};", propertyName, property.Type);
            writer.WriteLine();

        }

        private void WritePropertyComment(StreamWriter writer, string comment)
        {
            writer.WriteLine(Tab(1) + "/**");
            var commentLines = SplitComment(comment);
            foreach (string commentLine in commentLines)
            {
                writer.WriteLine(Tab(1) + "* {0}", commentLine);
            }
            writer.WriteLine(Tab(1) + "**/");
        }

        private void WriteNamespaceHandler(StreamWriter writer, TypeScriptType type)
        {
            writer.WriteLine();
            writer.WriteLine(Tab(1) + "// default namespace");
            writer.WriteLine(Tab(1) + "@XMLAttribute({name: \"xmlns\"})");
            writer.WriteLine(Tab(1) + "public defaultXmlNamespace?: string;");
            writer.WriteLine();
            writer.WriteLine(Tab(1) + "public enableDefaultXmlNamespace(): void {");
            writer.WriteLine(Tab(2) + "this.defaultXmlNamespace = \"{0}\";",this.XmlNamespace);
            writer.WriteLine(Tab(1) + "}");
            writer.WriteLine();
            writer.WriteLine(Tab(1) + "public disableDefaultXmlNamespace(): void {");
            writer.WriteLine(Tab(2) + "this.defaultXmlNamespace = undefined;");
            writer.WriteLine(Tab(1) + "}");
        }

        private void WriteTypeFooter(StreamWriter writer)
        {
            writer.WriteLine("}");
            writer.WriteLine();
        }

        private void WriteTypeHeader(StreamWriter writer, TypeScriptType type)
        {
            writer.WriteLine("@XMLElement({{root: \"{0}\"}})", System.Char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1));
            writer.WriteLine("export class {0} {{", type.Name);                   
        }

        private void WriteTypeComment(StreamWriter writer, string comment)
        {
            writer.WriteLine("/**");
            var commentLines = SplitComment(comment);
            foreach(string commentLine in commentLines)
            {
                writer.WriteLine("* " + commentLine);
            }           
            writer.WriteLine("*/");
        }

        private string[] SplitComment(string comment)
        {
            char[] slashDelimiter = { '\n' };

            return comment.Split(slashDelimiter, StringSplitOptions.RemoveEmptyEntries);        
        }

        private void WriteImports(StreamWriter writer)
        {
            writer.WriteLine("import {XMLAttribute, XMLChild, XMLElement} from \"xml-decorators\";");

            if (Imports == null)
            {
                writer.WriteLine();
                return;
            }

            if (Imports.Count == 0)
            {
                writer.WriteLine();
                return;
            }

            foreach (string importKey in Imports.Keys)
            {
                writer.WriteLine("import * as {0} from \"./{1}\";", importKey, Imports[importKey]);

            }
            writer.WriteLine();
        }

        private void WriteDisableTSLint(StreamWriter writer)
        {
            writer.WriteLine("/* tslint:disable */");
        }

        private string Tab(int numberOfTabs)
        {
            if (this.IndentSize.HasValue)
            {
                return new string(' ', numberOfTabs * this.IndentSize.Value);
            }
            else
            {
                return new string('\t', numberOfTabs);
            }
        }
    }
}
