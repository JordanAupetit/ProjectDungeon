using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chest : MonoBehaviour
{
	public float income;
	//public GameObject scripts;
	//public Data data;
	public float loot;
	
	protected void Start ()
	{
		income = 0.02f;
		//scripts = GameObject.Find ("World_Scripts");
		//data = scripts.GetComponent<Data>();
		loot = 300.0f;
	}
	
	protected void Update ()
	{
		if (Data.isPaused) { return; }

		Data.gold += income;
	}
	
}





