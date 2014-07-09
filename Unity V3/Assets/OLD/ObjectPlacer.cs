using UnityEngine;
using System.Collections;
using Pathfinding;

/** Small sample script for placing obstacles */
public class ObjectPlacer : MonoBehaviour {
	
	public GameObject friend; /** GameObject to place. Make sure the layer it is in is included in the collision mask on the GridGraph settings (assuming a GridGraph) */
	public GameObject ennemi;
	public GameObject obstacle; // Cube
	
	public bool direct = false; /** Flush Graph Updates directly after placing. Slower, but updates are applied immidiately */
	
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown ("w")) {
			PlaceObject (friend);
		}
		if (Input.GetKeyDown ("x")) {
			PlaceObject (ennemi);
		}
		if (Input.GetKeyDown ("c")) {
			PlaceObject (obstacle);
		}
		
		if (Input.GetKeyDown ("r")) {
			RemoveObject ();
		}
	}
	
	public void PlaceObject (GameObject go) {
		
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if ( Physics.Raycast (ray, out hit, Mathf.Infinity)) {
			string hitName = hit.collider.name;
			Debug.Log ("Voici la hauteur du GO : " + go.transform.lossyScale.y);
			
			if (hitName == "Ground") {
				Vector3 posGO = hit.collider.gameObject.transform.position;
				
				//Vector3 p = new Vector3(hit.point.x, posGO.y + go.transform.lossyScale.y / 2, hit.point.z);
				Vector3 p = new Vector3(hit.point.x, 1, hit.point.z);
				
				Debug.Log ("Avec la position : " + p);
			
				GameObject obj = (GameObject)GameObject.Instantiate (go,p,Quaternion.identity);
				
				Bounds b = obj.collider.bounds;
				//Pathfinding.Console.Write ("// Placing Object\n");
				GraphUpdateObject guo = new GraphUpdateObject(b);
				AstarPath.active.UpdateGraphs (guo);
				if (direct) {
					//Pathfinding.Console.Write ("// Flushing\n");
					AstarPath.active.FlushGraphUpdates();
				}	
			}
		}
	}
	
	public void RemoveObject () {
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if ( Physics.Raycast (ray, out hit, Mathf.Infinity)) {
			if (hit.collider.isTrigger || hit.transform.gameObject.name == "Ground") return;
			
			Bounds b = hit.collider.bounds;
			Destroy (hit.collider);
			Destroy (hit.collider.gameObject);
			
			//Pathfinding.Console.Write ("// Placing Object\n");
			GraphUpdateObject guo = new GraphUpdateObject(b);
			AstarPath.active.UpdateGraphs (guo,0.0f);
			if (direct) {
				//Pathfinding.Console.Write ("// Flushing\n");
				AstarPath.active.FlushGraphUpdates();
			}
		}
	}
}
