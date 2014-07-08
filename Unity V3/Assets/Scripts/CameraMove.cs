
using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {
	
	private float speed = 2.0f;
	private float moveZ = 0.0f;
	
    void Update () {
	
		inputMove();
	}
	
	void inputMove(){
		
		if(/*Input.GetKey("left") || */Input.GetKey("q")){
		
			transform.Translate(-speed,0,0);
		}
		else if(/*Input.GetKey("right") || */Input.GetKey("d")){
		
			transform.Translate(speed, 0, 0);
		}
	
		if(/*Input.GetKey("up") || */Input.GetKey("z")){
		
			transform.Translate(0, 0, -speed);
		}
		else if(/*Input.GetKey("down") || */Input.GetKey("s")){
		
			transform.Translate(0, 0, speed);
		}
		
		moveZ = Input.GetAxis("Mouse ScrollWheel") * speed * 10;
		transform.Translate(0, moveZ, 0);
	}
}