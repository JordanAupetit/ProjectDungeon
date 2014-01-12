using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Data : MonoBehaviour
{
	public static float gold;

	protected void Start ()
	{
		gold = 1000; // Or de départ
	}
	
	protected void Update ()
	{
		// rien
	}

	void OnGUI() {
		GUI.Box(new Rect (5, 5, 80, 30), "");
		GUI.Label(new Rect (10, 10, 70, 20), "Gold : " + (int)gold);
	}
}




