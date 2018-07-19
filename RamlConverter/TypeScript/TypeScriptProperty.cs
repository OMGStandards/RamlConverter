namespace RamlConverter.TypeScript
{
    public class TypeScriptProperty
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Type { get; set; }
        public string ArrayItemName { get; set; }
        public string Default { get; set; }
        public bool Required { get; set; }
    }
}