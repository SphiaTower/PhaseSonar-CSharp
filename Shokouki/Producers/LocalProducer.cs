using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows.Threading;
using FTIR.Utils;
using JetBrains.Annotations;

namespace Shokouki.Producers
{
    public class LocalProducer : IProducer
    {
        private readonly string _path;
        public BlockingCollection<double[]> BlockingQueue { get; } = new BlockingCollection<double[]>();

        [CanBeNull]
        public Action OnDataLoadedListener { get; set; }

        public LocalProducer(string path)
        {
            _path = path;
        }

        public void Produce()
        {
            Task.Run(() =>
            {
                var doubles = Toolbox.Read(_path);
                BlockingQueue.Add(doubles);
                OnDataLoadedListener?.Invoke();
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