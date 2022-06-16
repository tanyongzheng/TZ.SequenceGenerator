using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TZ.SequenceGenerator;

namespace TZ.RedisSequence
{
    public partial class RedisSequenceService:ISequenceService
    {
        private static int callCount = 0;
        private static List<ConnectionMultiplexer> redisList;
        private static readonly object lockObj = new object();
        private static readonly object getRedisLockObj = new object();
        
        //递增长度
        private static int increment = 0;

        private readonly RedisSequenceOptions _sequenceOptions;

        /// <summary>
        /// 程序中最好使用单例模式
        /// 如要新建对象实例，请在程序最开始的地方先实例化一个对象
        /// </summary>
        /// <param name="options"></param>
        public RedisSequenceService(IOptions<RedisSequenceOptions> options)
        {
            if (options == null || options.Value == null)
            {
                throw new Exception("please set RedisSequenceOptions!");
            }
            else if (options.Value != null)
            {
                _sequenceOptions = options.Value;
            }
            if (_sequenceOptions == null)
            {
                throw new Exception("please set RedisSequenceOptions!");
            }
            if (_sequenceOptions.DefaultDatabase < 0)
            {
                throw new Exception("please set RedisSequenceOptions->DefaultDatabase !");
            }
            if (_sequenceOptions.RedisConfigList == null || _sequenceOptions.RedisConfigList.Count == 0)
            {
                throw new Exception("please set RedisSequenceOptions->RedisConfigList!");
            }
            var sameHost = _sequenceOptions.RedisConfigList.GroupBy(x => x.Host + ":" + x.Port).Where(x => x.Count() > 1).ToList();
            if (sameHost.Any())
            {
                throw new Exception($"have same host [{sameHost.FirstOrDefault().Key}] RedisSequenceOptions->RedisConfigList!");
            }

            if (redisList == null || redisList.Count == 0)
            {
                lock (lockObj)
                {
                    if (redisList == null || redisList.Count == 0)
                    {
                        //初始化redis列表
                        InitRedisList();
                    }
                }
            }
        }


        #region Base Method
        /// <summary>
        /// 初始化起始序列
        /// Redis Sequence算法使用
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <param name="startSequence">起始序列</param>
        /// <param name="expiry">过期时间，不填则永不过期，适用于按天、按月做序列</param>
        /// <param name="skipInitialized">是否跳过已初始化（默认跳过，防止重启多次执行）</param>
        /// <returns></returns>
        public void InitStartSequence(string sequenceKey, long startSequence, TimeSpan? expiry=null, bool skipInitialized = true)
        {
            lock (lockObj)
            {
                // 判断是否已初始化
                var hasInit = true;

                for (var i = 0; i < redisList.Count; i++)
                {
                    var redis = redisList[i];
                    if (!redis.GetDatabase().KeyExists(sequenceKey))
                    {
                        hasInit = false;
                        break;
                    }
                }
                
                //已初始化跳过
                if (hasInit&& skipInitialized)
                {
                    return;
                }

                //未初始化且开始序号为1
                if (!hasInit&& startSequence==1)
                {
                    for (var i=0;i<redisList.Count;i++)
                    {
                        var initSequence = i - increment + 1;
                        var redis = redisList[i];
                        if (expiry.HasValue)
                            redis.GetDatabase().StringSet(sequenceKey, initSequence, expiry.Value);
                        else
                            redis.GetDatabase().StringSet(sequenceKey, initSequence);
                    }
                    return;
                }

                startSequence = startSequence - increment + 1;
                foreach (var redis in redisList)
                {
                    if (expiry.HasValue)
                        redis.GetDatabase().StringSet(sequenceKey, startSequence,expiry.Value);
                    else
                        redis.GetDatabase().StringSet(sequenceKey, startSequence);
                    startSequence++;
                }
            }
        }

