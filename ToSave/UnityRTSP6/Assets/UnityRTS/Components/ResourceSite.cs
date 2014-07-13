using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[SimulationMapping(typeof(Simulation.SCResourceSite))]
public class ResourceSite : MonoBehaviour {

    public ResourceCollection Resources;
    public bool DieOnDeplete = true;

}
