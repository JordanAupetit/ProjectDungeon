using UnityEngine;
using System.Collections;

public class UnitMovement : MonoBehaviour {

    public Transform model;
    public Transform proxy;
    public float reDetect;
    public float LOS;
    public float damage;

    protected GameObject targetParent;
    protected Transform target;
    protected NavMeshAgent agent;
    protected NavMeshObstacle obstacle;
    protected Vector3 lastPosition;

    //protected bool killTarget;
    protected float lifeMax;
    protected float life;
    protected float loot;          // Butin du personnage

    //protected GameObject lifeCapsule;
    //protected GameObject parentGO;
    //protected Collider lastCol;

    protected float clockAttack = 0;
    protected float timeToAttack;
    public string tagToAttack;
    protected Color colorGizmoTarget;
    protected bool mustAttack;
    protected float lifeToBack; // 10 => Back at 10% of life || 50 Back at 50% of life
    protected float distanceToAttack;
    //protected RaycastHit hit;

    protected GameObject[] targets;
    protected GameObject[] rooms;
    protected float clockDetect = 0;

    void Start () {
        agent            = proxy.GetComponent< NavMeshAgent >();
        obstacle         = proxy.GetComponent< NavMeshObstacle >();
        //obstacle.enabled = true;
        
        LOS              = 15.0f;
        damage           = 25.0f;
        reDetect         = 0.5f;
        timeToAttack     = 0.5f;
        mustAttack       = true;
        lifeToBack       = 50;
        life             = 100.0f;
        lifeMax          = life;
        loot             = 75.0f;
        distanceToAttack = 10.0f; // Distance pour un CAC
        
        rooms            = GameObject.FindGameObjectsWithTag ("RoomTAG");
        target = null;
    }

    public bool Damage(float dmg, UnitMovement attacker)
    {
        life -= dmg;

        //Debug.Log("Il me reste : " + life + " Damage ? "+ damage);
        
        if(life <= 0)
        {
            if(tagToAttack == "FriendlyTAG") {
                Data.gold += this.loot;
                Debug.Log ("JE MEURT et je donne mes gold : " + this.loot);
            } else if(tagToAttack == "EnnemiTAG") {
                attacker.loot += this.loot;
            }

            Destroy(gameObject);
            
            return true;
        }
        
        return false;
    }

    public Vector3 getPos()
    {
        return model.transform.position;
    }

/*    void OnDrawGizmos(){
        if (target != null && target.tag == tagToAttack)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine (model.transform.position, target.position);
        }
    }*/

    void Update () 
    {

        clockDetect += Time.deltaTime;
        clockAttack += Time.deltaTime;

        if (target == null) {
            GameObject room = rooms[UnityEngine.Random.Range(0,rooms.Length)];
            target = room.transform;

            //Debug.Log ("new target");
        } else {

            if (clockDetect >= reDetect && target.tag != tagToAttack) {
                targets = GameObject.FindGameObjectsWithTag (tagToAttack);

                for(int i = 0; i < targets.Length; i++) {
                    UnitMovement um = targets[i].GetComponent< UnitMovement >();
                    if (um != null) {
                        Vector3 targetPos = um.getPos();
                        Vector3 pos = getPos();
                        //Debug.Log(targetPos.x + " && " + pos.x);
                        //Debug.Log(targetPos.z + " && " + pos.z);
                        
                        if (    Mathf.Abs(targetPos.x) > (Mathf.Abs(pos.x) - LOS) &&
                                Mathf.Abs(targetPos.x) < (Mathf.Abs(pos.x) + LOS) &&
                                Mathf.Abs(targetPos.z) > (Mathf.Abs(pos.z) - LOS) &&
                                Mathf.Abs(targetPos.z) < (Mathf.Abs(pos.z) + LOS))
                        {
                            //Debug.Log("Collision");
                            target = um.model.transform;
                            targetParent = targets[i];
                        }
                    }
                }

                clockDetect = 0;
            }

            // Test if the distance between the agent (which is now the proxy) and the target
            // is less than the attack range (or the stoppingDistance parameter)
            if ((target.position - proxy.position).sqrMagnitude < Mathf.Pow(agent.stoppingDistance, 2)) {

                if (target.tag == "RoomTAG") {
                    target = null;
                } else if (target.tag == tagToAttack || (targetParent != null && targetParent.tag == tagToAttack)) {

                    if(clockAttack > timeToAttack){
                        //Debug.Log("JE TABASSE ! : " + life);

                        if(targetParent.GetComponent< UnitMovement >().Damage(damage, this)) {
                            targetParent = null;
                            target = null;
                        }

                        /*if(tagToAttack == "FriendlyTAG") {
                            scriptPath.target.GetComponentInChildren<Friend>().Damage(damage, this);
                        } else if(tagToAttack == "EnnemiTAG") {
                            scriptPath.target.GetComponentInChildren<Ennemi>().Damage(damage, this);
                        }*/

                        clockAttack = 0;
                    }

                //} else {
                    // If the agent is in attack range, become an obstacle and
                    // disable the NavMeshAgent component
                    obstacle.enabled = true;
                    agent.enabled = false;
                }
            } else {
                // If we are not in range, become an agent again
                obstacle.enabled = false;
                agent.enabled = true;
              
                // And move to the target's position
                agent.destination = target.position;
            }
                
            model.position = Vector3.Lerp(model.position, proxy.position, Time.deltaTime * 6);

            // Calculate the orientation based on the velocity of the agent
            Vector3 orientation = model.position - lastPosition;

            // Check if the agent has some minimal velocity
            if (orientation.sqrMagnitude > 0.1f) {
                // We don't want him to look up or down
                orientation.y = 0;
                // Use Quaternion.LookRotation() to set the model's new rotation and smooth the transition with Quaternion.Lerp();
                model.rotation = Quaternion.Lerp(model.rotation, Quaternion.LookRotation(model.position - lastPosition), Time.deltaTime * 8);
            } else {
                // If the agent is stationary we tell him to assume the proxy's rotation
                model.rotation = Quaternion.Lerp(model.rotation, Quaternion.LookRotation(proxy.forward), Time.deltaTime * 8);
            }

            // This is needed to calculate the orientation in the next frame
            lastPosition = model.position;

        }
    }
}