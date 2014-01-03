using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chest : MonoBehaviour
{
	public float income;
	public GameObject scripts;
	public Data data;
	
	protected void Start ()
	{
		income = 0.005f;
		scripts = GameObject.Find ("World_Scripts");
		data = scripts.GetComponent<Data>();
	}
	
	protected void Update ()
	{
		data.gold += income;
	}
	
}





