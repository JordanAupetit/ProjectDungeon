using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ennemi : Character
{
	protected GameObject[] chests;
	protected Transform chestTransformPosition;
	//protected GameObject ennemiBase;
	protected GameObject[] enemyBases;
	public 	AudioClip pikeChestClip; // FOR TEST -- to remove

	protected override void Start ()
	{
		base.Start ();

		tagToAttack = "FriendlyTAG";
		colorGizmoTarget = Color.red;
		damage = 3.0f;
		loot = 75.0f;
		//ennemiBase = GameObject.Find ("EnnemiBase");
		enemyBases    = GameObject.FindGameObjectsWithTag ("EnemyBaseTAG");

		//chests = GameObject.FindGameObjectsWithTag ("ChestTAG");
	}
	
	protected override void Update ()
	{
		if (Data.isPaused) { return; }
			
		base.Update ();

		// Si la vie est < à la limite de Fuite
		if (life < (lifeMax * lifeToBack / 100)) {
			mustAttack = false;
			scriptPath.target = null;
		} else {
			mustAttack = true;
		}

		// Faire attention, c'est une opération qui Peut etre Lourde <<<
		chests = GameObject.FindGameObjectsWithTag ("ChestTAG");

		// Si on se dirige vers une "Cible" mais quelle n'existe plus
		if (scriptPath.target != null && target == null) {
			//Debug.Log ("Ma cible est DETRUITE");
			scriptPath.target = null;
		}
			

		if (scriptPath.target == null) {

			// ============================
			// Systeme a REVOIR !!! <<<<<

			foreach(GameObject chest in chests){
				if(chest != null) {
					//Debug.Log ("GO CHEST");

					if(chest.transform == null)
						break;

					//chestTransformPosition.position = chest.transform.position;
					//scriptPath.target = myTransformPosition.transform;
					scriptPath.target = chest.transform;
					scriptPath.target.tag = chest.tag;
					scriptPath.canSearch = true;
					target = chest;
					
					break;
				}
			}

			if(chests.Length <= 0) {
				//Debug.Log ("On rentre a la MAISON");
				target = enemyBases[Random.Range(0,enemyBases.Length)];
				myTransformPosition.position = target.transform.position;
				scriptPath.target = myTransformPosition.transform;
				scriptPath.target.tag = target.tag;
				scriptPath.canSearch = true;
				//target = null;
			}
				
		}

		//Debug.Log ("Tag Target => " + scriptPath.target.tag);

		// Si on a un coffre en Target
		if(scriptPath.target != null/*  && scriptPath.target.tag == "ChestTAG"*/) { 
			//Debug.Log ("2# Le coffre est a porte mon capitaine ! => " + scriptPath.target.position + " Et nous : " + pos);
			
			// Si le coffre est proche
			if ((pos.x < scriptPath.target.position.x + offsetLoot && pos.x > scriptPath.target.position.x - offsetLoot) &&
			    (pos.z < scriptPath.target.position.z + offsetLoot && pos.z > scriptPath.target.position.z - offsetLoot))
			{
				//Debug.Log ("Le coffre est a porte mon capitaine ! => " + scriptPath.target.position + " Et nous : " + pos);
				//loot += target.GetComponent<Chest>().loot;

				if(scriptPath.target.tag != null && scriptPath.target.tag == "ChestTAG"){
					scriptPath.target = null;
					Destroy(target);
					AudioSource.PlayClipAtPoint(pikeChestClip, transform.position, 0.3f);
				} else {
					scriptPath.target = null;
				}
			}
		}
	}
}


