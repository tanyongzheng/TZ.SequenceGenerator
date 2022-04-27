using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TZ.RedisSequence;
using TZ.SequenceGenerator;

namespace Demo
{
    class Program
    {
        private static RedisSequenceConfigOptions options = new RedisSequenceConfigOptions();

        private static bool _isInitStartSequence = false;
        private static readonly object LockObj = new object();

        static async Task Main(string[] args)
        {
            // if StackExchange throw Error "Timeout performing ....."  Set Min Threads
            //https://stackexchange.github.io/StackExchange.Redis/Timeouts

            ThreadPool.SetMinThreads(200, 200);
            options.SetConfig();
            /*
            for (var i = 0; i < 50; i++)
            {
                Task.Run(() => {
                    GetSequence(100);
                });
            }
            */
            ISequenceService sequenceService = new RedisSequenceService(options);
            var sequenceKey = "OrderNum";
            if (!_isInitStartSequence)
            {
                sequenceService.InitStartSequence(sequenceKey, 1);
                _isInitStartSequence = true;
            }
            sequenceService.ResetAscending(sequenceKey);

            for (var i = 0; i < 50; i++)
            {
                await Task.Run(async() => {
                    await GetSequenceAsync(100);
                });
            }
            Console.ReadKey();
        }

        static void GetSequence(int count)
        {
            ISequenceService sequenceService = new RedisSequenceService(options);
            var sequenceKey = "PayCode";
            Stopwatch watch = new Stopwatch();
            var dateTimeFormat = "yyMMdd";
            watch.Start();
            for (var i = 0; i < count; i++)
            {
                var sequence = sequenceService.GetDaySequence(sequenceKey,dateTimeFormat);
                //Console.WriteLine($"第{(i + 1)}sequence:{sequence}");
                Console.WriteLine(sequence);
            }
            watch.Stop();
            Console.WriteLine($"总用时{watch.ElapsedMilliseconds}毫秒");
        }

        static async Task GetSequenceAsync(int count)
        {
            ISequenceService sequenceService = new RedisSequenceService(options);
            var sequenceKey = "OrderNum";
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < count; i++)
            {
                var sequence = await sequenceService.GetSequenceAsync(sequenceKey);
                //Console.WriteLine($"第{(i + 1)}sequence:{sequence}");
                Console.WriteLine(sequence);
            }
            watch.Stop();
            Console.WriteLine($"总用时{watch.ElapsedMilliseconds}毫秒");
        }

    }
}
