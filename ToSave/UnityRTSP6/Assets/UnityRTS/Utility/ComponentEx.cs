using UnityEngine;
using System.Collections;

namespace UnityRTS {

    public static class ComponentEx {

        // Helper method to handle getting components in parents
        public static T GetComponentInParents<T>(this Component behaviour) where T : Component {
            var transform = behaviour.transform;
            while (transform != null) {
                var component = transform.GetComponent<T>();
                if (component != null) return component;
                transform = transform.parent;
            }
            return null;
        }

        public static bool TryBroadcastMessage(this Component component, string name, object obj) {
            var receiverCount = BroadcastTo(component.GetComponentsInChildren<MonoBehaviour>(), name, obj);
            return receiverCount > 0;
        }
        public static bool TryBroadcastMessageUpward(this Component component, string name, object obj) {
            while (component != null) {
                var receiverCount = BroadcastTo(component.GetComponents<MonoBehaviour>(), name, obj);
                if (receiverCount > 0) return true;
                component = component.transform.parent;
            }
            return false;
        }
        public static int BroadcastTo(MonoBehaviour[] behaviours, string name, object obj) {
            int receiverCount = 0;
            for (int b = 0; b < behaviours.Length; ++b) {
                var behaviour = behaviours[b];
                var method = behaviour.GetType().GetMethod(name);
                if (method != null) {
                    receiverCount++;
                    method.Invoke(behaviour, new[] { obj });
                }
            }
            return receiverCount;
        }

    }

}