using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation {
    public class SAMove : SCAction {

        // The speed at which to move
        public float Speed = 1;

        // The action is complete when it has reached its destination
        public override bool Complete {
            get { return query == null || DistanceFromTarget < 0.5f; }
        }

        // A requst always contains a location, thus this action is always valid
        public override float ScoreRequest(ActionRequest request) { return 1; }

        private PathingQuery query;

        public override void Begin(ActionRequest request) {
            Rect bounds = new Rect(0, 0, 0, 0);
            if (request.TargetEntity != null) {
                bounds = request.TargetEntity.GetBoundingRect();
            } else {
                var pos = request.GetTargetLocation();
                bounds = new Rect(pos.x - 0.5f, pos.y - 0.5f, 1, 1);
            }
            query = new PathingQuery(GameObject.FindObjectOfType<PathingMap>(), bounds, 1);
            base.Begin(request);
        }

        // Move the owning entity toward the target
        public override void Step(int ms) {
            //var target = Request.GetTargetLocation();
            //entity.Position = Vector2.MoveTowards(entity.Position, target, dt * Speed);
            if (query != null) {
                var dir = query.GetDirectionFrom(Entity.Position);
                if ((Request.GetTargetLocation() - Entity.Position).sqrMagnitude < 0.7f * 0.7f) {
                    dir = (Request.GetTargetLocation() - Entity.Position).normalized;
                }
                Entity.Position.Value += dir * ms * Speed / 1000.0f;
            }
            base.Step(ms);
        }
        public override void CloneFrom(SimComponent component) {
            var other = (SAMove)component;
            Speed = other.Speed;
            base.CloneFrom(component);
        }
    }
}
