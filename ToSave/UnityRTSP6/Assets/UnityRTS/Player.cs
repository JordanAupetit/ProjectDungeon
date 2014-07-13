using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    // Use the object name as the players name
    public string Name { get { return name; } }
    // Allow configuration of a team colour
    public Color Color = Color.white;

    // The resources we are holding
    public ResourceCollection Resources;

    public int DeliverResources(string name, int amount) {
        return Resources.DeliverResources(name, amount);
    }

}
