using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Data : MonoBehaviour
{
	public static float gold;
	public static bool isPaused;

	protected void Start ()
	{
		gold = 1000; // Or de départ
		isPaused = false;
	}
	
	protected void Update ()
	{
		// rien
	}

	void OnGUI() {
		GUI.Box(new Rect (5, 5, 80, 30), "");
		GUI.Label(new Rect (10, 10, 70, 20), "Gold : " + (int)gold);

		if (isPaused) {
			if (GUI.Button (new Rect (260, (Screen.height - 40), 80, 30), "Continue")) {
				isPaused = false;
				//Time.timeScale = 1.0f;
			}
		} else {
			if (GUI.Button (new Rect (260, (Screen.height - 40), 80, 30), "Pause")) {
				isPaused = true;
				//Time.timeScale = 0.0f;
			}
		}

	}
}




