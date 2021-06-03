using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace TZ.RedisSequence
{
    public partial class RedisSequenceService
    {
        private static readonly SemaphoreSlim SyncSemaphore = new SemaphoreSlim(1, 1);

        #region Base Method
        /// <summary>
        /// 异步获取序列
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<long> GetSequenceAsync(string sequenceKey, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(sequenceKey))
            {
                throw new ArgumentNullException($"please set param {nameof(sequenceKey)}!");
            }
            using (await SyncSemaphore.LockAsync(token))
            {
                var redis = GetConnectionMultiplexer();
                //调用Redis的INCRBY递增命令
                var sequence = await redis.GetDatabase().StringIncrementAsync(sequenceKey, increment);
                return sequence;
            }
        }
        #endregion


        #region Extend Method

        /// <summary>
        /// 异步按天获取序列
        /// </summary>
        /// <param name="sequenceKeyPrefix">序列键前缀</param>
        /// <param name="dateTimeFormat">时间格式</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<long> GetDaySequenceAsync(string sequenceKeyPrefix, string dateTimeFormat = "yyyyMMdd", CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(sequenceKeyPrefix))
            {
                throw new ArgumentNullException($"please set param {nameof(sequenceKeyPrefix)}!");
            }
            var sequenceKey = sequenceKeyPrefix + DateTime.Now.ToString(dateTimeFormat);
            var startSequence = 0;
            using (await SyncSemaphore.LockAsync(token))
            {
                var redis = GetConnectionMultiplexer();
                if (!await redis.GetDatabase().KeyExistsAsync(sequenceKey) && _sequenceOptions.DaySequenceExpiry > 0)
                {
                    var callRedisIndex = redisList.IndexOf(redis);
                    startSequence = startSequence + callRedisIndex - increment + 1;
                    TimeSpan expiry = TimeSpan.FromDays(_sequenceOptions.DaySequenceExpiry);
                    await redis.GetDatabase().StringSetAsync(sequenceKey, startSequence, expiry);
                }
                //调用Redis的INCRBY递增命令
                var sequence =await redis.GetDatabase().StringIncrementAsync(sequenceKey, increment);
                return sequence;
            }
        }
        #endregion

    }
}
