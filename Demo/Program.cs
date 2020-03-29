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
        static void Main(string[] args)
        {
            // if StackExchange throw Error "Timeout performing ....."  Set Min Threads
            //https://stackexchange.github.io/StackExchange.Redis/Timeouts

            ThreadPool.SetMinThreads(200, 200);
            options.SetConfig();
            for (var i = 0; i < 50; i++)
            {
                Task.Run(() => {
                    GetSequence(100);
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

    }
}
