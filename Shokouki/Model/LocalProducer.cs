using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FTIR.Utils;

namespace Shokouki.Model
{
    public class LocalProducer : IProducer
    {
        public BlockingCollection<double[]> BlockingQueue { get; } = new BlockingCollection<double[]>();

        public void Produce()
        {
            Task.Run(() =>
            {
                var doubles = Toolbox.Read(@"C:\Buffer\1232123.txt");
                BlockingQueue.Add(doubles);
            });
        }

        public void Stop()
        {
        }

        int IProducer.HistoryProductCnt { get; }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public int HistoryProductCnt()
        {
            throw new NotImplementedException();
        }
    }
}