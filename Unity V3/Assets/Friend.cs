using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Friend : Character
{
	public bool moving; 		// Ordre de déplacement
	public bool moveAndAttack; 	// Attaque durant les déplacements

	protected override void Start ()
	{
		base.Start ();

		moving = false;
		moveAndAttack = false;
		tagToAttack = "EnnemiTAG";
		colorGizmoTarget = Color.blue;
		damage = 5.0f;
		loot = 0;
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
				//Debug.Log ("STAND BY MEC !");
			}
		}
	}
	
}



