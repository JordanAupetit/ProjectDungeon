using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation {
    public class SAAttack : SAInteract {

        public int Damage = 1;

        public override float ScoreRequest(ActionRequest request) {
            return request.TargetEntity != null &&
                request.TargetEntity.Player != Entity.Player ? 1.6f :
                0;
        }

        public override void Begin(ActionRequest request) {
            Debug.Log("Starting attacking " + request);
            base.Begin(request);
        }

        protected override void NotifyInterval(ActionRequest request) {
            if (request.TargetEntity != null)
                request.TargetEntity.Damage(Damage);
            base.NotifyInterval(request);
        }

        public override void CloneFrom(SimComponent component) {
            var other = (SAAttack)component;
            Damage = other.Damage;
            base.CloneFrom(component);
        }
    }
}
