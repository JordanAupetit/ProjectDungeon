using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Simulation {
    public class SimWorld : IServiceProvider {

        public PathingMap PathingMap;

        // List of commands to be invoked
        public List<Command> Commands = new List<Command>();

        public readonly ObservableList<SimEntity> Entities = new ObservableList<SimEntity>();

        public DateTime SimulationTime;

        public SimWorld() {
            Entities.OnAdding += Entities_OnAdding;
            Entities.OnRemoving += Entities_OnRemoving;
            Entities.OnClearing += Entities_OnClearing;
        }

        private void Entities_OnAdding(ObservableList<SimEntity> source, SimEntity entity) {
            entity.Bind(this);
        }
        private void Entities_OnRemoving(ObservableList<SimEntity> source, SimEntity entity) {
            entity.Unbind();
        }
        private void Entities_OnClearing(ObservableList<SimEntity> entities) {
            for (int e = 0; e < Entities.Count; ++e) Entities[e].Unbind();
        }

        public void Begin() { Begin(DateTime.MinValue); }
        public void Begin(DateTime time) {
            SimulationTime = time;
        }

        public void Step(int ms) {
            SimulationTime += TimeSpan.FromMilliseconds(ms);
            // Invoke any commands for this frame
            for (int c = 0; c < Commands.Count; ++c) {
                if (SimulationTime >= Commands[c].ExecutionTime) {
                    Commands[c].Invoke(this);
                    Commands.RemoveAt(c--);
                }
            }

            for (int e = Entities.Count - 1; e >= 0; --e) {
                Entities[e].Step(ms);
            }
        }

        public void PushCommand(CommandAction command) {
            Commands.Add(command);
        }


        public T GetService<T>() {
            if (typeof(T) == typeof(ObservableList<SimEntity>)) return (T)(object)Entities;
            if (typeof(T) == typeof(PathingMap)) return (T)(object)PathingMap;
            return default(T);
        }

        public void CloneFrom(SimWorld other) {
            SimulationTime = other.SimulationTime;
            int e = 0, o = 0;
            while (e < Entities.Count || o < other.Entities.Count) {
                int oid = o;
                if (e < Entities.Count) {
                    for (; oid < other.Entities.Count; ++oid) if (other.Entities[oid].Id == Entities[e].Id) break;
                } else oid = other.Entities.Count;
                if (e == Entities.Count || oid < other.Entities.Count) {
                    for (; o < oid; ++o) {
                        var otherEntity = other.Entities[o];
                        var myEntity = new SimEntity(otherEntity.Id);
                        myEntity.CloneFrom(otherEntity);
                        Entities.Insert(e++, myEntity);
                    }
                }
                if (e < Entities.Count) {
                    if(oid < other.Entities.Count) {
                        if (oid != o) Debug.LogError("Id missmatch");
                        Entities[e++].CloneFrom(other.Entities[o++]);
                    } else {
                        Entities.RemoveAt(e);
                    }
                }
            }
        }

        public SimEntity GetEntityById(int id) {
            for (int e = 0; e < Entities.Count; ++e) if (Entities[e].Id == id) return Entities[e];
            return null;
        }
    }
}