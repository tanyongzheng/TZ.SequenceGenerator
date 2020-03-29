using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace TZ.RedisSequence
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddRedisSequence(this IServiceCollection services, IConfiguration configuration = null)
        {
            if (configuration == null)
            {
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

                configuration = builder.Build();
            }
            services.AddOptions();
            services.Configure<RedisSequenceOptions>(configuration.GetSection(nameof(RedisSequenceOptions)));
            return services;
        }
    }
}
