﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PhaseSonar.Analyzers.WithoutReference;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
    public class TrueResult : IResult {
        public bool IsSuccessful { get; } = true;
        public bool HasException { get; } = false;
        public ProcessException? Exception { get; }
        public int ExceptionCnt { get; }
        public int ValidPeriodCnt { get; }
    }

    public class DataSerializer : IConsumerV2 {
        private readonly ParallelConsumerV2<SampleRecord, SpecialSampleWriter, TrueResult> _consumer;

        private readonly TrueResult _result = new TrueResult();

        /// <summary>
        ///     Create a Consumer.
        /// </summary>
        /// <param name="blockingQueue">The queue which containing elements to be consumed</param>
        /// <param name="workers">A collection of workers consuming the products in parallel.</param>
        /// <param name="targetCnt"></param>
        public DataSerializer([NotNull] BlockingCollection<SampleRecord> blockingQueue,
            [NotNull] IEnumerable<SpecialSampleWriter> workers, int? targetCnt) {
            _consumer = new ParallelConsumerV2<SampleRecord, SpecialSampleWriter, TrueResult>(blockingQueue, workers,
                ConsumeElement,
                result => { }, 2000, targetCnt);
        }

        public Dictionary<ProcessException, int> Exceptions { get; } = new Dictionary<ProcessException, int>();

        /// <summary>
        ///     The number of elements have been consumed.
        /// </summary>
        public int ConsumedCnt => _consumer.ConsumedCnt;

        public int? TargetCnt => _consumer.TargetCnt;

        /// <summary>
        ///     Stop consuming.
        /// </summary>
        public void Stop() {
            _consumer.Stop();
        }

        /// <summary>
        ///     Start consuming.
        /// </summary>
        public void Start() {
            _consumer.Start();
        }

        public event Action SourceInvalid {
            add { _consumer.SourceInvalid += value; }
            remove { _consumer.SourceInvalid -= value; }
        }


        public event UpdateEventHandler Update {
            add { _consumer.Update += value; }
            remove { _consumer.Update -= value; }
        }


        public event Action ProducerEmpty {
            add { _consumer.ProducerEmpty += value; }
            remove { _consumer.ProducerEmpty -= value; }
        }

        public event Action TargetAmountReached {
            add { _consumer.TargetAmountReached += value; }
            remove { _consumer.TargetAmountReached -= value; }
        }

        /// <summary>
        ///     Consume element in a branch.
        /// </summary>
        /// <param name="element">The product</param>
        /// <param name="worker">The worker in this branch.</param>
        /// <returns>Whether consuming succeeds.</returns>
        private TrueResult ConsumeElement(SampleRecord element, SpecialSampleWriter worker) {
            worker.Write(element);
            return _result;
        }
    }

    public class SpecialSampleWriter {
        protected const string Suffix = ".txt";

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SpecialSampleWriter(string directory, string prefix) {
            BasePath = Path.Combine(directory, prefix);
        }

        public string BasePath { get; }

        public void Write(SampleRecord record) {
            Toolbox.SerializeData(BasePath + record.Id.Enclose("No") + Suffix, record.PulseSequence);
        }
    }
}