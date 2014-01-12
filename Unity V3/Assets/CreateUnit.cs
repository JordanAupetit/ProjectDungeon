using UnityEngine;
using System.Collections;
using Pathfinding;

/** Small sample script for placing obstacles */
public class CreateUnit : MonoBehaviour {

	protected float timeIntervalToSpawn;
	protected float clockToSpawn;
	protected GameObject[] chestBases;
	protected GameObject[] chests;
	protected float offsetChest;

	public GameObject friendCopy = null; 
	public GameObject ennemiCopy = null;
	public GameObject chestCopy = null;
	public Vector3 posFriendlyBase;
	public Vector3 posEnnemiBase;
	//public GameObject scripts;
	//public Data data;
	
	public bool direct = false; /** Flush Graph Updates directly after placing. Slower, but updates are applied immidiately */
	
	void Start () {
		posFriendlyBase = GameObject.Find ("FriendlyBase").transform.position;
		posFriendlyBase.y = 1.1f;
		posEnnemiBase = GameObject.Find ("EnnemiBase").transform.position;
		posEnnemiBase.y = 1.1f;
		chestBases = GameObject.FindGameObjectsWithTag ("ChestBaseTAG");
		offsetChest = 3.0f;

		//scripts = GameObject.Find ("World_Scripts");
		//data = scripts.GetComponent<Data>();

		timeIntervalToSpawn = 20.0f; // On crée une unité à chacun de ces intervalles
	}
	
	// Update is called once per frame
	void Update() {

		clockToSpawn += Time.deltaTime;

		if (Input.GetKeyDown ("e")) {
			if(friendCopy != null) {

				if(Data.gold > 100.0f) { // COUT à modifier <<<<<<<<
					Data.gold -= 100.0f;
					PlaceObject (friendCopy, posFriendlyBase);
					Debug.Log ("Unit Created");
				}
				else {
					Debug.Log ("Can't Create Unit");
				}
			}
		}

		if (clockToSpawn >= timeIntervalToSpawn) {
			if(ennemiCopy != null) {
				PlaceObject (ennemiCopy, posEnnemiBase);
				Debug.Log ("Ennemi Created");
				clockToSpawn = 0;
			}
		}
	}

	void OnGUI() {
		if (GUI.Button (new Rect (10, (Screen.height - 40), 150, 30), "Soldat (100) / Press E")) {
			if(friendCopy != null) {
				
				if(Data.gold > 100.0f) { // COUT à modifier <<<<<<<<
					Data.gold -= 100.0f;
					PlaceObject (friendCopy, posFriendlyBase);
					Debug.Log ("Unit Created");
				}
				else {
					Debug.Log ("Can't Create Unit");
				}
			}
		}

		if (GUI.Button (new Rect (170, (Screen.height - 40), 80, 30), "Chest (300)")) {
			chests = GameObject.FindGameObjectsWithTag ("ChestTAG");
			bool chestFound;

			foreach(GameObject chestBase in chestBases){

				chestFound = false;

				foreach(GameObject chest in chests){
					if ((chest.transform.position.x < chestBase.transform.position.x + offsetChest &&
					     chest.transform.position.x > chestBase.transform.position.x - offsetChest) &&
					    (chest.transform.position.z < chestBase.transform.position.z + offsetChest &&
					 	chest.transform.position.z > chestBase.transform.position.z - offsetChest))
					{
						// Chest dans la zone -- On quitte la boucle
						Debug.Log ("Il y a un coffre dans la ZONE !!   chest : " + chest.transform.position + " Base Chest : " + chestBase.transform.position);
						chestFound = true;
						break;
					}
				}

				// On crée le Chest
				if(chestCopy != null && chestFound == false) {
					PlaceObject (chestCopy, chestBase.transform.position);
					chests = GameObject.FindGameObjectsWithTag ("ChestTAG");
					break;
				}
			}
		}
	}
	
	public void PlaceObject (GameObject go, Vector3 pos) {
		
		GameObject obj = (GameObject)GameObject.Instantiate (go,pos,Quaternion.identity);
		obj.SetActive (true);

		if (obj.collider != null) {
			Bounds b = obj.collider.bounds;
			GraphUpdateObject guo = new GraphUpdateObject(b);
			AstarPath.active.UpdateGraphs (guo);
		}

		if (direct)
			AstarPath.active.FlushGraphUpdates();
	}
}
