using Autofac;
using Learning.Common.Const;
using Learning.Common.Settings;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Autofac.AttributeExtensions;

namespace Learning.Repositories
{
    /// <summary>
    /// 仓储模块
    /// </summary>
    public class RepositoriesModule : Module
    {
        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAttributedClasses(ThisAssembly);

            builder.Register(context =>
            {
                var settings = context.Resolve<IOptionsMonitor<AppSettings>>().CurrentValue;

                var dbConnection = settings.DbSettings.LearningDbConnection;
                return new MySqlConnection(dbConnection);
            }).AsImplementedInterfaces()
            .AsSelf()
            .InstancePerLifetimeScope()
            .Keyed<IDbConnection>(AutofacKeyedConst.LearningDb);
        }
    }
}