        /// <summary>
        /// 重新设置升序（补齐后面Redis顺序错乱的序号，调用必须放到初始化起始序列方法后面）
        /// 多个Redis连接，如程序重启导致后面的序号小于前面的，则需要调用该方法（适用于单个程序）
        /// 多程序共用要保持升序该类库不适用（加分布式锁或单独部署一个序号生成器程序，或者只用一个Redis实例）
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <returns></returns>
        public void ResetAscending(string sequenceKey)
        {
            lock (lockObj)
            {
                long maxSequence = 0;
                int maxSequenceRedisIndex = -1;
                for (var i=0;i<redisList.Count;i++)
                {
                    var redis = redisList[i];
                    var sequenceStr =redis.GetDatabase().StringGet(sequenceKey);
                    if (sequenceStr.IsNull)
                    {
                        throw new Exception("序号未初始化，请先调用初始化方法");
                    }

                    if(!sequenceStr.TryParse(out long currentSequence))
                    {
                        throw new Exception($"第{i+1}个Redis的[{sequenceKey}]序号是非数字");
                    }

                    if(currentSequence>maxSequence)
                    {
                        maxSequence = currentSequence;
                        maxSequenceRedisIndex = i;
                    }
                }

                if (maxSequenceRedisIndex == -1)
                {
                    return;
                }

                var startIndex = maxSequenceRedisIndex + 1;
                if (startIndex == redisList.Count) return;

                var startSequence = maxSequence;
                for (var i = startIndex; i < redisList.Count; i++)
                {
                    startSequence += 1;
                    var redis = redisList[i];
                    redis.GetDatabase().StringSet(sequenceKey, startSequence);
                }
            }
        }

        /// <summary>
        /// 获取序列
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <returns></returns>
        public long GetSequence(string sequenceKey)
        {
            if (string.IsNullOrEmpty(sequenceKey))
            {
                throw new ArgumentNullException($"please set param {nameof(sequenceKey)}!");
            }
            lock (lockObj)
            {
                var redis = GetConnectionMultiplexer();
                //调用Redis的INCRBY递增命令
                var sequence = redis.GetDatabase().StringIncrement(sequenceKey, increment);
                return sequence;
            }
        }
        #endregion

        #region Extend Method
        /// <summary>
        /// 按天获取序列
        /// </summary>
        /// <param name="sequenceKeyPrefix">序列键前缀</param>
        /// <param name="dateTimeFormat">时间格式</param>
        /// <returns></returns>
        public long GetDaySequence(string sequenceKeyPrefix,string dateTimeFormat= "yyyyMMdd")
        {
            if (string.IsNullOrEmpty(sequenceKeyPrefix))
            {
                throw new ArgumentNullException($"please set param {nameof(sequenceKeyPrefix)}!");
            }
            var sequenceKey = sequenceKeyPrefix+DateTime.Now.ToString(dateTimeFormat);
            var startSequence = 0;
            lock (lockObj)
            {
                var redis = GetConnectionMultiplexer();
                if (!redis.GetDatabase().KeyExists(sequenceKey)&&_sequenceOptions.DaySequenceExpiry>0)
                {
                    var callRedisIndex = redisList.IndexOf(redis);
                    startSequence = startSequence + callRedisIndex - increment + 1;
                    TimeSpan expiry = TimeSpan.FromDays(_sequenceOptions.DaySequenceExpiry);
                    redis.GetDatabase().StringSet(sequenceKey, startSequence, expiry);
                }
                //调用Redis的INCRBY递增命令
                var sequence = redis.GetDatabase().StringIncrement(sequenceKey, increment);
                return sequence;
            }
        }
        #endregion

        #region private method
        
        /// <summary>
        /// 获取redis连接
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetConnectionMultiplexer()
        {
            lock (getRedisLockObj)
            {
                if (callCount == int.MaxValue)
                    callCount = 0;
                //按调用次数平均分配到每个Redis服务器
                int callRedisIndex = callCount % redisList.Count;
                var redis = redisList[callRedisIndex];
                callCount++;
                return redis;
            }
        }

        /// <summary>
        /// 初始化Redis连接集合
        /// </summary>
        private void InitRedisList()
        {
            redisList = new List<ConnectionMultiplexer>();
            foreach (var redisConfig in _sequenceOptions.RedisConfigList)
            {
                ConfigurationOptions configurationOptions = new ConfigurationOptions();
                configurationOptions.AbortOnConnectFail = false;//超时不重试
                configurationOptions.EndPoints.Add(redisConfig.Host, redisConfig.Port);
                if (!string.IsNullOrEmpty(redisConfig.Password))
                    configurationOptions.Password = redisConfig.Password;
                configurationOptions.DefaultDatabase = _sequenceOptions.DefaultDatabase;
                //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("server1:6379,server2:6379,abortConnect= false");
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);
                redisList.Add(redis);
            }
            increment = redisList.Count;
        }

        #endregion
    }
}

