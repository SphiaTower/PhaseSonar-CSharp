using JetBrains.Annotations;

namespace PhaseSonar.Correctors
{
    public interface ICorrector<out T> where T:ISpectrum
    {
        [NotNull]
        T OutputSpetrumBuffer();

        int OutputLength { get; }

        void Correct([NotNull] double[] pulseSequence, int startIndex, int pulseLength, int pointsBeforeCrest);
        void ClearBuffer();
    }
}