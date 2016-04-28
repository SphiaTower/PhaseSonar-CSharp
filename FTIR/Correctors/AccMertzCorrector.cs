using FTIR.Maths;

namespace FTIR.Correctors {
    public class AccMertzCorrector :MertzCorrector{
        

        private int _accCount = 0;

        public override int OutputPeriodCnt()
        {
            return _accCount;
        }

        public override void Correct(double[] pulseSequence, int startIndex, int pulseLength,int pointsBeforeCrest)
        {
            base.Correct(pulseSequence, startIndex, pulseLength, pointsBeforeCrest);
            _accCount++;
        }

        protected override void WriteBuffer(int i, double specPoint)
        {
            Output[i] += specPoint;
        }

        public override void ClearBuffer()
        {
            base.ClearBuffer();
            _accCount = 0;
        }

        public AccMertzCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor, int centreSpan = 256) : base(apodizer, fuzzyPulseLength, zeroFillFactor, centreSpan)
        {
        }
    }

   
}
