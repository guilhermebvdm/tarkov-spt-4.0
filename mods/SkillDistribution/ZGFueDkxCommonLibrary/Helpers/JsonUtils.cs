using System;

namespace ZGFueDkx.ZGCLib.Helpers
{
    internal class JsonUtils
    {
        [AttributeUsage(AttributeTargets.Property)]
        internal sealed class JsonIgnoreErrorAttribute : Attribute;
    }
}
