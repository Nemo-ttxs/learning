using System;
using Consul;
using Microsoft.Extensions.Configuration.Consul;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// 配置扩展
    /// </summary>
    public static class ConsulConfigurationBuilderExtensions
    {
        /// <summary>
        /// 添加来自 consul 的配置
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="consulClient">consul 工具</param>
        /// <param name="configKeyPath">配置基础路径</param>
        /// <param name="waitForInitialized">是否等待配置第一次初始化成功，默认为 true 即默认等待。</param>
        /// <param name="waitInitializedTimeoutSeconds">等待配置第一次初始化成功超时时间，默认1分钟</param>
        /// <param name="reloadOnChange">是否要在配置变更时重新加载，默认 false 即默认不监控配置变化。</param>
        /// <param name="watchIntervalSeconds">配置变化同步最新配置时的间隔时间，默认1分钟，在间隔时间内配置发生变更时，将会提前触发变更逻辑。</param>
        /// <param name="ignoreExtensionAsPartOfKey">
        /// 不建议调整默认配置。
        /// 作为键的一部分时候是否忽略后缀。默认 为 true.
        /// 比如 app.json，默认情况 app: 作为前缀，否则 以 app.json: 作为前缀。（除了该文件在<see cref="ConsulConfigurationSource.TreatAsRoot"/>列表中时） 
        /// </param>
        /// <param name="treatAsRoot">
        /// 把哪些元素当成根元素对待，忽略 consul 配置路径。
        /// 不在该列表中的元素在配置项中会以 consul 相对路径作为前缀。
        /// </param>
        /// <returns></returns>
        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder,
            IConsulClient consulClient,
            string configKeyPath,
            bool waitForInitialized = true,
            long waitInitializedTimeoutSeconds = 60,
            bool reloadOnChange = false,
            long watchIntervalSeconds = 60,
            bool ignoreExtensionAsPartOfKey = true,
            params string[] treatAsRoot)
        {
            return builder.AddConsul(source =>
            {
                source.ConsulClient = consulClient;
                source.ConsulKeyPath = configKeyPath;
                source.WaitForInitialized = waitForInitialized;
                source.WaitInitializedTimeout = TimeSpan.FromSeconds(waitInitializedTimeoutSeconds);
                source.ReloadOnChange = reloadOnChange;
                source.WatchInterval = TimeSpan.FromSeconds(watchIntervalSeconds);
                source.IgnoreExtensionAsPartOfKey = ignoreExtensionAsPartOfKey;
                if (treatAsRoot?.Length > 0)
                {
                    foreach (var item in treatAsRoot)
                    {
                        source.TreatAsRoot.Add(item);
                    }
                }
            });
        }

        /// <summary>
        /// 添加来自 consul 的配置
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder builder,
            Action<ConsulConfigurationSource> configure)
        {
            return builder.Add(configure);
        }
    }
}