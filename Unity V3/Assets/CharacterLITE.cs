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
	Stack<GameObject> mind;
	
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
		Debug.Log ("Ma Target est : " + target);
		
		/*if(target != null)
			mind.Push(target);*/
		
		parentGO = gameObject.transform.parent.gameObject;
		scriptPath = parentGO.GetComponent<AIPath>();
		//scriptPath.target = target.transform;
		
		Debug.Log ("Mon parent est : " + parentGO.name + " Target : " + scriptPath.target);
		
		//movement = gameObject.GetComponent<AI_Comportement_V3>();
		renderer.material.color = myColor;
		
		Vector3 pos = transform.position;
		
		LifeCapsule l = new LifeCapsule(pos);
		lifeCapsule = l.lifeC;
		
		//field_of_view = FieldOfView.BuildFOV(this);
		
//		FieldOfView f = new FieldOfView(pos, this.gameObject);
//		field_of_view = f.fov;
		
		//field_of_view = new GameObject();
		//field_of_view.AddComponent("FieldOfView");
	}
	
	void OnTriggerEnter (Collider col)
    {
		/*if(col.tag == "EnnemiTAG") {
			Debug.Log ("Trig ENTER : " + col + " Tag : " + col.tag);
			renderer.material.color = Color.red;
			nearEnnemi++;
			scriptPath.target = col.gameObject.transform;
			//field_of_view.GetComponent<FieldOfView>().red();
		}*/
    }
	
	void OnTriggerExit (Collider col)
    {
		/*if(col.tag == "EnnemiTAG") {
			nearEnnemi--;
			
			if(nearEnnemi <= 0)
				renderer.material.color = Color.green;
		}*/
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
		//Debug.Log("Je suis : " + gameObject + " Life : " + life);
		Vector3 pos = transform.position;
		
		lifeCapsule.transform.position = new Vector3(pos.x, pos.y + 2, pos.z);
		lifeCapsule.transform.localScale = new Vector3(0.5f, life / 100 + 0.1f, 0.5f);
		
		//field_of_view.pos = pos;
		//fov.pos = pos;
		
		//field_of_view.transform.position = pos;
	}
}

