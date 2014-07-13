using UnityEngine;
using System.Linq;
using System.Collections;
using System;

public class Entity : MonoBehaviour {

    public Player Player;
    public Vector2 Position {
        get { var pos = transform.position; return new Vector2(pos.x, pos.z); }
        set { transform.position = new Vector3(value.x, transform.position.y, value.y); }
    }

    public Simulation.SimEntity Simulation { get; private set; }

    public int Health { get { return Simulation.Health; } }
    public int MaxHealth { get { return Simulation.MaxHealth; } }

    public void Awake() {
        // Ensure the entity has a player
        if (Player == null) {
            var gaia = GameObject.Find("Gaia");
            if (gaia != null) Player = gaia.GetComponent<Player>();
        }
        if (Player == null) Player = GameObject.FindObjectOfType<Player>();
    }

    public void Bind(Simulation.SimEntity simulation) {
        if (Simulation != null) Unbind();
        Simulation = simulation;
        Simulation.OnDead += Simulation_OnDead;
    }

    public void Unbind() {
        Simulation.OnDead -= Simulation_OnDead;
        Simulation = null;
    }

    private void Simulation_OnDead() {
        Die();
    }

    // Controlled update of the object
    public void Update() {
        float dt = Time.deltaTime;
        if (Simulation != null) {
            Position = Vector2.MoveTowards(Position, Simulation.Position, dt * 50);
        }
    }

    // Return if this object can be selected
    public bool GetIsSelectable() {
        return renderer.enabled;
    }

    // Kill the object
    public void Die() {
        Destroy(gameObject);
    }

}
