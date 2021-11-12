using System;
using System.Collections.Concurrent;
using System.IO;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// 配置解析选择器
    /// </summary>
    public class DefaultConsulConfigurationParserSelector : IConsulConfigurationParserSelector
    {
        private static IConsulConfigurationParser _json = ConsulConfigurationJsonParser.Singleton;
        private static IConsulConfigurationParser _xml = ConsulConfigurationXmlParser.Singleton;
        private static IConsulConfigurationParser _ini = ConsulConfigurationIniParser.Singleton;
        private static IConsulConfigurationParser _prop = ConsulConfigurationPropertiesParser.Singleton;

        /// <summary>
        /// 全局默认的 JSON 解析器
        /// </summary>
        public static IConsulConfigurationParser Json
        {
            get => _json;
            set
            {
                if (value != null)
                {
                    _json = value;
                }
            }
        }

        /// <summary>
        /// 全局默认的 XML 解析器
        /// </summary>
        public static IConsulConfigurationParser Xml
        {
            get => _xml;
            set
            {
                if (value != null)
                {
                    _xml = value;
                }
            }
        }

        /// <summary>
        /// 全局默认的 INI 解析器
        /// </summary>
        public static IConsulConfigurationParser Ini
        {
            get => _ini;
            set
            {
                if (value != null)
                {
                    _ini = value;
                }
            }
        }

        /// <summary>
        /// 全局默认的 PROP 解析器，支持后缀为 .prop/.properties
        /// </summary>
        public static IConsulConfigurationParser Prop
        {
            get => _prop;
            set
            {
                if (value != null)
                {
                    _prop = value;
                }
            }
        }

        private readonly ConcurrentDictionary<string, IConsulConfigurationParser> _elementKeySpecificParsers =
            new ConcurrentDictionary<string, IConsulConfigurationParser>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, IConsulConfigurationParser> _extensionSpecificParsers =
            new ConcurrentDictionary<string, IConsulConfigurationParser>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public void SetParser(string relativePath, IConsulConfigurationParser parser)
        {
            _elementKeySpecificParsers.AddOrUpdate(relativePath, parser, (k, o) => parser);
        }

        /// <inheritdoc />
        public void SetParserForExtension(string extension, IConsulConfigurationParser parser)
        {
            _extensionSpecificParsers.AddOrUpdate(extension, parser, (k, o) => parser);
        }

        /// <inheritdoc />
        public virtual IConsulConfigurationParser Select(string relativePath)
        {
            if (_elementKeySpecificParsers.TryGetValue(relativePath, out var value))
            {
                return value;
            }

            var extension = Path.GetExtension(relativePath.ToLower());
            if (_extensionSpecificParsers.TryGetValue(extension, out value))
            {
                return value;
            }

            switch (extension)
            {
                case ".json":
                    return Json;
                case ".xml":
                    return Xml;
                case ".ini":
                    return Ini;
                case ".prop":
                case ".properties":
                    return Prop;
                default:
                    throw new NotSupportedException($"please set a `IConsulConfigurationParser` for `{relativePath}`");
            }
        }
    }
}