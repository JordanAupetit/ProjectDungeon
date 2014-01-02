using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ennemi : Character
{
	private GameObject[] chests;

	protected override void Start ()
	{
		base.Start ();

		tagToAttack = "FriendlyTAG";
		colorGizmoTarget = Color.red;
		damage = 15.0f;

		chests = GameObject.FindGameObjectsWithTag ("ChestTAG");
	}
	
	protected override void Update ()
	{
		base.Update ();

		if (scriptPath.target == null) {

			// ============================
			// Systeme a MODIFIER !!! <<<<<

			foreach(GameObject chest in chests){
				if(chest != null) {
					Debug.Log ("GO CHEST");
					
					myTransformPosition.position = chest.transform.position;
					scriptPath.target = myTransformPosition.transform;
					scriptPath.target.tag = chest.tag;
					scriptPath.canSearch = true;
					target = chest;
					
					break;
				}
			}
		}

		//Debug.Log ("Tag Target => " + scriptPath.target.tag);

		// Si on a un coffre en Target
		if(scriptPath.target != null  && scriptPath.target.tag == "ChestTAG") { 
			//Debug.Log ("2# Le coffre est a porte mon capitaine ! => " + scriptPath.target.position + " Et nous : " + pos);
			
			// Si le coffre est proche
			if ((pos.x < scriptPath.target.position.x + offsetLoot && pos.x > scriptPath.target.position.x - offsetLoot) &&
			    (pos.y < scriptPath.target.position.y + offsetLoot && pos.y > scriptPath.target.position.y - offsetLoot))
			{
				Debug.Log ("Le coffre est a porte mon capitaine ! => " + scriptPath.target.position + " Et nous : " + pos);
				scriptPath.target = null;
				Destroy(target);
			}
		}
	}
	
}


