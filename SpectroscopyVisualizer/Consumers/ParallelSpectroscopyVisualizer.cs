using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    ///     A parallel version of <see cref="SerialSpectroscopyVisualizer" /> which dequeues elements parallelly.
    /// </summary>
    public class ParallelSpectroscopyVisualizer : AbstractConsumer<double[]>
    {
        private const string Lock = "lock";
        private double[] _dummyAxis;

        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="blockingQueue">The queue containing all elements to be consumed.</param>
        /// <param name="accumulators">A list of accumulators processing the data sampled parallelly.</param>
        /// <param name="adapter">An adapter for display.</param>
        /// <param name="writer">A Writer for data storage.</param>
        public ParallelSpectroscopyVisualizer(
            BlockingCollection<double[]> blockingQueue,
            List<SerialAccumulator> accumulators,
            DisplayAdapter adapter,
            SpectrumWriter writer)
            : base(blockingQueue)
        {
            Accumulators = accumulators;
            Adapter = adapter;
            Axis = new AxisBuilder(adapter.WavefromView);
            Writer = writer;
        }

        /// <summary>
        ///     A list of accumulators processing the data sampled parallelly.
        /// </summary>
        public List<SerialAccumulator> Accumulators { get; }

        /// <summary>
        ///     The accumulation of spectra.
        /// </summary>
        [CanBeNull]
        public ISpectrum SumSpectrum { get; protected set; }

        /// <summary>
        ///     A Writer for data storage.
        /// </summary>
        public SpectrumWriter Writer { get; }


        private AxisBuilder Axis { get; }

        /// <summary>
        ///     <see cref="DisplayAdapter" />
        /// </summary>
        public DisplayAdapter Adapter { get; set; }

        public CanvasView View => Adapter.WavefromView;

        /// <summary>
        ///     Start consuming.
        /// </summary>
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

        /// <summary>
        ///     This method is not implemented due to the bad design of the class hierarchy.// TODO Rewrite the consumer base
        ///     class.
        /// </summary>
        /// <param name="item">The element</param>
        /// <returns>Consumed successfully or not</returns>
        public override bool ConsumeElement(double[] item)
        {
            throw new NotImplementedException();
        }


        private bool ConsumeElement([NotNull] double[] item, Accumulator accumulator)
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

        /// <summary>
        ///     Called when a new element is processed in the background.
        /// </summary>
        /// <param name="single">The processed spetrum of a newly dequeued element.</param>
        protected void OnDataUpdatedInBackground([NotNull] ISpectrum single)
        {
            var averAll = Adapter.SampleAverageAndSquare(SumSpectrum);
            var averSingle = Adapter.SampleAverageAndSquare(single);

            if (Writer.IsOn) Writer.Enqueue(single);

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
            }
                );
        }
    }
}