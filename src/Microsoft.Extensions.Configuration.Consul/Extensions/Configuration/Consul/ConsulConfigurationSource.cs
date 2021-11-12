using System;
using System.Collections.Generic;
using System.Diagnostics;
using Consul;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// 基于 Consul 的配置源
    /// </summary>
    public class ConsulConfigurationSource : IConfigurationSource
    {
        private TimeSpan _watchInterval = TimeSpan.FromMinutes(1);
        private TimeSpan _waitInitializedTimeout = TimeSpan.FromMinutes(1);

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        /// <summary>
        /// consul 工具
        /// </summary>
        public IConsulClient ConsulClient { get; set; }

        /// <summary>
        /// 配置基础路径
        /// </summary>
        public string ConsulKeyPath { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        /// <summary>
        /// 把哪些元素当成根元素对待，忽略 consul 配置路径。
        /// 不在该列表中的元素在配置项中会以 consul 相对路径作为前缀。
        /// </summary>
        public ISet<string> TreatAsRoot { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 不建议调整默认配置。
        /// 作为键的一部分时候是否忽略后缀。默认 为 true.
        /// 比如 app.json，默认情况 app: 作为前缀，否则 以 app.json: 作为前缀。（除了该文件在<see cref="TreatAsRoot"/>列表中时）
        /// </summary>
        public bool IgnoreExtensionAsPartOfKey { get; set; } = true;

        /// <summary>
        /// 是否要在配置变更时重新加载，默认 false 即默认不监控配置变化。
        /// </summary>
        public bool ReloadOnChange { get; set; } = false;

        /// <summary>
        /// 配置变化同步最新配置时的间隔时间，默认1分钟，在间隔时间内配置发生变更时，将会提前触发变更逻辑。
        /// </summary>
        public TimeSpan WatchInterval
        {
            get => _watchInterval;
            set
            {
                if (value > TimeSpan.Zero)
                {
                    _watchInterval = value;
                }
            }
        }

        /// <summary>
        /// 是否等待配置第一次初始化成功，默认为 true 即默认等待。
        /// </summary>
        public bool WaitForInitialized { get; set; } = true;

        /// <summary>
        /// 等待配置第一次初始化成功超时时间，默认1分钟
        /// </summary>
        public TimeSpan WaitInitializedTimeout
        {
            get => _waitInitializedTimeout;
            set
            {
                if (value > TimeSpan.Zero)
                {
                    _waitInitializedTimeout = value;
                }
            }
        }

        /// <summary>
        /// 配置解析选择器
        /// </summary>
        public IConsulConfigurationParserSelector Selector { get; set; } = new DefaultConsulConfigurationParserSelector();

        /// <summary>
        /// 发生异常时调用
        /// </summary>
        public event Action<string, Exception?>? OnError;

        internal void TriggerError(string message, Exception? exception) => OnError?.Invoke(message, exception);

#if  DEBUG
        /// <summary>
        /// 调试调用
        /// </summary>
        public event Action<string>? OnDebug;

        [Conditional("DEBUG")]
        internal void Debug(string message) => OnDebug?.Invoke(message);
#endif

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            if (ConsulClient == null)
            {
                throw new InvalidOperationException("please provider a proper consul-client");
            }
            if (ConsulKeyPath == null)
            {
                throw new InvalidOperationException("please provider a proper consul-key-path");
            }
            return new ConsulConfigurationProvider(this);
        }
    }
}
