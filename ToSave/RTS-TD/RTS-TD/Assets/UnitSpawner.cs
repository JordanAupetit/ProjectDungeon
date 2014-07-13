using UnityEngine;
using System.Collections;

public class UnitSpawner : MonoBehaviour {

	public Transform target;
	public GameObject unit;
	
	public float spawnTime = .1f;
	float spawnTimeLeft = .1f;

	float cpt = 0;

	// Update is called once per frame
	void Update () {
		if(spawnTimeLeft <= 0 && cpt < 10000) {
			GameObject go = (GameObject)Instantiate(unit, transform.position, transform.rotation);
			//go.GetComponent<AstarAI>().target = target;
			go.GetComponent<AIPath>().target = target;
			spawnTimeLeft = spawnTime;
			cpt++;
		}
		else {
			spawnTimeLeft -= Time.deltaTime;
		}
	}
}
