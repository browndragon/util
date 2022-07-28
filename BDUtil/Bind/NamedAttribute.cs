using System;

namespace BDUtil.Bind
{
    /// Named is a specific binding string->type (and since every Type has a few names, it's meaningful to discuss it in the abstract...)
    /// Utilities are provided for Name, Full Name, AQName, etc.
    /// The default in most name-y contexts is to go for Name anyway, so that's a bit silly.
    public sealed class NamedAttribute : BindAttribute
    {
        public const string kTypeName = "{name}";
        public const string kTypeFullName = "{fullname}";
        public const string kTypeAssemblyQualifiedName = "{assemblyqualifiedname}";
        public string Nick;
        public NamedAttribute(string nick) => Nick = nick;
        public string GetName(Type type) => Nick switch
        {
            kTypeName => type.Name,
            kTypeFullName => type.FullName,
            kTypeAssemblyQualifiedName => type.AssemblyQualifiedName,
            _ => Nick,
        };
        public override object GetKey(Type type) => GetName(type);
    }
}