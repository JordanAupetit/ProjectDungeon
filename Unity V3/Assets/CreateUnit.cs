using UnityEngine;
using System.Collections;
using Pathfinding;

/** Small sample script for placing obstacles */
public class CreateUnit : MonoBehaviour {
	
	public GameObject friend = null; 
	public Vector3 pos;
	public GameObject scripts;
	public Data data;
	
	public bool direct = false; /** Flush Graph Updates directly after placing. Slower, but updates are applied immidiately */
	
	void Start () {
		pos = new Vector3 (3.0f, 1.1f, -25.0f);
		scripts = GameObject.Find ("World_Scripts");
		data = scripts.GetComponent<Data>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown ("e")) {
			Debug.Log ("Je crée une unite");

			if(friend != null) {

				if(data.gold > 100.0f) { // COUT à modifier <<<<<<<<
					data.gold -= 100.0f;
					PlaceObject (friend);
				}
			}
		}
	}
	
	public void PlaceObject (GameObject go) {
		
		GameObject obj = (GameObject)GameObject.Instantiate (go,pos,Quaternion.identity);
		obj.SetActive (true);

		Bounds b = obj.collider.bounds;
		GraphUpdateObject guo = new GraphUpdateObject(b);
		AstarPath.active.UpdateGraphs (guo);

		if (direct)
			AstarPath.active.FlushGraphUpdates();
	}
}
