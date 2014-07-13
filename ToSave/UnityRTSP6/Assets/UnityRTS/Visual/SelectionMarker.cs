using UnityEngine;
using System.Collections;

/// <summary>
/// Draw a marker above the terrain when the object is selected
/// </summary>
public class SelectionMarker : MonoBehaviour {

    // The entity to display this marker for
    public Entity Entity;

    void Start() {
        // Set the colour to the players colour
        renderer.material.color = Entity.Player.Color;
    }

    public void LateUpdate() {
        // Update the position to be slightly above the terrain
        // at the entities position
        var pos = Entity.Position;
        transform.position = new Vector3(pos.x, 0.02f, pos.y);
    }

}
