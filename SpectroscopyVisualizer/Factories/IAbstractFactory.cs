using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
