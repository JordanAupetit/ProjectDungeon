using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Data : MonoBehaviour
{
	public float gold;

	protected void Start ()
	{
		gold = 200;
	}
	
	protected void Update ()
	{
		Debug.Log ("Nous avons " + gold + " gold");
	}
	
}




