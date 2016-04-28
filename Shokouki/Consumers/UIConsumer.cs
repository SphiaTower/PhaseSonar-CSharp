using System.Collections.Concurrent;
using System.Windows.Controls;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    public abstract class UiConsumer<T> : Consumer<T>
    {
        protected UiConsumer(BlockingCollection<T> blockingQueue, IScopeView view, DisplayAdapter adapter) : base(blockingQueue)
        {
            View = view;
            Adapter = adapter;
            Axis = new AxisBuilder(View); // todo bug
        }

        public IScopeView View { get;  }
        public DisplayAdapter Adapter { get; }
        protected AxisBuilder Axis { get; }
    }
}