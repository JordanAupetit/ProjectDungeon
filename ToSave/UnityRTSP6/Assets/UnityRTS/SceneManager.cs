using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Control to invoke action around unit (ie. cont+b to build nearest building)
// Stutter step build! (cont+b, s, cont+b, s, cont+b)
// Labeled player spots in maps, ie. Brad at "Left Flank", Bob at "Pocket", etc.
// Multi-screen

public class SceneManager : MonoBehaviour {

    public Simulation.SimWorld Simulation;
    public Simulation.SimWorld ServerSimulation;

    public Player CurrentPlayer;

    void Start() {
        Simulation = new Simulation.SimWorld();
        Simulation.PathingMap = GameObject.FindObjectOfType<Simulation.PathingMap>();
        var entities = GameObject.FindObjectsOfType<Entity>();
        for (int e = 0; e < entities.Length; ++e) {
            var entity = entities[e];
            var simEntity = new Simulation.SimEntity(e);
            simEntity.Player = entity.Player;
            simEntity.Position.Value = entity.Position;
            entity.Bind(simEntity);
            foreach (var component in entity.GetComponentsInChildren<MonoBehaviour>()) {
                var mapping = (SimulationMappingAttribute[])component.GetType().GetCustomAttributes(typeof(SimulationMappingAttribute), true);
                if (mapping != null && mapping.Length > 0) {
                    var simType = mapping[0].SimulationType;
                    var simCmp = (Simulation.SimComponent)Activator.CreateInstance(simType);
                    foreach (var field in component.GetType().GetFields()) {
                        if (field.DeclaringType.IsAssignableFrom(typeof(MonoBehaviour))) continue;
                        var simFld = simCmp.GetType().GetField(field.Name);
                        if (simFld == null) continue;
                        simFld.SetValue(simCmp, field.GetValue(component));
                    }
                    if (component is PathEntity) {
                        var pathEntity = (PathEntity)component;
                        var simPEntity = (Simulation.SCPathEntity)simCmp;
                        var boxC = pathEntity.BoundCollider;
                        if (boxC != null) {
                            var size = Vector3.Scale(boxC.size, entity.transform.localScale);
                            var polygon = new Simulation.Polygon(
                                new Vector2(boxC.center.x - size.x / 2, boxC.center.z - size.z / 2),
                                new Vector2(boxC.center.x + size.x / 2, boxC.center.z - size.z / 2),
                                new Vector2(boxC.center.x + size.x / 2, boxC.center.z + size.z / 2),
                                new Vector2(boxC.center.x - size.x / 2, boxC.center.z + size.z / 2)
                            );
                            simEntity.Bounds.Value = polygon;
                        }
                    }
                    simEntity.Components.Add(simCmp);
                }
            }
            Simulation.Entities.Add(simEntity);
            entity.SendMessage("SetIsCurrentTeam", Instance.CurrentPlayer == entity.Player);
        }
        Simulation.Begin();
        ServerSimulation = new Simulation.SimWorld();
        ServerSimulation.CloneFrom(Simulation);
    }

    DateTime lastUpdate = DateTime.Now;
    void Update() {
        if (lastUpdate < DateTime.Now - TimeSpan.FromSeconds(1)) lastUpdate = DateTime.Now - TimeSpan.FromSeconds(1);
        // Step the server to the safest point
        while (DateTime.Now > lastUpdate + TimeSpan.FromMilliseconds(10)) {
            ServerSimulation.Step(10);
            lastUpdate += TimeSpan.FromMilliseconds(10);
        }
        // Step the client to be infront of the server
        while (Simulation.SimulationTime < ServerSimulation.SimulationTime + TimeSpan.FromSeconds(1)) {
            Simulation.Step(10);
        }

        if (Input.GetKeyDown(KeyCode.O)) {
            ServerSimulation.CloneFrom(Simulation);
            Debug.Log(ServerSimulation.Entities.Count + " == " + Simulation.Entities.Count);
        }
        if (Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.I)) {
            var simSource = Input.GetKeyDown(KeyCode.U) ? ServerSimulation : Simulation;
            var entities = GameObject.FindObjectsOfType<Entity>();
            for (int e = 0; e < entities.Length; ++e) {
                var entity = entities[e];
                entity.Bind(simSource.GetEntityById(entity.Simulation.Id));
            }
            Debug.Log("Server: " + ServerSimulation.SimulationTime.Ticks / 1000000 + " Client: " + Simulation.SimulationTime.Ticks / 1000000);
        }
    }

    // Queue a command to be invoked as soon as possible
    public void QueueCommand(Simulation.CommandAction command) {
        command.ExecutionTime = Simulation.SimulationTime;
        Simulation.PushCommand(command);
        ServerSimulation.PushCommand(command);
    }


    // A singleton instance of this class
    private static SceneManager _instance;
    public static SceneManager Instance {
        get {
            if (_instance == null) _instance = GameObject.FindObjectOfType<SceneManager>();
            return _instance;
        }
    }

}
