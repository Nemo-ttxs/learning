using System.Collections.Generic;
using Consul;
using Microsoft.Extensions.Configuration.Xml;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// XML 格式 配置解析器
    /// </summary>
    public class ConsulConfigurationXmlParser : IConsulConfigurationParser
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly ConsulConfigurationXmlParser Singleton = new ConsulConfigurationXmlParser();

        private ConsulConfigurationXmlParser() { }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string?>> Parse(KVPair kvPair) => XmlStreamConfigurationProvider.Read(kvPair.ToStream(), XmlDocumentDecryptor.Instance);
    }
}
