using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using SpectroscopyVisualizer.Controllers;
using SpectroscopyVisualizer.Presenters;

namespace SpectroscopyVisualizer.Consumers
{
    public class ParallelSpectroscopyVisualizer<T> : UiConsumer<double[]> where T : ISpectrum
    {
        private const string Lock = "lock";
        private double[] _dummyAxis;
        private int _refreshCnt;

        public ParallelSpectroscopyVisualizer(
            BlockingCollection<double[]> blockingQueue,
            CanvasView view,
            List<SequentialAccumulator<T>> accumulators,
            DisplayAdapter adapter,
            Camera<T> camera)
            : base(blockingQueue, view, adapter)
        {
            Accumulators = accumulators;
            Camera = camera;
        }

        public List<SequentialAccumulator<T>> Accumulators { get; }

        [CanBeNull]
        public ISpectrum SumSpectrum { get; protected set; }

        public Camera<T> Camera { get; }

        public override bool Save
        {
            get { return Camera.IsOn; }
            set { Camera.IsOn = value; }
        }

        public override void Reset()
        {
            base.Reset();
            // todo potential bug
        }


        public override void Consume()
        {
            SumSpectrum?.Clear();

            IsOn = true;
            Task.Run(() =>
            {
                Parallel.ForEach(Accumulators, accumulator =>
                {
                    while (IsOn)
                    {
                        double[] raw;
                        if (!BlockingQueue.TryTake(out raw, MillisecondsTimeout)) break;
                        if (!IsOn) return;
                        if (ConsumeElement(raw, accumulator))
                        {
                            lock (this)
                            {
                                ContinuousFailCnt = 0;
                                ConsumedCnt++;
                                FireConsumeEvent();
                            }
                        }
                        else
                        {
                            lock (this)
                            {
                                ContinuousFailCnt++;
                                if (ContinuousFailCnt >= 10)
                                {
                                    FireFailEvent();
                                    //                            Application.Current.Dispatcher.InvokeAsync(()=>onConsumeFailed?.Invoke());
                                    break;
                                }
                            }
                        }
                    }
                    IsOn = false;
                });
            });
        }

        public override bool ConsumeElement(double[] item)
        {
            throw new NotImplementedException();
        }

        public bool ConsumeElement([NotNull] double[] item, Accumulator<T> accumulator)
        {
            var elementSpectrum = accumulator.Accumulate(item);
            if (elementSpectrum == null)
            {
                return false;
            }
            lock (Lock)
            {
                if (SumSpectrum == null)
                {
                    SumSpectrum = elementSpectrum.Clone();
                }
                else
                {
                    SumSpectrum.TryAbsorb(elementSpectrum);
                }
            }
            OnDataUpdatedInBackground(elementSpectrum);
            return true;
        }

        protected void OnDataUpdatedInBackground([NotNull] T single)
        {
            var averAll = Adapter.SampleAverageAndSquare(SumSpectrum);
            var averSingle = Adapter.SampleAverageAndSquare(single);

            if (Camera.IsOn) Camera.Capture(single);

            _dummyAxis = _dummyAxis ?? Axis.DummyAxis(averAll);
            View.InvokeAsync(() =>
            {
                var averSinglePts = Adapter.CreateGraphPoints(_dummyAxis, averSingle);
                var averAllPts = Adapter.CreateGraphPoints(_dummyAxis, averAll);
                // todo: why does lines above must be exec in the UI thread??
                View.ClearWaveform();
                //                View.Canvas.Children.RemoveRange(0,2);
                View.DrawWaveform(averSinglePts, Colors.Red);
                View.DrawWaveform(averAllPts);
                // todo: bug: buffer cleared while closure continues
                _refreshCnt++;
            }
                );
        }
    }
}