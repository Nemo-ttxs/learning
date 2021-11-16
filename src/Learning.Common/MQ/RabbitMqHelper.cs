using EasyNetQ;
using EasyNetQ.Topology;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace Learning.Common.MQ
{
    /// <summary>
    ///  RabbitMQ Helper
    /// </summary>
    public sealed class RabbitMqHelper : IDisposable
    {
        private IBus? _instance;
        private readonly ILog _log = LogManager.GetLogger(typeof(RabbitMqHelper));

        private static ConcurrentDictionary<string, IExchange> _exchange = new ConcurrentDictionary<string, IExchange>();

        private async Task<IExchange> ExchangeDeclareAsync(string exchangeName, string type)
        {
            IExchange exchange;
            if (!_exchange.TryGetValue(exchangeName, out exchange))
            {
                var bus = _instance.Advanced;
                exchange = await bus.ExchangeDeclareAsync(exchangeName, type);
                _exchange.TryAdd(exchangeName, exchange);
            }
            return exchange;
        }

        private IExchange ExchangeDeclare(string exchangeName, string type)
        {
            IExchange exchange;
            if (!_exchange.TryGetValue(exchangeName, out exchange))
            {
                var bus = _instance.Advanced;
                exchange = bus.ExchangeDeclare(exchangeName, type);
                _exchange.TryAdd(exchangeName, exchange);
            }
            return exchange;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectString"></param>
        public RabbitMqHelper([DisallowNull] string connectString) => _instance = RabbitHutch.CreateBus(connectString);

        #region Fanout
        /// <summary>
        ///  Fanout Publish
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public PubResult FanoutPublish<T>(T message, string exchangeName) where T : class => FanoutPublishMessage(new Message<T>(message), exchangeName);


        /// <summary>
        ///  Fanout Publish
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public PubResult FanoutPublishMessage<T>(IMessage<T> message, string exchangeName) where T : class
        {
            try
            {
                if (_instance == null)
                {
                    return PubResult.Fail("消息队列未初始化");
                }

                var bus = _instance.Advanced;
                var exchange = ExchangeDeclare(exchangeName, ExchangeType.Fanout);
                bus.Publish(exchange, "", false, message);
                return PubResult.Success();
            }
            catch (Exception ex)
            {
                _log.Error("mq发生异常", ex);
                return PubResult.Fail(ex);
            }
        }

        /// <summary>
        ///  Fanout Publish Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public Task<PubResult> FanoutPublishAsync<T>(T message, string exchangeName) where T : class => FanoutPublishMessageAsync(new Message<T>(message), exchangeName);

        /// <summary>
        ///  Fanout Publish Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public async Task<PubResult> FanoutPublishMessageAsync<T>(IMessage<T> message, string exchangeName) where T : class
        {
            try
            {
                if (_instance == null)
                {
                    return PubResult.Fail("消息队列未初始化");
                }

                var bus = _instance.Advanced;
                var exchange = await ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout);
                await bus.PublishAsync(exchange, "", false, message);
                return PubResult.Success();
            }
            catch (Exception ex)
            {
                _log.Error("mq发生异常", ex);
                return PubResult.Fail(ex);
            }
        }
        /// <summary>
        ///  Fanout Subscribe 为了避免使用错误导致多起线程，废弃
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callBack"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        [Obsolete]
        public void FanoutConsume<T>(Action<T> callBack, string exchangeName, string queueName) where T : class
        {
            if (_instance == null)
            {
                return;
            }

            var bus = _instance.Advanced;
            var exchange = ExchangeDeclare(exchangeName, ExchangeType.Fanout);
            var queue = bus.QueueDeclare(queueName);
            _ = bus.Bind(exchange, queue, "");
            _ = bus.Consume(queue, result => result.Add<T>((message, exInfo) => callBack(message.Body)));
        }
        /// <summary>
        ///  Fanout Subscribe Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callBack"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        public async Task FanoutConsumeAsync<T>(Func<T, Task> callBack, string exchangeName, string queueName) where T : class
        {
            if (_instance != null)
            {
                var bus = _instance.Advanced;
                var exchange = await ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout);
                var queue = bus.QueueDeclare(queueName);
                _ = await bus.BindAsync(exchange, queue, "");
                _ = bus.Consume(queue, result => result.Add<T>((message, exInfo) => callBack(message.Body)));
            }
        }
        #endregion

        #region Direct
        /// <summary>
        ///  Direct Publish
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey">RouteKey必须完全匹配，才会被队列接收，否则该消息会被抛弃。</param>
        /// <returns></returns>
        public PubResult DirectPublish<T>(T message, string exchangeName, string routingKey) where T : class => DirectPublishMessage(new Message<T>(message), exchangeName, routingKey);

        /// <summary>
        ///  Direct Publish
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey">RouteKey必须完全匹配，才会被队列接收，否则该消息会被抛弃。</param>
        /// <returns></returns>
        public PubResult DirectPublishMessage<T>(IMessage<T> message, string exchangeName, string routingKey) where T : class
        {
            try
            {
                if (_instance == null)
                {
                    return PubResult.Success();
                }

                var bus = _instance.Advanced;
                var exchange = ExchangeDeclare(exchangeName, ExchangeType.Direct);
                bus.Publish(exchange, routingKey, false, message);
                return PubResult.Success();
            }
            catch (Exception ex)
            {
                _log.Error("mq发生异常", ex);
                return PubResult.Fail(ex);
            }
        }
        /// <summary>
        ///  Direct Publish Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey">RouteKey必须完全匹配，才会被队列接收，否则该消息会被抛弃。</param>
        /// <returns></returns>
        public Task<PubResult> DirectPublishAsync<T>(T message, string exchangeName, string routingKey) where T : class => DirectPublishMessageAsync(new Message<T>(message), exchangeName, routingKey);
        /// <summary>
        ///  Direct Publish Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey">RouteKey必须完全匹配，才会被队列接收，否则该消息会被抛弃。</param>
        /// <returns></returns>
        public async Task<PubResult> DirectPublishMessageAsync<T>(IMessage<T> message, string exchangeName, string routingKey) where T : class
        {
            try
            {
                if (_instance == null)
                {
                    return PubResult.Fail("消息队列未初始化");
                }

                var bus = _instance.Advanced;
                var exchange = await ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
                await bus.PublishAsync(exchange, routingKey, false, message);
                return PubResult.Success();
            }
            catch (Exception ex)
            {
                _log.Error("mq发生异常", ex);
                return PubResult.Fail(ex);
            }
        }
        /// <summary>
        ///  Direct Subscribe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callBack"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        public void DirectConsume<T>(Action<T> callBack, string exchangeName, string queueName, string routingKey) where T : class
        {
            if (_instance == null)
            {
                return;
            }

            var bus = _instance.Advanced;
            var exchange = ExchangeDeclare(exchangeName, ExchangeType.Direct);
            var queue = bus.QueueDeclare(queueName);
            _ = bus.Bind(exchange, queue, routingKey);

            _ = bus.Consume(queue, result => result.Add<T>((message, exInfo) => callBack(message.Body)));
        }

        /// <summary>
        ///  Fanout Subscribe Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callBack"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        public async Task DirectConsumeAsync<T>(Func<T, Task> callBack, string exchangeName, string queueName, string routingKey) where T : class
        {
            if (_instance != null)
            {
                var bus = _instance.Advanced;
                var exchange = await ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
                var queue = bus.QueueDeclare(queueName);
                _ = await bus.BindAsync(exchange, queue, routingKey);
                _ = bus.Consume(queue, result => result.Add<T>((message, exInfo) => callBack(message.Body)));
            }
        }
        #endregion

        #region Topic
        /// <summary>
        ///  Topic Publish
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param> 
        /// <returns></returns>
        public PubResult TopicPublish<T>(T message, string exchangeName, string routingKey) where T : class
        {
            try
            {
                if (_instance == null)
                {
                    return PubResult.Fail("消息队列未初始化");
                }

                var bus = _instance.Advanced;
                var exchange = ExchangeDeclare(exchangeName, ExchangeType.Topic);
                bus.Publish(exchange, routingKey, false, new Message<T>(message));
                return PubResult.Success();
            }
            catch (Exception ex)
            {
                _log.Error("mq发生异常", ex);
                return PubResult.Fail(ex);
            }
        }
        /// <summary>
        ///  Topic Publish Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routingKey"></param>
        /// <returns></returns>
        public async Task<PubResult> TopicPublishAsync<T>(T message, string exchangeName, string routingKey) where T : class
        {
            try
            {
                if (_instance == null)
                {
                    return PubResult.Fail("消息队列未初始化");
                }

                var bus = _instance.Advanced;
                var exchange = await ExchangeDeclareAsync(exchangeName, ExchangeType.Topic);
                await bus.PublishAsync(exchange, routingKey, false, new Message<T>(message));
                return PubResult.Success();
            }
            catch (Exception ex)
            {
                _log.Error("mq发生异常", ex);
                return PubResult.Fail(ex);
            }
        }

        /// <summary>
        ///  Topic Subscribe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callBack"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKeys"></param>
        public void TopicoutConsume<T>(Action<T> callBack, string exchangeName, string queueName, string[] routingKeys) where T : class
        {
            if (_instance == null)
            {
                return;
            }

            var bus = _instance.Advanced;
            var exchange = ExchangeDeclare(exchangeName, ExchangeType.Topic);
            var queue = bus.QueueDeclare(queueName);
            foreach (var item in routingKeys)
            {
                _ = bus.Bind(exchange, queue, item);
            }
            _ = bus.Consume(queue, result => result.Add<T>((message, exInfo) => callBack(message.Body)));
        }

        /// <summary>
        ///  Topic Subscribe Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callBack"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKeys"></param>
        public async Task TopicConsumeAsync<T>(Func<T, Task> callBack, string exchangeName, string queueName, string[] routingKeys) where T : class
        {
            if (_instance != null)
            {
                var bus = _instance.Advanced;
                var exchange = await ExchangeDeclareAsync(exchangeName, ExchangeType.Topic);
                var queue = bus.QueueDeclare(queueName);
                foreach (var item in routingKeys)
                {
                    _ = await bus.BindAsync(exchange, queue, item);
                }
                _ = bus.Consume(queue, result => result.Add<T>((message, exInfo) => callBack(message.Body)));
            }
        }
        #endregion

        #region
        /// <summary>
        ///  Direct 发送延时消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">消息</param>
        /// <param name="deadLetterExchangeName">转向exchange</param>
        /// <param name="queueName">对象名</param>
        /// <param name="routingKey">路由</param>
        /// <param name="exchangeName">转向exchange</param>
        /// <returns></returns>
        public async Task<PubResult> DirectPublishDelayedMessageAsync<T>(IMessage<T> message, string exchangeName, string deadLetterExchangeName, string queueName, string routingKey) where T : class
        {
            try
            {
                if (_instance == null)
                {
                    return PubResult.Fail("消息队列未初始化");
                }

                var bus = _instance.Advanced;
                var exchange = await ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
                var queue = await bus.QueueDeclareAsync(queueName, deadLetterExchange: deadLetterExchangeName/*deadLetterRoutingKey: "log.error.last",*/);
                _ = await bus.BindAsync(exchange, queue, routingKey);
                await bus.PublishAsync(exchange, routingKey, false, message);

                return PubResult.Success();
            }
            catch (Exception ex)
            {
                _log.Error("mq发生异常", ex);
                return PubResult.Fail(ex);
            }
        }

        /// <summary>
        ///  Direct 发送延时消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">消息</param>
        /// <param name="deadLetterExchangeName">转向exchange</param>
        /// <param name="routingKey">路由</param>
        /// <param name="exchangeName">exchange</param>
        /// <returns></returns>
        public Task<PubResult> DirectPublishDelayedMessageAsync<T>(IMessage<T> message, string exchangeName, string deadLetterExchangeName, string routingKey) where T : class => DirectPublishDelayedMessageAsync(message, exchangeName, deadLetterExchangeName, $"{exchangeName}.queue", routingKey);

        /// <summary>
        ///  Direct 发送延时消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expiration">过期时间(毫秒，如：1000)</param>
        /// <param name="message">消息</param>
        /// <param name="deadLetterExchangeName">转向exchange</param>
        /// <param name="routingKey">路由</param>
        /// <param name="exchangeName">exchange</param>
        /// <returns></returns>
        public Task<PubResult> DirectPublishDelayedMessageAsync<T>(T message, string expiration, string exchangeName, string deadLetterExchangeName, string routingKey) where T : class
        {
            var data = new Message<T>(message);
            data.Properties.Expiration = expiration;
            return DirectPublishDelayedMessageAsync(data, exchangeName, deadLetterExchangeName, routingKey);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.Dispose();
            _instance = null;
        }
    }
}
