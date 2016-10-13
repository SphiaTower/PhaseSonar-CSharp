using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Factories {
    public static class FactoryHolder {
        [CanBeNull] private static IFactory _factory;

        [NotNull]
        public static IFactory Get() {
            return _factory ?? (_factory = new ParallelInjector());
        }
    }
}