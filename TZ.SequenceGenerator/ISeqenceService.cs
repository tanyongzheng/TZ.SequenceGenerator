using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TZ.SequenceGenerator
{
    public interface ISequenceService
    {
        /// <summary>
        /// 初始化起始序列
        /// Redis Id算法使用
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <param name="startSequence">起始序列</param>
        /// <param name="expiry">过期时间，不填则永不过期，适用于按天、按月做序列</param>
        /// <param name="skipInitialized">是否跳过已初始化（默认跳过，防止重启多次执行）</param>
        /// <returns></returns>
        void InitStartSequence(string sequenceKey, long startSequence, TimeSpan? expiry = null, bool skipInitialized = true);

        /// <summary>
        /// 重新设置升序
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        void ResetAscending(string sequenceKey);

        /// <summary>
        /// 获取序列
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <returns></returns>
        long GetSequence(string sequenceKey);

        /// <summary>
        /// 异步获取序列
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<long> GetSequenceAsync(string sequenceKey, CancellationToken token = default);

        /// <summary>
        /// 按天获取序列
        /// </summary>
        /// <param name="sequenceKeyPrefix">序列键前缀</param>
        /// <param name="dateTimeFormat">时间格式</param>
        /// <returns></returns>
        long GetDaySequence(string sequenceKeyPrefix, string dateTimeFormat = "yyyyMMdd");

        /// <summary>
        /// 异步按天获取序列
        /// </summary>
        /// <param name="sequenceKeyPrefix">序列键前缀</param>
        /// <param name="dateTimeFormat">时间格式</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<long> GetDaySequenceAsync(string sequenceKeyPrefix, string dateTimeFormat = "yyyyMMdd",
            CancellationToken token = default);
    }
}
