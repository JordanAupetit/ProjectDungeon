using UnityEngine;
using System.Collections;

public class playerMovement : MonoBehaviour {
	
  NavMeshAgent agent;

  void Start () {
    agent = GetComponent< NavMeshAgent >();
  }

  void Update () {
    if (Input.GetMouseButtonDown(0)) {
    	Debug.Log("click");
      // ScreenPointToRay() takes a location on the screen
      // and returns a ray perpendicular to the viewport
      // starting from that location
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      // Note that "11" represents the number of the "ground"
      // layer in my project. It might be different in yours!
      //LayerMask mask = LayerMask.NameToLayer("ground");
      
      // Cast the ray and look for a collision
      if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {

      	//Debug.Log("Left Click :: Name => " + hit.collider.name + " Tag => " + hit.collider.tag);
      	if (hit.collider.name == "Plane") {

	      	Debug.Log("click ground");
	        // If we detect a collision with the ground, 
	        // tell the agent to move to that location
	        agent.destination = hit.point;
      	}
      }
    }
  }
}
