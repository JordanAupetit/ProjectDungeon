using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation {
    public class SCResourceSite : SimComponent {

        public ResourceCollection Resources;
        public bool DieOnDeplete = true;

        public int TakeResources(string name, int amount) {
            var taken = Resources.TakeResources(name, amount);
            if (DieOnDeplete && !Resources.HasResources) {
                Entity.Die();
            }
            return taken;
        }

        public override void CloneFrom(SimComponent component) {
            var other = (SCResourceSite)component;
            if (Resources == null) Resources = new ResourceCollection();
            Resources.CloneFrom(other.Resources);
            DieOnDeplete = other.DieOnDeplete;
            base.CloneFrom(component);
        }

    }
}
