using System.Collections.Generic;
using Consul;
using Microsoft.Extensions.Configuration.Ini;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// INI 格式 配置解析器
    /// </summary>
    public class ConsulConfigurationIniParser : IConsulConfigurationParser
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly ConsulConfigurationIniParser Singleton = new ConsulConfigurationIniParser();

        private ConsulConfigurationIniParser() { }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string?>> Parse(KVPair kvPair) => IniStreamConfigurationProvider.Read(kvPair.ToStream());
    }
}
