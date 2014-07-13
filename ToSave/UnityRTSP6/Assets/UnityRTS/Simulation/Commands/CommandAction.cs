using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Simulation {
    /// <summary>
    /// Begin an action on the provided entities
    /// </summary>
    [Serializable]
    public class CommandAction : Command {

        public int[] EntityIds;
        public int RequestEntityId;
        public Vector3 RequestLocation;

        public override void Invoke(SimWorld world) {
            var request = new ActionRequest() {
                TargetEntity = world.GetEntityById(RequestEntityId),
                TargetLocation = RequestLocation,
            };
            foreach (var entityId in EntityIds) {
                var entity = world.GetEntityById(entityId);
                entity.BeginAction(request);
            }
            base.Invoke(world);
        }

    }

}
