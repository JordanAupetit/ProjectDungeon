using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation {
    public class SAInteract : SCAction {

        private SAMove moveAction;
        public ActionRequest SubRequest {
            get { return moveAction != null ? moveAction.Request : null; }
            set {
                EndStage();
                if (moveAction == null) moveAction = Entity.GetComponent<SAMove>();
                moveAction.Begin(value);
            }
        }
        public float InteractionRange = 1;
        public float InteractionInterval = 1;

        private float interactionTime;

        public override bool Complete {
            get { return moveAction == null || moveAction.Request == null; }
        }

        public virtual float AugmentedRange { get { return InteractionRange; } }
        public virtual float AugmentedInterval { get { return InteractionInterval; } }

        public override void Begin(ActionRequest request) {
            base.Begin(request);
            moveAction = null;
            RequireStage();
        }
        public override void Step(int ms) {
            bool allowMove = true;
            if (moveAction != null && moveAction.Request != null && IsInRange(moveAction.Request)) {
                if (UpdateInteraction(moveAction.Request, ms / 1000.0f)) allowMove = false;
            }
            if (moveAction != null && moveAction.Request != null && allowMove) moveAction.Step(ms);
        }
        protected virtual bool IsInRange(ActionRequest request) {
            return request.GetDistanceTo(Position) <= AugmentedRange;
        }
        protected virtual bool UpdateInteraction(ActionRequest request, float dt) {
            if (SubRequest != null) {
                interactionTime += dt;
                var interval = AugmentedInterval;
                if (interactionTime > interval) {
                    interactionTime -= interval;
                    NotifyInterval(request);
                }
                return true;
            }
            return false;
        }
        protected virtual void NotifyInterval(ActionRequest request) {

        }

        protected virtual void RequireStage() {
            if (moveAction == null || moveAction.Request == null) BeginStage();
        }
        protected virtual void BeginStage() {
            BeginStage(Request);
        }
        protected virtual void BeginStage(ActionRequest request) {
            SubRequest = request;
        }
        protected virtual void EndStage() {
            if (moveAction != null) moveAction.End();
            interactionTime = 0;
        }

        public override void CloneFrom(SimComponent component) {
            var other = (SAInteract)component;
            InteractionRange = other.InteractionRange;
            InteractionInterval = other.InteractionInterval;
            interactionTime = other.interactionTime;
            base.CloneFrom(component);
        }
    }
}
