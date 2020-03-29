using System;
using System.Collections.Generic;
using System.Text;

namespace TZ.SequenceGenerator
{
    public interface ISequenceService
    {
        /// <summary>
        /// 初始化起始Id
        /// Redis Id算法使用
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <param name="startSequence">起始序列</param>
        /// <param name="expiry">过期时间，不填则永不过期，适用于按天、按月做序列</param>
        /// <returns></returns>
        void InitStartSequence(string sequenceKey, long startSequence, TimeSpan? expiry = null);

        /// <summary>
        /// 获取序列
        /// </summary>
        /// <param name="sequenceKey">序列键名</param>
        /// <returns></returns>
        long GetSequence(string sequenceKey);

        /// <summary>
        /// 按天获取序列
        /// </summary>
        /// <param name="sequenceKeyPrefix">序列键前缀</param>
        /// <param name="dateTimeFormat">时间格式</param>
        /// <returns></returns>
        long GetDaySequence(string sequenceKeyPrefix, string dateTimeFormat = "yyyyMMdd");
    }
}
