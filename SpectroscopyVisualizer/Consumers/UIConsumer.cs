using System.Collections.Concurrent;
using SpectroscopyVisualizer.Presenters;

namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    /// A consumer encapsulating UI elements. In fact, this design is ugly, and composition should have been used
    /// instead of inheritance. // TODO remove this hierarchy
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public abstract class UiConsumer<TIn> : Consumer<TIn>
    {
        /// <summary>
        /// Create a UI consumer.
        /// </summary>
        /// <param name="blockingQueue">The queue of all elements to be consumed.</param>
        /// <param name="view">The view used to present displays.</param>
        /// <param name="adapter"><see cref="DisplayAdapter"/></param>
        protected UiConsumer(BlockingCollection<TIn> blockingQueue, CanvasView view, DisplayAdapter adapter)
            : base(blockingQueue)
        {
            View = view;
            Adapter = adapter;
            Axis = new AxisBuilder(View); // todo bug
        }

        /// <summary>
        /// The view used to present displays.
        /// </summary>
        public CanvasView View { get; }
        /// <summary>
        /// <see cref="DisplayAdapter"/>
        /// </summary>
        public DisplayAdapter Adapter { get; }
        /// <summary>
        /// A dummy axis builder. // TODO
        /// </summary>
        protected AxisBuilder Axis { get; }
        /// <summary>
        /// Save data.
        /// </summary>
        public abstract bool Save { get; set; }
    }
}