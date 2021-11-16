using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using Autofac.AttributeExtensions;
using Microsoft.Extensions.Options;
using Learning.Common.Settings;
using Learning.Common.Redis;
using Learning.Common.Const;
using Learning.Common.MQ;

namespace Learning.Services
{
    /// <summary>
    /// 服务模块
    /// </summary>
    public class ServicesModule : Module
    {
        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAttributedClasses(ThisAssembly);

            builder.Register(context =>
            {
                var redisConnection = context.Resolve<IOptionsMonitor<AppSettings>>().CurrentValue.RedisSettings.BussinessRedis;
                return new RedisHelper(0, redisConnection);
            })
            .AsSelf()
            .SingleInstance()
            .Keyed<RedisHelper>(AutofacKeyedConst.BusinessRedis);

            builder.Register(context =>
            {
                var settings = context.Resolve<IOptionsMonitor<AppSettings>>().CurrentValue;
                return new RabbitMqHelper(settings.RabbitMqConnection);
            })
            .AsSelf()
            .SingleInstance()
            .Keyed<RabbitMqHelper>(AutofacKeyedConst.BusinessMQ);
        }
    }
}
