using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterLITE : MonoBehaviour
{
	
	private float life = 100.0f;
	private GameObject lifeCapsule;
	public GameObject target = null;
	public Color myColor;
	private int nearEnnemi = 0;
	private GameObject parentGO;
	private AIPath scriptPath;
	private GameObject[] chests;
	public Transform myTransformPosition;
	
	private class LifeCapsule
	{
		public GameObject lifeC;
		
		public LifeCapsule(Vector3 pos)
		{
			lifeC = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			lifeC.transform.position = new Vector3(pos.x, pos.y + 2, pos.z);
			lifeC.transform.localScale = new Vector3(0.5f, 1.1f, 0.5f);
			lifeC.transform.Rotate(0, 0, 90);
			lifeC.renderer.material.color = Color.red;
		}
	}
	
	void Start ()
	{
		myTransformPosition = new GameObject().transform;
		myTransformPosition.position = new Vector3(0.0f,0.0f,0.0f);

		Debug.Log ("Ma Target est : " + target);
		
		parentGO = gameObject.transform.parent.gameObject;
		scriptPath = parentGO.GetComponent<AIPath>();
		
		Debug.Log ("Mon parent est : " + parentGO.name + " Target : " + scriptPath.target);
		
		renderer.material.color = myColor;
		
		Vector3 pos = transform.position;
		
		LifeCapsule l = new LifeCapsule(pos);
		lifeCapsule = l.lifeC;

		chests = GameObject.FindGameObjectsWithTag ("ChestTAG");
	}
	
	public bool Damage (float dmg) 
	{
		life -= dmg;
		
		if(life <= 0)
		{
			Destroy(lifeCapsule);
			Destroy(parentGO);
			Destroy(gameObject);
			
			return true;
		}
		
		return false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (scriptPath.target == null) {
			foreach(GameObject chest in chests){
				Debug.Log ("GO CHEST");

				myTransformPosition.position = chest.transform.position;
				scriptPath.target = myTransformPosition.transform;
				scriptPath.canSearch = true;

				break;
			}
		}

		Vector3 pos = transform.position;
		
		lifeCapsule.transform.position = new Vector3(pos.x, pos.y + 2, pos.z);
		lifeCapsule.transform.localScale = new Vector3(0.5f, life / 100 + 0.1f, 0.5f);
	}
}

