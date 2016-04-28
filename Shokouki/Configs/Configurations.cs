using System;
using System.Windows.Controls;
using FTIR.Utils;

namespace Shokouki.Configs
{
    public class Configurations
    {
        private static Configurations _singleton;

        private Configurations(double repetitionRate, int centreSpanLength, int zeroFillFactor, int threadNum, int dispPoints)
        {
            RepetitionRate = repetitionRate;
            ZeroFillFactor = zeroFillFactor;
            ThreadNum = threadNum;
            DispPoints = dispPoints;
            CentreSpanLength = centreSpanLength;
        }

        public double RepetitionRate { get; set; }
        public int ZeroFillFactor { get; set; }

        public int CentreSpanLength { get; set; }
        public int ThreadNum { get; set; }
        public int DispPoints { get; set; }



        public void Bind(
            Control repetitionRate,
            Control zeroFillFactor,
            Control centreSpanLength,
            Control threadNum,
            Control dispPoints)
        {
            repetitionRate.DataContext = this;
            zeroFillFactor.DataContext = this;
            centreSpanLength.DataContext = this;
            threadNum.DataContext = this;
            dispPoints.DataContext = this;
        }
        public static void Initialize(double repetitionRate,int centreSpanLength, int zeroFillFactor, int threadNum, int dispPoints) {
            if (_singleton!=null)
            {
                throw new Exception("environment already init");
            }
            _singleton = new Configurations(repetitionRate, centreSpanLength, zeroFillFactor,threadNum,dispPoints);
        }

        public static Configurations Get()
        {
            Toolbox.RequireNonNull(_singleton, "environment not init");
            return _singleton;
        }
    }
}