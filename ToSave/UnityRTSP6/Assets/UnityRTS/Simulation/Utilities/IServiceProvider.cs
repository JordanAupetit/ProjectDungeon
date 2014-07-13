using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation {
    public interface IServiceProvider {
        T GetService<T>();
    }
}
