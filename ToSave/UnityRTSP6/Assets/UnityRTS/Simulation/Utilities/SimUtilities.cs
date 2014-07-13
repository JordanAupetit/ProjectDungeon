using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation {
    public static class EnumEx {
        public static bool HasFlag(this Enum value, Enum derp) {
            var iderp = Convert.ToInt32(derp);
            return HasFlag(value, iderp);
        }
        public static bool HasFlag(this Enum value, int iderp) {
            var ivalue = Convert.ToInt32(value);
            return (ivalue & iderp) == iderp;
        }
    }
}
