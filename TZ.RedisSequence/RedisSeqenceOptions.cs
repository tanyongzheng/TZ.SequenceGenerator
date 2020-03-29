using System;
using System.Collections.Generic;
using System.Text;

namespace TZ.RedisSequence
{
    public class RedisSequenceOptions
    {
        public int DefaultDatabase { get; set; }

        public int DaySequenceExpiry { get; set; }

        public List<RedisConfig> RedisConfigList { get; set; }
    }

    public class RedisConfig
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Password { get; set; }
    }
}
