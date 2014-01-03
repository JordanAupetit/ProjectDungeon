using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Data : MonoBehaviour
{
	public float gold;
	private GameObject texteGO;
	private TextMesh texte;

	protected void Start ()
	{
		gold = 200; // Or de départ
		texteGO = GameObject.Find ("TextGold");
		texte = texteGO.GetComponent<TextMesh> ();
	}
	
	protected void Update ()
	{
		//Debug.Log ("Nous avons " + gold + " gold");
		texte.text = "Gold : " + (int)gold;
	}
	
}




