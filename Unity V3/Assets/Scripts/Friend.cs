using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Friend : Character
{
	public bool moving; 		// Ordre de déplacement
	public bool moveAndAttack; 	// Attaque durant les déplacements
	protected GameObject[] rooms;

	protected override void Start ()
	{
		base.Start ();

		moving           = false;
		moveAndAttack    = false;
		tagToAttack      = "EnnemiTAG";
		colorGizmoTarget = Color.blue;
		damage           = 5.0f;
		loot             = 0;
		rooms            = GameObject.FindGameObjectsWithTag ("RoomTAG");
	}

	void OnTriggerStay (Collider col)
	{
		if (col.tag == tagToAttack) {
			if (moveAndAttack || !moving) {
				myTransformPosition.position = col.gameObject.transform.position;
				scriptPath.target = col.gameObject.transform;
				scriptPath.canSearch = true;
			}
//			} else if (!moving) {
//				myTransformPosition = col.gameObject.transform;
//				scriptPath.target = col.gameObject.transform;
//				scriptPath.canSearch = true;
//			}

			//scriptPath.target = col.gameObject.transform;
			//scriptPath.canSearch = true;
		}
	}
	
	protected override void Update ()
	{
		if (Data.isPaused) { return; }

		base.Update ();

		if (scriptPath.target != null && target == null) {
			//Debug.Log ("Ma cible est DETRUITE");
			scriptPath.target = null;
		}

		if (scriptPath.target == null) {
			GameObject room = rooms[Random.Range(0,rooms.Length)];
			scriptPath.target = room.transform;
			scriptPath.target.tag = room.tag;
			scriptPath.canSearch = true;
			target = room;
		}

		// Si on a une ROOM en Target
		if(scriptPath.target != null  && scriptPath.target.tag == "RoomTAG") { 
			//Debug.Log ("2# Le coffre est a porte mon capitaine ! => " + scriptPath.target.position + " Et nous : " + pos);
			
			// Si la room est proche
			if ((pos.x < scriptPath.target.position.x + offsetLoot && pos.x > scriptPath.target.position.x - offsetLoot) &&
			    (pos.z < scriptPath.target.position.z + offsetLoot && pos.z > scriptPath.target.position.z - offsetLoot))
			{
				//Debug.Log ("Le coffre est a porte mon capitaine ! => " + scriptPath.target.position + " Et nous : " + pos);
				scriptPath.target = null;
				//loot += target.GetComponent<Chest>().loot;
				//Destroy(target);
				//AudioSource.PlayClipAtPoint(pikeChestClip, transform.position, 0.3f);
			}
		}

		// Si une de vos unités est loin de sa position de "Stand By"
		// et qu'il n'a Pas de cible
		// on lui fait rejoindre cette position
		if (scriptPath != null && moving && !moveAndAttack && scriptPath.target != null) 
		{
			if (!((scriptPath.target.transform.position.x < pos.x + offsetMove && scriptPath.target.transform.position.x > pos.x - offsetMove) &&
			      (scriptPath.target.transform.position.z < pos.z + offsetMove && scriptPath.target.transform.position.z > pos.z - offsetMove)))
			{
//				myTransformPosition.position = stayAt;
//				scriptPath.target = myTransformPosition.transform;
//				scriptPath.canSearch = true;
//				moving = true;
				//Debug.Log ("NOT STAND BY MEC ! TARGET POS => " + scriptPath.target.transform.position + " POS => " + pos);
			}
			else // Sinon cela signifie qu'il n'est pas en mouvement
			{
				moving = false;
				animator.SetBool("moving", false);
			}
		}
	}
	
}



