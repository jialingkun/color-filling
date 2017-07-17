using UnityEngine;
using System.Collections;

public class ZoomImage : MonoBehaviour {
	//speed
	public float zoomSpeed = 0.001f;
	public float panSpeed = 0.1f;
	public float backToCenterSpeed = 0.3f;

	private RectTransform imageTransform;
	private float scaling = 1.0f;
	private Vector2 currentScale;
	private Vector2 currentPosition;

	private float defaultlimitX;
	private float defaultlimitY;
	private float limitX;  //pan limit,max image tranformation
	private float limitY;

	private float deltaPosX = 0.0f;
	private float deltaPosY = 0.0f;

	private Vector2 firstPos;
	// Use this for initialization
	void Start () {
		imageTransform = this.GetComponent<RectTransform> ();

		//initialize for object creation only
		currentScale = new Vector2 (imageTransform.localScale.x, imageTransform.localScale.y);
		currentPosition = new Vector2 (0,0);

		//default position limit
		RectTransform ImagePanel = imageTransform.parent.GetComponent<RectTransform> ();
		defaultlimitX = ImagePanel.offsetMax.x;
		defaultlimitY = ImagePanel.offsetMax.y;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.touchCount == 2)      //For Detecting Multiple Touch On Screen
		{
			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			// Change image scale
			scaling = imageTransform.localScale.x - (deltaMagnitudeDiff * zoomSpeed * Time.deltaTime * Screen.currentResolution.height);

			// code for maximum and minimum zoom limit
			scaling = Mathf.Max(scaling, 1.0f);
			scaling = Mathf.Min(scaling, 4.0f);

			//implement the scale value to image
			currentScale.x = scaling;
			currentScale.y = scaling;
			imageTransform.localScale = currentScale; 

			if (deltaMagnitudeDiff > 0.0f) { //zoom out back to center
				imageTransform.localPosition = Vector2.MoveTowards(imageTransform.localPosition, 
					Vector2.zero, 
					backToCenterSpeed * Time.deltaTime * deltaMagnitudeDiff * scaling * Screen.currentResolution.height);
			}

			//set x and y limit depend on scale
			limitX = defaultlimitX * (scaling-1);
			limitY = defaultlimitY * (scaling-1);
		}


		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)       //Single Touch Began
		{
			//store the first touch position
			Touch touch = Input.GetTouch(0);
			firstPos.x = touch.position.x;
			firstPos.y = touch.position.y;
		}

		if (Input.touchCount == 1 && (Input.GetTouch(0).phase == TouchPhase.Moved ||Input.GetTouch(0).phase == TouchPhase.Stationary)) //single touch move or hold     //Single Touch Moved
		{
			
			if (scaling > 1.01f)
			{
				//store the next touch position
				Touch touch = Input.GetTouch(0);
				Vector2 secPos;
				secPos.x = touch.position.x;
				secPos.y = touch.position.y;

				//calculate panning value
				deltaPosX = (secPos.x - firstPos.x)*Time.deltaTime*panSpeed* Screen.currentResolution.height;
				deltaPosY = (secPos.y - firstPos.y)*Time.deltaTime*panSpeed* Screen.currentResolution.height;

				//add panning value to current position
				currentPosition.x = imageTransform.localPosition.x + deltaPosX;
				currentPosition.y = imageTransform.localPosition.y + deltaPosY;

				//limit panning
				currentPosition.x = Mathf.Min (currentPosition.x, limitX);
				currentPosition.x = Mathf.Max (currentPosition.x, -limitX);

				currentPosition.y = Mathf.Min (currentPosition.y, limitY);
				currentPosition.y = Mathf.Max (currentPosition.y, -limitY);

				//implement panning position
				imageTransform.localPosition = currentPosition;
			}
		}

	}
}
