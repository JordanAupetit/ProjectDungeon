using UnityEngine;
using System.Collections;

namespace Simulation {
    public class SimComponent {

        public SimEntity Entity { get; private set; }

        // Position of the owning entity (or source of this action)
        public Vector2 Position { get { return Entity.Position; } }

        public virtual void Bind(SimEntity entity) {
            Entity = entity;
        }
        public virtual void Unbind() {
            Entity = null;
        }

        public virtual T GetService<T>() where T : class {
            return Entity.GetService<T>();
        }


        public virtual void Step(int ms) {
            
        }

        public virtual void CloneFrom(SimComponent component) {
            
        }
    }
}