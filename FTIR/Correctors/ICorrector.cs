using JetBrains.Annotations;

namespace FTIR.Correctors
{
    public interface ICorrector
    {
        void Correct([NotNull]double[] pulseSequence, int startIndex, int pulseLength, int pointsBeforeCrest);
        [NotNull]
        double[] Output { get; }
        int OutputPeriodCnt();
        void ClearBuffer();
    }
}