﻿using System.Collections.Concurrent;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    public abstract class UiConsumer<TIn> : Consumer<TIn>
    {
        protected UiConsumer(BlockingCollection<TIn> blockingQueue, IScopeView view, DisplayAdapter adapter)
            : base(blockingQueue)
        {
            View = view;
            Adapter = adapter;
            Axis = new AxisBuilder(View); // todo bug
        }

        public IScopeView View { get; }
        public DisplayAdapter Adapter { get; }
        protected AxisBuilder Axis { get; }
        public abstract bool Save { get; set; }
    }
}