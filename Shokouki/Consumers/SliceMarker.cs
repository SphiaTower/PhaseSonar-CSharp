using System.Collections.Concurrent;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FTIR.Slicers;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    public class SliceMarker : UiConsumer<double[]>
    {
     
        public ISlicer Slicer { get; }


        public override void ConsumeElement(double[] item)
        {
          
            var startIndiceGroups = Slicer.Slice(item);
            if (startIndiceGroups == null)
            {
                return;
            }
            var crestIndice = startIndiceGroups[0].Select(startIndex =>(double) startIndex + Slicer.SliceStartOffset).ToArray();
            var crests = crestIndice.Select(index => item[(int) index]).ToArray();
            View.Invoke(() =>
            {
                var pulseSequencePoints = Adapter.ToPoints(item);
                var crestPoints = Adapter.ToPoints(crestIndice, crests);

                //var collection = Adapter.ToPoints(scaleAxis, crests);

                View.Clear();

                View.DrawLine(pulseSequencePoints);
                View.DrawLine(crestPoints,Colors.Red);
            });
        }

        public SliceMarker(BlockingCollection<double[]> blockingQueue, IScopeView view, DisplayAdapter adapter, ISlicer slicer) : base(blockingQueue, view, adapter)
        {
            Slicer = slicer;
        }
    }
}