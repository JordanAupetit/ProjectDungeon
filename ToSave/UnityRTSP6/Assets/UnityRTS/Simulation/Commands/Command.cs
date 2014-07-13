using UnityEngine;
using System.Collections;
using System;

namespace Simulation {
    /// <summary>
    /// Subclasses of this class should be synchronised to all other connected clients
    /// and handle interacting with the game world. All interactions should be done
    /// through a Command, so that they are replicated on all clients
    /// </summary>
    [Serializable]
    public class Command {

        public DateTime ExecutionTime;

        public virtual void Invoke(SimWorld world) { }

    }

}