using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation {
    public class SCResourceDropSite : SimComponent {

        public int DeliverResources(string type, int amount) {
            return Entity.Player.DeliverResources(type, amount);
        }

    }
}
