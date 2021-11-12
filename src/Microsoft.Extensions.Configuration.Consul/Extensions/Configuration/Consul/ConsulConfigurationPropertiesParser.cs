using Consul;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// 支持 properties 格式的解析，文件后缀为 .prop 或 .properties
    /// </summary>
    public class ConsulConfigurationPropertiesParser : IConsulConfigurationParser
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly ConsulConfigurationPropertiesParser
            Singleton = new ConsulConfigurationPropertiesParser();

        private ConsulConfigurationPropertiesParser() { }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string?>> Parse(KVPair kvPair)
        {
            var text = Encoding.UTF8.GetString(kvPair.Value);
            foreach (var line in text.Split('\r', '\n'))
            {
                var pair = line.Trim();
                //skip empty line and comments and incorrect config
                if (string.IsNullOrWhiteSpace(pair) || pair.StartsWith("#") || pair.StartsWith("="))
                {
                    continue;
                }

                var index = pair.IndexOf('=');
                if (index > -1)
                {
                    var key = pair.Substring(0, index).Trim();
                    var value = "";
                    if (pair.Length > index + 1)
                    {
                        value = pair.Substring(index + 1).Trim();
                        if (value.StartsWith("\"") && value.EndsWith("\""))
                        {
                            if (value.Length >= 2)
                            {
                                value = value.Substring(1, value.Length - 2);
                            }
                        }
                    }
                    yield return new KeyValuePair<string, string?>(key, value);
                    continue;
                }
                //value is null, only key without equal sign.
                yield return new KeyValuePair<string, string?>(pair, null);
            }
        }
    }
}
