using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SimulationMappingAttribute : Attribute {

    public Type SimulationType;

    public SimulationMappingAttribute(Type type) {
        SimulationType = type;
    }

}
