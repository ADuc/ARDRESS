//http://stackoverflow.com/questions/29891252/unity3d-touch-rotate-object

using UnityEngine;
using System.Collections;

public class Touch_rotate : MonoBehaviour {
	private bool vertical;
	private bool horizontal;
	private Vector3 startPos;
	// Update is called once per frame
	void Update () {
		if (Input.touchCount == 1) {
			var touch = Input.GetTouch(0);
			switch(Input.GetTouch(0).phase){
			case TouchPhase.Moved:
				float swipeDistVertical = (new Vector3(0, touch.deltaPosition.y, 0) - new Vector3(0, startPos.y, 0)).magnitude;              
				if (swipeDistVertical > 0)                  
				{                   
					float swipeValue = Mathf.Sign(touch.deltaPosition.y - startPos.y);                   
					if (swipeValue > 0 || swipeValue < 0)//up swipe     
					{
						vertical = true;
						horizontal = false;
					}                               
				}               
				float swipeDistHorizontal = (new Vector3(touch.deltaPosition.x,0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;             
				if (swipeDistHorizontal > 0)                    
				{                   
					float swipeValue = Mathf.Sign(touch.deltaPosition.x - startPos.x);                   
					if (swipeValue > 0 || swipeValue < 0)//right swipe
					{
						horizontal = true;
						vertical = false;
					}                           
				}
				
				if(vertical)
				{
					transform.Rotate(touch.deltaPosition.y * 0.3f, 0,0,Space.World);
				}
				if(horizontal)
				{
					transform.Rotate(0,touch.deltaPosition.x * 0.3f,0,Space.World);
				}
				break;
			}
		}
	}
}
