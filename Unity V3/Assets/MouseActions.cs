using UnityEngine;
using System.Collections;

public class MouseActions : MonoBehaviour
{

	private RaycastHit hit;
	private int selectLayer;
	private static Vector3 mouseDownPoint;

	public static GameObject CurrentlySelectedUnit;
	public GameObject Target;

	void Awake()
	{
		mouseDownPoint = Vector3.zero;
		selectLayer = 11; // Character
	}
	
	void Update ()
	{
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if(Physics.Raycast(ray, out hit, Mathf.Infinity)){

			if(Input.GetMouseButtonDown(0))
			{
				mouseDownPoint = hit.point;
			}

			// On effectue un clique gauche
			if(Input.GetMouseButtonUp(0) && DidUserClickLeftMouse(mouseDownPoint))
			{
				Debug.Log("Name ====> " + hit.collider.name + " Tag => " + hit.collider.tag);
				
				if(hit.collider.tag == "FriendlyTAG")
				{
					Debug.Log("We found a unit !!! ===> " + hit.collider.tag);

					//if(CurrentlySelectedUnit != hit.collider.gameObject)
					//{
						DeselectSelectedObject();

						GameObject SelectedUnit = hit.collider.transform.gameObject;
						Character ch = SelectedUnit.GetComponentInChildren<Character>();
						ch.SetColor(Color.yellow);

						//if(CurrentlySelectedUnit != null) 
							//CurrentlySelectedUnit.transform.FindChild("Capsule").gameObject.green();

						CurrentlySelectedUnit = hit.collider.transform.gameObject;
					//}
				}
				else {
					Debug.Log ("DESELECTION ! ");
					DeselectSelectedObject();
				}
			}

			// On effectue un clique droit
			if(Input.GetMouseButtonUp(1) && DidUserClickLeftMouse(mouseDownPoint))
			{
				if(CurrentlySelectedUnit != null){
					if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
						
						// Si on fait un clique sur le sol, alors on change la position de "Stand By" du character
						if(hit.collider.name == "Ground")
						{
							//Debug.Log("GO HERE AND PUT THE BANANA DOWN ! ");
							Character ch = CurrentlySelectedUnit.GetComponentInChildren<Character>();
							
							ch.stayAt.x = hit.point.x;
							ch.stayAt.z = hit.point.z;
							ch.myTransformPosition.position = ch.stayAt;
							ch.moving = true;
						}
					}
				}
			}

		} else {
			if(Input.GetMouseButtonUp(0) && DidUserClickLeftMouse(mouseDownPoint))
			{
				Debug.Log ("DESELECTION ! ");
				DeselectSelectedObject();
			}
		}

		Debug.DrawRay(ray.origin, ray.direction * Mathf.Infinity, Color.yellow);
	}


	public bool DidUserClickLeftMouse(Vector3 hitPoint)
	{
		float clickZone = 1.3f;

		if(
			(mouseDownPoint.x < hitPoint.x + clickZone && mouseDownPoint.x > hitPoint.x - clickZone) &&
			(mouseDownPoint.y < hitPoint.y + clickZone && mouseDownPoint.y > hitPoint.y - clickZone) &&
			(mouseDownPoint.z < hitPoint.z + clickZone && mouseDownPoint.z > hitPoint.z - clickZone)
		)
			return true; else return false;
	}

	public static void DeselectSelectedObject()
	{
		if(CurrentlySelectedUnit != null)
		{
			Character ch = CurrentlySelectedUnit.GetComponentInChildren<Character>();
			ch.SetColor(Color.green);
			CurrentlySelectedUnit = null;
		}
	}

}

































