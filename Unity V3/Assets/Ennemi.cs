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
		loot = 75.0f;

		//chests = GameObject.FindGameObjectsWithTag ("ChestTAG");
	}
	
	protected override void Update ()
	{
		base.Update ();

		// Faire attention, c'est une opération qui Peut etre Lourde <<<
		chests = GameObject.FindGameObjectsWithTag ("ChestTAG");

		// Si on se dirige vers une "Cible" mais quelle n'existe plus
		if (scriptPath.target != null && target == null) {
			Debug.Log ("Ma cible est DETRUITE");
			scriptPath.target = null;
		}
			

		if (scriptPath.target == null) {

			// ============================
			// Systeme a MODIFIER !!! <<<<<

			foreach(GameObject chest in chests){
				if(chest != null) {
					Debug.Log ("GO CHEST");

					if(chest.transform == null)
						break;

					myTransformPosition.position = chest.transform.position;
					scriptPath.target = myTransformPosition.transform;
					scriptPath.target.tag = chest.tag;
					scriptPath.canSearch = true;
					target = chest;
					
					break;
				}
			}

			if(chests.Length <= 0) {
				myTransformPosition.position = stayAt;
				scriptPath.target = myTransformPosition.transform;
				scriptPath.canSearch = true;
				target = null;
				Debug.Log ("On rentre a la MAISON");
			}
				
		}

		//Debug.Log ("Tag Target => " + scriptPath.target.tag);

		// Si on a un coffre en Target
		if(scriptPath.target != null  && scriptPath.target.tag == "ChestTAG") { 
			Debug.Log ("2# Le coffre est a porte mon capitaine ! => " + scriptPath.target.position + " Et nous : " + pos);
			
			// Si le coffre est proche
			if ((pos.x < scriptPath.target.position.x + offsetLoot && pos.x > scriptPath.target.position.x - offsetLoot) &&
			    (pos.z < scriptPath.target.position.z + offsetLoot && pos.z > scriptPath.target.position.z - offsetLoot))
			{
				Debug.Log ("Le coffre est a porte mon capitaine ! => " + scriptPath.target.position + " Et nous : " + pos);
				scriptPath.target = null;
				//loot += target.GetComponent<Chest>().loot;
				Destroy(target);
			}
		}
	}
	
}


