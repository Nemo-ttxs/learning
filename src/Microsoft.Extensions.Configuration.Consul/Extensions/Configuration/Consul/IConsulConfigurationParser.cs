using System.Collections.Generic;
using System.IO;
using Consul;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// Defines how the configuration loaded from Consul should be parsed.
    /// </summary>
    public interface IConsulConfigurationParser
    {
        /// <summary>
        /// Parse the <see cref="Stream" /> into a dictionary.
        /// </summary>
        /// <param name="kvPair">The byte[] to parse.</param>
        /// <returns>A dictionary representing the configuration in a flattened form.</returns>
        IEnumerable<KeyValuePair<string, string?>> Parse(KVPair kvPair);
    }
}
