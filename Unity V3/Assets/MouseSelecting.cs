using UnityEngine;
using System.Collections;

public class MouseSelecting : MonoBehaviour
{

	RaycastHit hit;

	public static GameObject CurrentlySelectedUnit;

	public GameObject Target;

	private static Vector3 mouseDownPoint;

	void Awake()
	{
		mouseDownPoint = Vector3.zero;
	}
	
	void Update ()
	{
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if(Physics.Raycast(ray, out hit, Mathf.Infinity)){

			if(Input.GetMouseButtonDown(0))
			{
				mouseDownPoint = hit.point;
			}

			if(hit.collider.name == "Ground")
			{
				if(Input.GetMouseButtonUp(0) && DidUserClickLeftMouse(mouseDownPoint))
				{
					DeselectSelectedObject();
				}
			} else {

				if(Input.GetMouseButtonUp(0) && DidUserClickLeftMouse(mouseDownPoint))
				{
					//Debug.Log("Name ====> " + hit.collider.name);

					if(hit.collider.tag == "FriendlyTAG")
					{
						Debug.Log("We found a unit !!! ===> " + hit.collider.tag);

						if(CurrentlySelectedUnit != hit.collider.gameObject)
						{
							GameObject SelectedUnit = hit.collider.transform.gameObject;
							Character ch = SelectedUnit.GetComponent<Character>();
							ch.SetColor(Color.yellow);

							if(CurrentlySelectedUnit != null) 
								//CurrentlySelectedUnit.transform.FindChild("Capsule").gameObject.green();

							CurrentlySelectedUnit = hit.collider.gameObject;
						}
					} else {
						DeselectSelectedObject();
					}
				}
			}

		} else {
			if(Input.GetMouseButtonUp(0) && DidUserClickLeftMouse(mouseDownPoint))
			{
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

		}
	}

}

































