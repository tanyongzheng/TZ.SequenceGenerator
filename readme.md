# TZ.SequenceGenerator

[![nuget](https://img.shields.io/nuget/v/TZ.SequenceGenerator.svg?style=flat-square)](https://www.nuget.org/packages/TZ.SequenceGenerator) 
[![stats](https://img.shields.io/nuget/dt/TZ.SequenceGenerator.svg?style=flat-square)](https://www.nuget.org/stats/packages/TZ.SequenceGenerator?groupby=Version)
[![License](https://img.shields.io/badge/license-Apache2.0-blue.svg)](https://github.com/tanyongzheng/TZ.SequenceGenerator/blob/master/LICENSE)
![.NETStandard](https://img.shields.io/badge/.NETStandard-%3E%3D2.0-green.svg)

## 介绍
分布式序号生成器（目前只做了基于Redis的实现）适用于序号类型（序号Key）固定，用于提前初始化（如按日期或月可以做定时任务来提前初始化，或程序启动时初始化），且不严格要求连续性（有小概率序号不完整，如多redis实例其中一个出故障则可能会跳过某一序号，如1，2，3，6，7，8）。

主要功能：
1. 获取序号
2. 按天获取序号

## Redis序号生成器使用说明

1. Install-Package TZ.RedisSequence

2. 注入服务：
- 原生配置
```csharp
    // 配置Redis
    services.AddRedisSequence(Configuration);
    // 单例注入
    services.AddSingleton<RedisQueueService>();
```
- abp框架配置
```csharp
    // 配置Redis    
    Configure<RedisSequenceOptions>(configuration.GetSection("RedisSequenceOptions"));
    // 单例注入
    context.Services.AddSingleton<ISequenceService, RedisSequenceService>();
```

3. 配置Redis
```
{
  "RedisIdOptions": {
    "DefaultDatabase": 5,
    "DaySequenceExpiry": 1,
    "RedisConfigList": [
      {
        "Host": "127.0.0.1",
        "Port": 6379,
        "Password": ""
      },
      {
        "Host": "127.0.0.1",
        "Port": 6378,
        "Password": ""
      }
    ]
  }
}

```

4. 初始化
```csharp
    // 获取序号服务
    // ...
    // 初始化序号（默认key不过期，默认跳过已初始化）
    sequenceService.InitStartSequence("SequenceKey", 1,TimeSpan.FromHours(1));
```

5. 使用见项目Demo
