using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;

namespace Microsoft.Extensions.Configuration.Consul
{
    /// <summary>
    /// 基于 Consul 的配置提供者
    /// </summary>
    public class ConsulConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly ConsulConfigurationSource _source;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly object _syncRoot = new object();
        private readonly ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim();
        private ulong _lastIndex;
        private Task? _pollTask;
        private bool _disposed;
        private long _loadTimes;

        /// <summary>
        /// 已经加载了多少次
        /// </summary>
        // ReSharper disable UnusedMember.Global
        public long LoadTimes => _loadTimes;
        // ReSharper restore UnusedMember.Global

        /// <summary>
        /// 初始化一个 基于 Consul 的配置提供者 实例
        /// </summary>
        /// <param name="source"></param>
        public ConsulConfigurationProvider(ConsulConfigurationSource source)
        {
            _source = source;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <inheritdoc />
        public override void Load()
        {
            if (_source.ReloadOnChange)
            {
                if (_pollTask == null)
                {
                    lock (_syncRoot)
                    {
                        if (_pollTask == null)
                        {
                            _pollTask = Task.Run(LongPolling);
                            if (_source.WaitForInitialized)
                            {
                                var timeout = _source.WaitInitializedTimeout;
                                if (!_manualResetEventSlim.Wait(timeout))
                                {
                                    throw new TimeoutException("wait for configuration initialized timeout.");
                                }
                            }
                        }
                    }
                }
                return;
            }
            LoadAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        private async Task LongPolling()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await LoadAsync(true)
                    .ConfigureAwait(false);
            }
        }

        private async Task LoadAsync(bool reload = false)
        {
            try
            {
                var queryOptions = new QueryOptions
                {
                    WaitIndex = reload && _source.ReloadOnChange ? _lastIndex : 0,
                    WaitTime = _source.WatchInterval
                };
                var queryResult = await _source
                    .ConsulClient
                    .KV
                    .List(_source.ConsulKeyPath, queryOptions, _cancellationTokenSource.Token)
                    .ConfigureAwait(false);
                if (queryResult.StatusCode == HttpStatusCode.OK)
                {
                    if (queryResult.Response.Length > 0)
                    {
                        //changed.
                        if (_lastIndex < queryResult.LastIndex)
                        {
                            Data = queryResult
                                .Response
                                .Where(t => !t.Key.EndsWith("/"))
                                .SelectMany(p =>
                                    {
                                        var relativePath = p.Key.Substring(_source.ConsulKeyPath.Length).Trim('/');
                                        var config = _source.Selector
                                            .Select(relativePath)
                                            .Parse(p);
                                        if (!_source.TreatAsRoot.Contains(relativePath))
                                        {
                                            var elementKey = p.Key.Trim('/', '\\').Trim()
                                                .Replace("\\", ConfigurationPath.KeyDelimiter)
                                                .Replace("/", ConfigurationPath.KeyDelimiter);
                                            elementKey = _source.IgnoreExtensionAsPartOfKey
                                                ? Path.GetFileNameWithoutExtension(elementKey)
                                                : elementKey;
                                            return config
                                                .Select(item => new KeyValuePair<string, string?>(
                                                    ConfigurationPath.Combine(elementKey, item.Key),
                                                    item.Value));
                                        }

                                        return config;
                                    }
                                ).ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);
                            OnReload();
                        }
                    }
#if DEBUG
                    else
                    {
                        _source.Debug("configuration is empty.");
                    }
#endif

                    if (Interlocked.Increment(ref _loadTimes) == 1)
                    {
                        _manualResetEventSlim.Set();
                    }

                    _lastIndex = queryResult.LastIndex;
                }
                else
                {
                    _source.TriggerError("load configuration failed. response status code isn't ok.", null);
                }
            }
            catch (Exception exception)
            {
                _source.TriggerError("load configuration failed. see exception for more details.", exception);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _disposed = true;
        }
    }
}
