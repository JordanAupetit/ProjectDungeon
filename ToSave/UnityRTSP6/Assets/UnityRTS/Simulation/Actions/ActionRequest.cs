using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation {
    public class ActionRequest {

        // The target entity and position
        public SimComponent TargetComponent;
        public SimEntity TargetEntity;
        public Vector2 TargetLocation;

        public ActionRequest() { }
        public ActionRequest(SimComponent component) : this(component.Entity) { TargetComponent = component; }
        public ActionRequest(SimEntity entity) : this(entity.Position) { TargetEntity = entity; }
        public ActionRequest(Vector2 loc) { TargetLocation = loc; }

        public float GetDistanceTo(Vector2 pos) {
            // TODO: Use IServiceProvider
            if (TargetEntity != null) return TargetEntity.GetDistanceTo(pos);
            return Vector2.Distance(pos, TargetLocation);
        }

        // Get the target entities position, or the target position
        public Vector2 GetTargetLocation() {
            if (TargetEntity != null) return TargetEntity.Position;
            return TargetLocation;
        }

        // Try to find the require component
        public T RequireComponent<T>() where T : SimComponent {
            if (TargetComponent is T) return (T)TargetComponent;
            if (TargetEntity != null) return TargetEntity.GetComponent<T>();
            return null;
        }

        // So that the debug print looks nicer
        public override string ToString() {
            //return (TargetComponent != null ? TargetComponent.name + "@" : "") + (TargetEntity != null ? TargetEntity.name : "-none-") + " at (" + TargetLocation + ")";
            return (TargetEntity != null ? TargetEntity.Name : "-none-") + " at (" + TargetLocation + ")";
        }

    }
}
