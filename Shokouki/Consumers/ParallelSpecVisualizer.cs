using System.Collections.Concurrent;
using System.Threading.Tasks;
using FTIR.Analyzers;
using FTIR.Maths;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    public class ParallelSpecVisualizer : SpectroscopyVisualizer
    {
        private static readonly object Latch = new object(); // todo unusable

        public ParallelSpecVisualizer(BlockingCollection<double[]> blockingQueue, IScopeView view,
            Accumulator accumulator, DisplayAdapter adapter) : base(blockingQueue, view, accumulator, adapter)
        {
        }

        public override void Consume()
        {
            if (Accumulated != null)
            {
                Funcs.Clear(Accumulated);
            }
            /* else
            {
                Accumulated = new double[];
            }*/
            IsOn = true;
            Parallel.For(0, 4, i =>
            {
                while (IsOn)
                {
                    double[] raw;
                    if (!BlockingQueue.TryTake(out raw, MillisecondsTimeout)) return;
                    if (!IsOn) return;
                    ConsumeElement(raw);
                }
            });
        }

        public override void ConsumeElement(double[] item)
        {
            var result = Accumulator.Accumulate(item);
            if (result == null)
            {
                return;
            }
            Accumulated = Accumulated ?? new double[result.Spec.Length];
            lock (Latch)
            {
                Funcs.AddTo(Accumulated, result.Spec);
                PulseCnt += result.PeriodCnt;
                ConsumedCnt++;
                OnDataUpdatedInBackground(result);
            }
        }
    }
}