using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation {
    public class SCAction : SimComponent {

        // The request that began this action
        public ActionRequest Request { get; private set; }
        // Is this action complete?
        public virtual bool Complete { get { return Request == null; } }

        // Distance from the actions target (or NaN if no target)
        public float DistanceFromTarget { get { return Request != null ? Request.GetDistanceTo(Position) : float.NaN; } }

        // Return how best this action is able to serve the request
        public virtual float ScoreRequest(ActionRequest request) { return 0; }

        // Begin the request
        public virtual void Begin(ActionRequest request) {
            Request = request;
        }
        // Update the entity
        public virtual void Step(int ms) {
            if (Request == null) throw new Exception("Unable to step an action without a target!");
        }
        // End the request
        public virtual void End() {
            Request = null;
        }

    }
}
