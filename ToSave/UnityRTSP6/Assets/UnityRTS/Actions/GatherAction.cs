using UnityEngine;
using System.Collections;

[SimulationMapping(typeof(Simulation.SAGather))]
public class GatherAction : InteractionAction {

    public int Capacity = 10;
    public int GatherAmount = 1;
    public float DropRange = 1;
    public float DropTime = 0.5f;

}
