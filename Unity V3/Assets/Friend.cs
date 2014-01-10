using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Friend : Character
{
	protected override void Start ()
	{
		base.Start ();

		tagToAttack = "EnnemiTAG";
		colorGizmoTarget = Color.blue;
		damage = 25.0f;
		loot = 0;
	}
	
	protected override void Update ()
	{
		base.Update ();

		// Si une de vos unités est loin de sa position de "Stand By"
		// et qu'il n'a Pas de cible
		// on lui fait rejoindre cette position
		if (scriptPath != null && scriptPath.target == null) 
		{
			if (!((pos.x < stayAt.x + offsetMove && pos.x > stayAt.x - offsetMove) &&
			      (pos.y < stayAt.y + offsetMove && pos.y > stayAt.y - offsetMove)))
			{
				myTransformPosition.position = stayAt;
				scriptPath.target = myTransformPosition.transform;
				scriptPath.canSearch = true;
				moving = true;
			}
			else // Sinon cela signifie qu'il n'est pas en mouvement
			{
				moving = false;
			}
		}
	}
	
}



