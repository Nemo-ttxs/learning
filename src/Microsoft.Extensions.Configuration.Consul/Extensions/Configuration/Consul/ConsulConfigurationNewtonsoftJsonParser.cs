using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Consul;
using Microsoft.Extensions.Configuration.NewtonsoftJson;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// JSON 格式 配置解析器，基于 Newtonsoft
    /// </summary>
    public class ConsulConfigurationNewtonsoftJsonParser : IConsulConfigurationParser
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly ConsulConfigurationNewtonsoftJsonParser Singleton = new ConsulConfigurationNewtonsoftJsonParser();

        private ConsulConfigurationNewtonsoftJsonParser() { }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string?>> Parse(KVPair kvPair) => NewtonsoftJsonConfigurationFileParser.Parse(kvPair.ToStream());

        private static class NewtonsoftJsonConfigurationFileParser
        {
            private static readonly Func<Stream, IDictionary<string, string?>> ParseMethod;

            static NewtonsoftJsonConfigurationFileParser() => ParseMethod = GenerateParseMethod();

            /// <summary>
            /// yes, force call internal class method
            /// </summary>
            /// <returns></returns>
            private static Func<Stream, IDictionary<string, string?>> GenerateParseMethod()
            {
                var dynamicMethod = new DynamicMethod(
                    $"{nameof(NewtonsoftJsonConfigurationFileParser)}.{nameof(Parse)}",
                    typeof(IDictionary<string, string>),
                    new[] { typeof(Stream) },
                    typeof(NewtonsoftJsonConfigurationFileParser).Module,
                    true);

                var method = typeof(NewtonsoftJsonConfigurationProvider)
                    .Assembly
                    .GetType($"{typeof(NewtonsoftJsonConfigurationProvider).Namespace}.{nameof(NewtonsoftJsonConfigurationFileParser)}")
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
