using System;
using System.Reflection.Emit;

namespace Microsoft.Extensions.Configuration.Consul
{
    internal static class DynamicMethodExtensions
    {
        public static TDelegate CreateDelegate<TDelegate>(this DynamicMethod method)
            where TDelegate : Delegate
        {
            return (TDelegate) method.CreateDelegate(typeof(TDelegate));
        }
    }
}
