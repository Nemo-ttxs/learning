using System;
using System.Collections.Generic;
using System.Text;

namespace Learning.Common.Settings
{
    public class AppSettings
    {
        public DbSettings DbSettings { get; set; }

        public RedisSettings RedisSettings { get; set; }

        public string RabbitMqConnection { get; set; }
    }

    public class DbSettings
    { 
        public string LearningDbConnection { get; set; }
    }

    public class RedisSettings
    {
        public string BussinessRedis { get; set; }
    }
}
