using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
	
	private float life = 100.0f;
	private GameObject lifeCapsule;
	//private GameObject field_of_view;
	public GameObject target = null;
	//public int intarget = 666;
 	//private FieldOfView characterFov = null;
	//private Component movement;
	public Color myColor;
	private int nearEnnemi = 0;
	private GameObject parentGO;
	private AIPath scriptPath;
	private Stack<GameObject> mind = new Stack<GameObject>();
	bool killTarget;
	private Collider lastCol;
	public Vector3 myPosition;
	private Transform myTransformPosition;
	public bool canDraw = true;
	
	//private Texture2D texture = Resources.Load("rouge.gif") as Texture2D; 
	
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
		myTransformPosition.position = myPosition;
		
		Debug.Log ("Ma Target est : " + target);
		
		parentGO = gameObject.transform.parent.gameObject;
		scriptPath = parentGO.GetComponent<AIPath>();
		//scriptPath.target = target.transform;
		
		Debug.Log ("Mon parent est : " + parentGO.name + " Target : " + scriptPath.target);
		
		/*if(scriptPath.target != null)
			mind.Push(scriptPath.target.gameObject);*/
		//Debug.Log ("Test START Stack : " + mind.Peek() + " Count : " + mind.Count);
		
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
		if(col.tag == "EnnemiTAG") {
			Debug.Log ("Trig ENTER : " + col + " Tag : " + col.tag);
			renderer.material.color = Color.red;
			nearEnnemi++;
			
			//Debug.Log ("Test Stack : " + mind.Peek() + " Count : " + mind.Count);
			
			//mind.Push(col.gameObject);
			
			//Debug.Log ("Test Stack : " + mind.Peek() + " Count : " + mind.Count);
			scriptPath.target = col.gameObject.transform;
			//scriptPath.target = mind.Peek().transform;
			scriptPath.canSearch = true;
		}
    }
	
	void OnTriggerStay (Collider col)
	{
		if(col.tag == "EnnemiTAG") {
			lastCol = col;
		}
	}
	
	void OnTriggerExit (Collider col)
    {
		if(col.tag == "EnnemiTAG") {
			Debug.Log ("Trig EXIT : " + col + " Tag : " + col.tag);
			nearEnnemi--;
			
			if(nearEnnemi <= 0)
				renderer.material.color = Color.green;
		}
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

	public void SetColor(Color col)
	{
		renderer.material.color = col;
	}
	
	// Update is called once per frame
	void Update ()
	{
		/*if(mind.Count > 0 && mind.Peek() != null) {
			scriptPath.target = mind.Peek().transform;
			scriptPath.canSearch = true;
		} else if(mind.Count > 0 && mind.Peek() == null) {
			mind.Pop();
		}*/
		
		//Debug.Log("My mind : " + mind);
		
		Vector3 pos = transform.position;
		
		lifeCapsule.transform.position = new Vector3(pos.x, pos.y + 2, pos.z);
		lifeCapsule.transform.localScale = new Vector3(0.5f, life / 100 + 0.1f, 0.5f);
		
		if(scriptPath.target == null) 
		{
			myTransformPosition.position = myPosition;
			scriptPath.target = myTransformPosition.transform;
			scriptPath.canSearch = true;
		}
		
		
		if (scriptPath.target != null && scriptPath.target.tag == "EnnemiTAG")
		{		
			//Debug.Log("Et 1 DAMAGE dans ta **** !");
			killTarget = scriptPath.target.GetComponentInChildren<CharacterLITE>().Damage(0.1f);
			
			/*if(nearEnnemi > 0 && lastCol != null) {
				scriptPath.target = lastCol.gameObject.transform;
				scriptPath.canSearch = true;
				return;
			}*/
			
			/*if(killTarget) {
				scriptPath.target = GameObject.Find("RedBase").transform;
				scriptPath.canSearch = true;
				nearEnnemi--;
			}*/
		}/* else if (scriptPath.target == null) {
			scriptPath.target = GameObject.Find("RedBase").transform;
			scriptPath.canSearch = true;
			nearEnnemi--;
		}*/
		
		//field_of_view.pos = pos;
		//fov.pos = pos;
		
		//field_of_view.transform.position = pos;
	}
	
	void OnDrawGizmos ()
	{
		if (scriptPath.target != null && scriptPath.target.tag == "EnnemiTAG")
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine (transform.position, scriptPath.target.position);
		}
	}
	
	
}

