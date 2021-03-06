﻿using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
    
    public float spawnRadius = 1;
    public int numberOfAgents = 50;
    public GameObject unitPrefab;

    void Start () {
        for (int i = 0; i < numberOfAgents; i++) {
            // Choose a random location within the spawnRadius
            Vector2 randomLoc2d = Random.insideUnitCircle * spawnRadius;
            Vector3 randomLoc3d = new Vector3(transform.position.x + randomLoc2d.x, transform.position.y, transform.position.z + randomLoc2d.y);

            // Make sure the location is on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomLoc3d, out hit, 100, 1)) {
                randomLoc3d = hit.position;
            }

            // Instantiate and make the enemy a child of this object
            GameObject o = (GameObject)Instantiate(unitPrefab, randomLoc3d, transform.rotation);
        }
    }
/*
    void OnDrawGizmosSelected () {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere (transform.position, spawnRadius);
    }*/
}