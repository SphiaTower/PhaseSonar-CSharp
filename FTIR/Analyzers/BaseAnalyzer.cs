using System.Collections.Generic;
using FTIR.Correctors;
using FTIR.Slicers;

namespace FTIR.Analyzers
{
    public abstract class BaseAnalyzer
    {
        public ISlicer Slicer { get; set; }

        protected BaseAnalyzer(ISlicer slicer)
        {
            Slicer = slicer;
        }

    }

    public class SpecInfo
    {
        public SpecInfo(double[] spec, int periodCnt)
        {
            Spec = spec;
            PeriodCnt = periodCnt;
        }

        public double[] Spec { get; }
        public int PeriodCnt { get; }
    }
}