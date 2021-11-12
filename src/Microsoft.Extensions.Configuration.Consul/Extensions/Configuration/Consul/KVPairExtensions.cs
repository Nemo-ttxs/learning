using System.IO;
using Consul;

namespace Microsoft.Extensions.Configuration.Consul
{
    // ReSharper disable InconsistentNaming
    internal static class KVPairExtensions
    // ReSharper restore InconsistentNaming
    {
        public static Stream ToStream(this KVPair pair)
        {
            return new MemoryStream(pair.Value, false);
        }
    }
}