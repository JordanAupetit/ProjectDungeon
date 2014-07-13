using UnityEngine;
using System.Collections;

[SimulationMapping(typeof(Simulation.SCPathEntity))]
public class PathEntity : MonoBehaviour {

    // Bounds to be used for AO, height blocking, and LOS reveal
    public BoxCollider BoundCollider;

    public void Awake() {
        if (BoundCollider == null) BoundCollider = GetComponent<BoxCollider>();
    }

}
