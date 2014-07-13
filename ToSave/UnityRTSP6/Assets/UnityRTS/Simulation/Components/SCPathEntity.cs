using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation {
    public class SCPathEntity : SimComponent {

        private PathingMap map;

        public override void Bind(SimEntity entity) {
            base.Bind(entity);
            map = GetService<PathingMap>();
            if (map != null) map.AddCost(Entity.Bounds, Entity.Position);
        }
        public override void Unbind() {
            if (map != null) map.RemoveCost(Entity.Bounds, Entity.Position);
            base.Unbind();
        }

    }
}
