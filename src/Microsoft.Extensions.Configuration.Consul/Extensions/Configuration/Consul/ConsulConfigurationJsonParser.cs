using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Consul;
using Microsoft.Extensions.Configuration.Json;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// JSON 格式 配置解析器
    /// </summary>
    public class ConsulConfigurationJsonParser : IConsulConfigurationParser
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly ConsulConfigurationJsonParser Singleton = new ConsulConfigurationJsonParser();

        private ConsulConfigurationJsonParser() { }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string?>> Parse(KVPair kvPair) => JsonConfigurationFileParser.Parse(kvPair.ToStream());

        private static class JsonConfigurationFileParser
        {
            private static readonly Func<Stream, IDictionary<string, string?>> ParseMethod;

            static JsonConfigurationFileParser() => ParseMethod = GenerateParseMethod();

            /// <summary>
            /// yes, force call internal class method
            /// </summary>
            /// <returns></returns>
            private static Func<Stream, IDictionary<string, string?>> GenerateParseMethod()
            {
                var dynamicMethod = new DynamicMethod(
                    $"{nameof(JsonConfigurationFileParser)}abc.{nameof(Parse)}",
                    typeof(IDictionary<string, string>),
                    new[] { typeof(Stream) },
                    typeof(JsonConfigurationFileParser).Module,
                    true);

                var method = typeof(JsonConfigurationProvider)
                    .Assembly
                    .GetType($"{typeof(JsonConfigurationProvider).Namespace}.{nameof(JsonConfigurationFileParser)}")
                    .GetMethod(nameof(Parse), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                var ilGenerator = dynamicMethod.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Call, method);
                ilGenerator.Emit(OpCodes.Ret);

                return dynamicMethod.CreateDelegate<Func<Stream, IDictionary<string, string?>>>();
            }

            public static IDictionary<string, string?> Parse(Stream input) => ParseMethod(input);
        }
    }
}
