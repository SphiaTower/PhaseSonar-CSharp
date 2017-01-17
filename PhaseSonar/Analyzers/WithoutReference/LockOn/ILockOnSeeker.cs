using System.Numerics;

namespace PhaseSonar.Analyzers.WithoutReference.LockOn {
    public interface ILockOnSeeker {
        int SeekLockOnPoint(Complex[] spec, int start, int stop);
    }
}