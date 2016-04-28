using System;
using System.Collections.Generic;
using System.Linq;
using FTIR.Analyzers;
using FTIR.Utils;

namespace FTIR
{
    internal class Program
    {
        private static void Main(string[] args)
        {
         /*   var watch = new StopWatch();
            var data = Toolbox.Toolbox.Read(@"D:\data\test\full test\5-3.txt");
            //var data = Toolbox.Read(@"D:\database\test\part-2-7.txt");
            watch.Reset("load");
            var splitter = Splitter.DefaultSplitter(1250);
            watch.Reset("create splitter");
            var parallelResult = splitter.Split(data);
            watch.Reset("correct");
            Toolbox.Toolbox.WriteData(
                @"D:\database\test\full test\5-3-csharp-parallel-n" + parallelResult.Source.PeriodCnt + ".txt",
                parallelResult.Source.Spec);
            watch.Reset("write");*/
        }

        private static void Test(IReadOnlyList<double> result)
        {
            var std = Utils.Toolbox.Read(@"D:\database\test\full test\sum-corr-spec-5-3-n201.txt");
            //var std = Toolbox.Read(@"D:\database\test\old result\result.txt");
            for (var i = 0; i < 100; i++)
            {
                Console.WriteLine(result[i] - std[i]);
            }
            Console.WriteLine(@"sum");
            Console.WriteLine(result.Sum());
            Console.WriteLine(std.Sum());
        }
    }
}