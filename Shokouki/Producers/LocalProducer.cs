using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Threading;
using FTIR.Utils;
using JetBrains.Annotations;

namespace Shokouki.Producers
{
    public class LocalProducer : IProducer
    {
        public BlockingCollection<double[]> BlockingQueue { get; } = new BlockingCollection<double[]>();
        private readonly IEnumerable<string> _paths;
        [CanBeNull]
        public Action OnDataLoadedListener { get; set; }

     
        public LocalProducer(IEnumerable<string> paths)
        {
            _paths = paths;
        }

        public void Produce()
        {
            Task.Run(() =>
            {
                foreach (var path in _paths)
                {
                    var doubles = Toolbox.Read(path);
                    BlockingQueue.Add(doubles);
                    OnDataLoadedListener?.Invoke();
                }
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