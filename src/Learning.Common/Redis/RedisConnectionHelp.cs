using log4net;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Learning.Common.Redis
{
    /// <summary>
    /// ConnectionMultiplexer对象管理帮助类
    /// </summary>
    public static class RedisConnectionHelp
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RedisConnectionHelp));

        private static readonly ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>> ConnectionCache = new ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>>();

        /// <summary>
        /// 缓存获取
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnectionMultiplexer(string connectionString)
            => ConnectionCache.GetOrAdd(connectionString, key => new Lazy<ConnectionMultiplexer>(() => GetManager(connectionString))).Value;

        private static ConnectionMultiplexer GetManager(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("please provider correct redis connection string", nameof(connectionString));
            }
            var connect = ConnectionMultiplexer.Connect(connectionString);
            //注册如下事件
            connect.ConnectionFailed += MuxerConnectionFailed;
            connect.ConnectionRestored += MuxerConnectionRestored;
            connect.ErrorMessage += MuxerErrorMessage;
            connect.ConfigurationChanged += MuxerConfigurationChanged;
            connect.HashSlotMoved += MuxerHashSlotMoved;
            connect.InternalError += MuxerInternalError;

            return connect;
        }

        #region 事件

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e) => Log.Error("MuxerConfigurationChanged Configuration changed: " + e.EndPoint);

        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e) => Log.ErrorFormat("MuxerErrorMessage redisConnectionHelp.OnError:Endpoint:{0}:Message:{1}", e.EndPoint, e.Message);

        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e) => Log.Error($"MuxerConnectionRestored redisConnectionHelp.OnError:Endpoint:{e.EndPoint}", e.Exception);

        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e) => Log.Error($"MuxerConnectionFailed redisConnectionHelp.OnError:Endpoint:{e.EndPoint}", e.Exception);

        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e) => Log.Error("MuxerHashSlotMoved HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);

        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e) => Log.Error($"MuxerInternalError redisConnectionHelp.OnError:Endpoint:{e.EndPoint}", e.Exception);

        #endregion 事件
    }
}
