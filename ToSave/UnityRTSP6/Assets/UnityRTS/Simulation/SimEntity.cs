using UnityEngine;
using System.Collections;
using System;

namespace Simulation {
    public class SimEntity : IServiceProvider {

        public readonly int Id;
        public Observable<string> Name;
        public Observable<Vector2> Position;
        public readonly ObservableList<SimComponent> Components = new ObservableList<SimComponent>();
        public readonly TaskManager TaskManager;
        public Observable<Polygon> Bounds;

        public Player Player { get; set; }
        public SimWorld World;

        public SCAction ActiveAction { get; private set; }
        public Action OnDead;

        public int Health = 100, MaxHealth = 100;

        public SimEntity(int id) {
            Id = id;
            TaskManager = new TaskManager();
            //Bounds.Value = new Polygon(new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1));
            Components.OnAdding += Components_OnAdding;
            Components.OnRemoving += Components_OnRemoving;
            Components.OnClearing += Components_OnClearing;
        }

        private void Components_OnAdding(ObservableList<SimComponent> source, SimComponent component) {
            if (World != null) component.Bind(this);
        }
        private void Components_OnRemoving(ObservableList<SimComponent> source, SimComponent component) {
            if (World != null) component.Unbind();
        }
        private void Components_OnClearing(ObservableList<SimComponent> component) {
            if (World != null) for (int c = 0; c < Components.Count; ++c) Components[c].Unbind();
        }

        public void Bind(SimWorld world) {
            World = world;
            for (int c = 0; c < Components.Count; ++c) Components[c].Bind(this);
        }
        public void Unbind() {
            for (int c = 0; c < Components.Count; ++c) Components[c].Unbind();
            World = null;
        }

        public float GetDistanceTo(Vector2 pos) {
            if (Bounds.Value != null) {
                return Bounds.Value.GetDistanceTo(pos - Position);
            } else {
                var bounds = GetBoundingRect();
                pos.x = Mathf.Clamp(pos.x, bounds.xMin, bounds.xMax);
                pos.y = Mathf.Clamp(pos.y, bounds.yMin, bounds.yMax);
                return (pos - Position).magnitude;
            }
        }

        public T GetComponent<T>() where T : SimComponent {
            for (int c = 0; c < Components.Count; ++c) if (Components[c] is T) return (T)Components[c];
            return null;
        }

        public T GetService<T>() {
            if (typeof(T) == typeof(Polygon)) return (T)(object)Bounds.Value;
            if (World != null) return World.GetService<T>();
            return default(T);
        }

        public T FindNearby<T>() where T : SimComponent { return FindNearby<T>(null); }
        public T FindNearby<T>(Func<T, bool> selector) where T : SimComponent {
            var myPos = Position;
            var entities = GetService<ObservableList<SimEntity>>();
            float minDist2 = float.MaxValue;
            T nearest = null;
            for (int e = 0; e < entities.Count; ++e) {
                var entity = entities[e];
                var component = entity.GetComponent<T>();
                if (component == null) continue;
                if (selector != null && !selector(component)) continue;
                float dist2 = (entity.Position.Value - Position.Value).sqrMagnitude;
                if (dist2 < minDist2) {
                    minDist2 = dist2;
                    nearest = component;
                }
            }
            return nearest;
        }


        public Rect GetBoundingRect() {
            Vector2 pos = Position;
            if (Bounds.Value != null) {
                Rect rect = Bounds.Value.GetExtents();
                rect.x += pos.x;
                rect.y += pos.y;
                return rect;
            } else {
                return new Rect(pos.x - 0.5f, pos.y - 0.5f, 1, 1);
            }
        }

        // Request an action be performed
        public void BeginAction(ActionRequest request) {
            SCAction best = null;
            float bestScore = float.MinValue;
            for (int c = 0; c < Components.Count; ++c) {
                var action = Components[c] as SCAction;
                if (action != null) {
                    float score = action.ScoreRequest(request);
                    if (score > bestScore) {
                        best = action;
                        bestScore = score;
                    }
                }
            }
            if (best != null) BeginAction(best, request);
            else Debug.Log("Unable to find action for request " + request);
        }
        // Request a specific action be performed
        public void BeginAction(SCAction action, ActionRequest request) {
            if (ActiveAction != null) ActiveAction.End();
            ActiveAction = action;
            if (ActiveAction != null) ActiveAction.Begin(request);
        }
        // End the current action
        public void EndAction() {
            BeginAction(null, null);
        }

        // Controlled update of the object
        public void Step(int ms) {
            if (ActiveAction != null) {
                ActiveAction.Step(ms);
                if (ActiveAction.Complete) EndAction();
            }
        }

        public void Damage(int amount) {
            if (Health > 0) {
                Health -= amount;
                if (Health <= 0) { Health = 0; Die(); }
            }
        }

        public void Die() {
            if (OnDead != null) OnDead();
            var entities = GetService<ObservableList<SimEntity>>();
            entities.Remove(this);
        }


        public void CloneFrom(SimEntity other) {
            Position = other.Position;
            Health = other.Health;
            MaxHealth = other.MaxHealth;
            int c = 0, o = 0;
            while (c < Components.Count || o < other.Components.Count) {
                int oid = o;
                if (c < Components.Count) {
                    for (; oid < other.Components.Count; ++oid) if (other.Components[oid].GetType() == Components[c].GetType()) break;
                } else oid = other.Components.Count;
                if (c == Components.Count || oid < other.Components.Count) {
                    for (; o < oid; ++o) {
                        var otherComp = other.Components[o];
                        var myComp = (SimComponent)Activator.CreateInstance(otherComp.GetType());
                        myComp.CloneFrom(otherComp);
                        Components.Insert(c++, myComp);
                    }
                }
                if (c < Components.Count) {
                    if (oid < other.Components.Count) {
                        if (oid != o) Debug.LogError("Id missmatch");
                        Components[c++].CloneFrom(other.Components[o++]);
                    } else {
                        Components.RemoveAt(c);
                    }
                }
            }
        }
    }
}
