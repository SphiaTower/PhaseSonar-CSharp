using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Producers
{
    public class LocalProducer : IProducer<SampleRecord> // TODO!!!!
    {
        private readonly IEnumerable<string> _paths;


        public LocalProducer(IEnumerable<string> paths)
        {
            _paths = paths;
        }

        [CanBeNull]
        public Action OnDataLoadedListener { get; set; }

        public BlockingCollection<SampleRecord> BlockingQueue { get; } = new BlockingCollection<SampleRecord>();

        int IProducer<SampleRecord>.HistoryProductCnt
        {
            get { throw new NotImplementedException(); }
        }

        
        public void Produce()
        {
            Task.Run(() =>
            {
                foreach (var path in _paths)
                {
                    var doubles = Toolbox.Read(path);
                    BlockingQueue.Add(new SampleRecord(doubles, 0));
                    if (OnDataLoadedListener != null)
                    {
                        Application.Current.Dispatcher.Invoke(OnDataLoadedListener);
                    }
                }
            });
        }

        public void Stop()
        {
        }


        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}