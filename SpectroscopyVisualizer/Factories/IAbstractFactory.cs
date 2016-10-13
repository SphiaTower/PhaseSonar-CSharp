using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Factories {
    public interface IAbstractFactory {
        [NotNull]
        IFactory GetFactory();
    }

    public class AbstractFactoryImpl : IAbstractFactory {
        public IFactory GetFactory() {
            return new ParallelInjector();
        }
    }
}