using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TZ.RedisSequence;

namespace Demo
{
    public class RedisSequenceConfigOptions : IOptions<RedisSequenceOptions>
    {
        private RedisSequenceOptions redisSequenceOptions;
        public RedisSequenceOptions Value
        {
            get
            {
                return redisSequenceOptions;
            }
        }

        public void SetConfig()
        {
            /*
            //在当前目录或者根目录中寻找appsettings.json文件
            var fileName = "appsettings.json";

            var directory = AppContext.BaseDirectory;
            directory = directory.Replace("\\", "/");

            var filePath = $"{directory}/{fileName}";
            if (!File.Exists(filePath))
            {
                var length = directory.IndexOf("/bin");
                filePath = $"{directory.Substring(0, length)}/{fileName}";
            }

            var builder = new ConfigurationBuilder()
                .AddJsonFile(filePath, false, true);

            var configuration = builder.Build();
            redisSequenceOptions = configuration.GetValue<RedisSequenceOptions>(nameof(RedisSequenceOptions));
            if (redisSequenceOptions == null)
            {
                / *
                var jsonStr = File.ReadAllText(filePath);
                var jsonObj = JsonHelper.Deserialize<RedisSequenceOptionsJson>(jsonStr);
                redisSequenceOptions = jsonObj.RedisSequenceOptions;
                * /
            }
            if (redisSequenceOptions == null)
            {
                throw new Exception("redis id config RedisSequenceOptions is null");
            }
            */
            
            redisSequenceOptions = new RedisSequenceOptions();
            redisSequenceOptions.DefaultDatabase = 5;
            redisSequenceOptions.DaySequenceExpiry = 1;
            redisSequenceOptions.RedisConfigList = new List<RedisConfig>();
            var redisConfig1 = new RedisConfig();
            redisConfig1.Host = "127.0.0.1";
            redisConfig1.Port = 6379;
            redisSequenceOptions.RedisConfigList.Add(redisConfig1);
            var redisConfig3 = new RedisConfig();
            redisConfig3.Host = "127.0.0.1";
            redisConfig3.Port = 6378;
            redisSequenceOptions.RedisConfigList.Add(redisConfig3);            
        }

        public class RedisSequenceOptionsJson
        {
            public RedisSequenceOptions RedisSequenceOptions { get; set; }
        }
    }
}
