using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

    public bool EnableScreenEdges = true;
    public bool EnableKeyboard = true;

    void Update() {
        Vector3 right = transform.right;
        Vector3 forward = Vector3.Cross(right, Vector3.up);

        Vector2 move = Vector2.zero;
        // Receive input from moving the mouse to the screen edges
        if (EnableScreenEdges) {
            if (Input.mousePosition.x <= 0) move.x--;
            if (Input.mousePosition.y <= 0) move.y--;
            if (Input.mousePosition.x >= Screen.width - 1) move.x++;
            if (Input.mousePosition.y >= Screen.height - 1) move.y++;
        }
        // Receive input from pressing the directional keys
        if (EnableKeyboard) {
            move.x += Input.GetAxis("Horizontal");
            move.y += Input.GetAxis("Vertical");
        }

        transform.position += (right * move.x + forward * move.y) * 30 * Time.deltaTime;
    }

}
