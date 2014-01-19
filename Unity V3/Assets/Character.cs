using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
	
	//protected int nearEnnemi = 0;
	//protected Stack<GameObject> mind = new Stack<GameObject>();

	//protected Texture2D texture = Resources.Load("rouge.gif") as Texture2D; 

	public GameObject target = null;
	public Color myColor;
	public Vector3 stayAt;
	public Transform myTransformPosition;
	public bool canDraw = true;
	public float offsetMove;
	public float offsetLoot;
	public float loot; 			// Butin du personnage
	public AIPath scriptPath;

	protected bool killTarget;
	protected float lifeMax;
	protected float life;
	protected GameObject lifeCapsule;
	protected GameObject parentGO;
	protected Collider lastCol;
	protected float clockAttack = 0;
	protected float timeToAttack;
	protected string tagToAttack;
	protected Color colorGizmoTarget;
	protected float damage;
	protected Vector3 pos;
	protected bool mustAttack;
	protected float lifeToBack; // 10 => Back at 10% of life || 50 Back at 50% of life
	protected RaycastHit hit;
	protected float distanceToAttack;
	
	protected class LifeCapsule
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
	
	protected virtual void Start ()
	{
		timeToAttack = 0.5f;
		offsetMove = 1.0f;
		offsetLoot = 2.5f;
		myTransformPosition = new GameObject().transform;
		myTransformPosition.position = stayAt;
		
		//Debug.Log ("Ma Target est : " + target);
		
		parentGO = gameObject.transform.parent.gameObject;
		scriptPath = parentGO.GetComponent<AIPath>();
		
		//Debug.Log ("Mon parent est : " + parentGO.name + " Target : " + scriptPath.target);

		renderer.material.color = myColor;
		mustAttack = true;
		lifeToBack = 50;
		life = 100.0f;
		lifeMax = 100.0f;
		distanceToAttack = 10.0f; // Distance pour un CAC
		
		Vector3 pos = transform.position;
		
		LifeCapsule l = new LifeCapsule(pos);
		lifeCapsule = l.lifeC;
	}
	
	void OnTriggerStay (Collider col)
    {
		/*Debug.Log ("Mon tagAttaque est : " + tagToAttack);

		if(tagToAttack == "FriendlyTAG")
			Debug.Log ("ON A PAS ENCORE UNE CIBLE!");*/


//		if (col.tag == tagToAttack && mustAttack) {
//			if (moveAndAttack) {
//				scriptPath.target = col.gameObject.transform;
//				scriptPath.canSearch = true;
//			} else if (!moving) {
//				scriptPath.target = col.gameObject.transform;
//				scriptPath.canSearch = true;
//			}
//		}

		if (col.tag == tagToAttack && mustAttack) {
			scriptPath.target = col.gameObject.transform;
			scriptPath.canSearch = true;
		}
    }
	
	public bool Damage (float dmg, Character attacker) 
	{
		life -= dmg;
		
		if(life <= 0)
		{
			if(tagToAttack == "FriendlyTAG") {
				Data.gold += this.loot;
				Debug.Log ("JE MEURT et je donne mes gold : " + this.loot);
			} else if(tagToAttack == "EnnemiTAG") {
				attacker.loot += this.loot;
			}

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

	void OnDrawGizmos(){
		if (scriptPath != null && scriptPath.target != null && scriptPath.target.tag == tagToAttack)
		{
			Gizmos.color = colorGizmoTarget;
			Gizmos.DrawLine (transform.position, scriptPath.target.position);
		}
	}
	
	// Update is called once per frame
	protected virtual void Update ()
	{
		pos = transform.position;

		// On fait défiler l'horloge qui permet de savoir quand "Attaquer"
		clockAttack += Time.deltaTime;

		lifeCapsule.transform.position = new Vector3(pos.x, pos.y + 2, pos.z);
		lifeCapsule.transform.localScale = new Vector3(0.5f, life / 100 + 0.1f, 0.5f);

		// On inflige des dégats au corps a corps
		if (scriptPath != null && scriptPath.target != null && scriptPath.target.tag == tagToAttack)
		{		

			if(tagToAttack == "EnnemiTAG") {
				//Debug.Log ("DRAWLINE => to " + transform.position + " at " + scriptPath.target.position);
//				Debug.DrawLine(transform.position, scriptPath.target.position, Color.cyan);
//
//				RaycastHit hit;
//				if (Physics.Linecast(transform.position, scriptPath.target.position, out hit)) {
//					Debug.Log ("CONTACT !!!! => distance : " + hit.distance);
//				}
			}

			if (Physics.Linecast(transform.position, scriptPath.target.position, out hit)) {
				if (Mathf.Abs(hit.distance) <= distanceToAttack) {
					if(clockAttack > timeToAttack){
						//Debug.Log("JE TABASSE !");

						if(tagToAttack == "FriendlyTAG") {
							killTarget = scriptPath.target.GetComponentInChildren<Friend>().Damage(damage, this);
						} else if(tagToAttack == "EnnemiTAG") {
							killTarget = scriptPath.target.GetComponentInChildren<Ennemi>().Damage(damage, this);
						}

						if(killTarget)
							scriptPath.target = null;

						clockAttack = 0;
					}
				}
			}
		}
	}
	
}

