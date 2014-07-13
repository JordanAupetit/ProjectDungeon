using UnityEngine;
using System.Collections;

[SimulationMapping(typeof(Simulation.SAAttack))]
public class AttackAction : InteractionAction {

    public int Damage = 1;

}
