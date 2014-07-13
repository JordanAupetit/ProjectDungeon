using UnityEngine;
using System.Collections;

using UnityRTS;

/// <summary>
/// The MoveAction provides logic to move an object through
/// the world
/// </summary>
[SimulationMapping(typeof(Simulation.SAMove))]
public class MoveAction : EntityAction {

    // The speed at which to move
    public float Speed = 1;

}
