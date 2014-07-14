using UnityEngine;
using System.Collections;

public class SpawnEnemy : MonoBehaviour {

	public float timeToSpawn;
	public GameObject enemyPrefab;

	float clockSpawn;
	GameObject[] enemyBases;

	void Start () {
		clockSpawn = 0.0f;
		timeToSpawn = 10.0f;
		enemyBases = GameObject.FindGameObjectsWithTag ("EnemyBaseTAG");
	}
	
	void Update () {
		clockSpawn += Time.deltaTime;

		if (clockSpawn > timeToSpawn) {
			Debug.Log("Spawn !");

			GameObject spawnBase = enemyBases[Random.Range(0,enemyBases.Length)];

			for (int i = 0; i < 10; i++) {
	            Vector2 randomLoc2d = Random.insideUnitCircle;
	            Vector3 randomLoc3d = new Vector3(spawnBase.transform.position.x + randomLoc2d.x, spawnBase.transform.position.y, spawnBase.transform.position.z + randomLoc2d.y);

	            // Make sure the location is on the NavMesh
	            NavMeshHit hit;
	            if (NavMesh.SamplePosition(randomLoc3d, out hit, 100, 1)) {
	                randomLoc3d = hit.position;
	            }

	            // Instantiate and make the enemy a child of this object
	            GameObject o = (GameObject)Instantiate(enemyPrefab, randomLoc3d, spawnBase.transform.rotation);
	        }

	        clockSpawn = 0.0f;
		}
	}
}
