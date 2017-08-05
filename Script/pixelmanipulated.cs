using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

//using System.Diagnostics;

public class pixelmanipulated : MonoBehaviour {

	private GameData permanentData;
	private int stageID;
	private int filenameID;
	private bool isFinish;
	private GameObject backgroundPalette;
	private GameObject backgroundOperation;
	private GameObject backgroundFinish;
	private GameObject backgroundFinishMessage;
	private int activePaletteNumber;
	private GameObject leftPalette;
	private GameObject rightPalette;
	private GameObject[] paletteObject;



	//fill

	//struct to store pixel point as integer
	public struct intVector2{
		public int x, y;
	}

	private Texture2D originalTexture;

	private Texture2D currentTexture;
	private Color selectedColor;

	//threshold to bug fix color RGB value not equal by small margin
	private float diffThreshold;

	private string coloredPath;

	private Color nodeColor;


	//zoom and pan

	//canvas scaler to fix diffferent speed in different screen issue
	private CanvasScaler canvasScaler;
	private Vector2 ScreenScale;


	//speed
	public float zoomSpeed = 5f;
	public float panSpeed = 250f;

	public float nudgeRange=0.5f;

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

	private Touch touch;
	private Touch touchZero;
	private Touch touchOne;
	private Vector2 firstPos;

	private Vector2 secPos;




	//delay fill
	public float holdRate = 0.1f;
	private float nextWait = 0.0f;

	//specific touch
	private bool isTouching = false;


	//undo redo

	//struct to store pixel point as integer
	public struct oldState{
		public intVector2 cursor;
		public Color color;
	}

	private Stack<oldState> undoStack;
	private Stack<oldState> redoStack;
	private oldState lastUndo;




	// Use this for initialization
	void Start () {
		//global data
		permanentData = GameObject.Find ("permanentData").GetComponent<GameData>();
		stageID = PlayerPrefs.GetInt ("stageID");
		filenameID = PlayerPrefs.GetInt ("filenameID");
		originalTexture = permanentData.stageImages [stageID];

		backgroundOperation = GameObject.Find ("BackgroundOperation");
		backgroundPalette = GameObject.Find ("BackgroundPalette");
		backgroundFinish = GameObject.Find ("BackgroundFinish");
		backgroundFinishMessage = GameObject.Find ("BackgroundFinishMessage");

		backgroundFinish.SetActive (false);
		backgroundFinishMessage.SetActive (false);

		activePaletteNumber = 0;
		leftPalette = GameObject.Find ("Left");
		rightPalette = GameObject.Find ("Right");
		paletteObject = new GameObject[3];
		paletteObject[0] = GameObject.Find ("1");
		paletteObject[1] = GameObject.Find ("2");
		paletteObject[2] = GameObject.Find ("3");

		leftPalette.SetActive (false);
		paletteObject[1].SetActive (false);
		paletteObject[2].SetActive (false);

		isFinish = false;
		isTouching = false;


		//fill

		//setting temp texture width and height 
		currentTexture = new Texture2D (originalTexture.width, originalTexture.height);

		//load texture from disk
		coloredPath = Application.persistentDataPath + "/ColoredPictures";
		Texture2D tempTexture = load (coloredPath + "/"+ filenameID +".png");
		if (tempTexture != null) {
			currentTexture = tempTexture;
		} else {

			//fill the new texture with the original one (to avoid "empty" pixels)
			for (int y =0; y<currentTexture.height; y++) {
				for (int x = 0; x<currentTexture.width; x++) {
					if (originalTexture.GetPixel (x, y).grayscale < 0.8f) { //1 = black, 0 = white
						currentTexture.SetPixel (x, y, Color.black);
					} else {
						currentTexture.SetPixel (x, y, Color.white);
					}
				}
			}

		}

		//fill the new texture with the original one (to avoid "empty" pixels)
		for (int y =0; y<currentTexture.height; y++) {
			for (int x = 0; x<currentTexture.width; x++) {
				if (originalTexture.GetPixel (x, y).grayscale < 0.8f) { //1 = black, 0 = white
					currentTexture.SetPixel (x, y, Color.black);
				} else {
					currentTexture.SetPixel (x, y, Color.white);
				}
			}
		}

		diffThreshold = 0.08f;


		selectedColor = Color.green;
		//print ("texture size: "+currentTexture.width + "x" + currentTexture.height);
		//Apply 
		currentTexture.Apply ();
		//change the object main texture
		this.GetComponent<RawImage>().texture = currentTexture;


		//zoom and pan

		imageTransform = this.GetComponent<RectTransform> ();

		//initialize for object creation only
		currentScale = new Vector2 (imageTransform.localScale.x, imageTransform.localScale.y);
		currentPosition = new Vector2 (0,0);

		//default position limit
		//RectTransform ImagePanel = imageTransform.parent.GetComponent<RectTransform> ();
		defaultlimitX = imageTransform.offsetMax.x;
		defaultlimitY = imageTransform.offsetMax.y;

		canvasScaler = GetComponentInParent<CanvasScaler> ();
		ScreenScale = new Vector2(canvasScaler.referenceResolution.x / Screen.width, canvasScaler.referenceResolution.y / Screen.height);


		//undo redo
		undoStack = new Stack<oldState>();
		redoStack = new Stack<oldState>();




	}


	// Update is called once per frame
	void Update () {
		if (isTouching && !isFinish) {


			if (Input.touchCount == 2)      //For Detecting Multiple Touch On Screen
			{
				if (Input.GetTouch(0).phase == TouchPhase.Began) {
					//set fill delay parameter
					nextWait = Time.time + holdRate;
				}


				// Store both touches.
				touchZero = Input.GetTouch(0);
				touchOne = Input.GetTouch(1);


				// Find the position in the previous frame of each touch.
				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

				// Find the difference in the distances between each frame.
				float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

				// Change image scale
				scaling = imageTransform.localScale.x - (deltaMagnitudeDiff * ScreenScale.x * ScreenScale.y * zoomSpeed * Time.deltaTime);

				// code for maximum and minimum zoom limit
				scaling = Mathf.Max(scaling, 1.0f);
				scaling = Mathf.Min(scaling, 6f);

				//implement the scale value to image
				currentScale.x = scaling;
				currentScale.y = scaling;
				imageTransform.localScale = currentScale; 

				//set x and y limit depend on scale
				limitX = defaultlimitX * (scaling-1);
				limitY = defaultlimitY * (scaling-1);

				//get image position
				currentPosition.x = imageTransform.localPosition.x;
				currentPosition.y = imageTransform.localPosition.y;

				//limit image position
				currentPosition.x = Mathf.Min (currentPosition.x, limitX);
				currentPosition.x = Mathf.Max (currentPosition.x, -limitX);

				currentPosition.y = Mathf.Min (currentPosition.y, limitY);
				currentPosition.y = Mathf.Max (currentPosition.y, -limitY);

				//implement limit position
				imageTransform.localPosition = currentPosition;
			}


			if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)       //Single Touch Began
			{
				//set fill delay parameter
				nextWait = Time.time + holdRate;

				//store the first touch position
				touch = Input.GetTouch(0);
				firstPos.x = touch.position.x;
				firstPos.y = touch.position.y;
			}

			if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved) //single touch move or hold     //Single Touch Moved
			{

				if (scaling > 1.01f)
				{
					//store the next touch position
					touch = Input.GetTouch(0);
					secPos.x = touch.position.x;
					secPos.y = touch.position.y;

					//calculate range from first touch, normalize by screen resolution
					deltaPosX = (secPos.x - firstPos.x) * ScreenScale.x;
					deltaPosY = (secPos.y - firstPos.y) * ScreenScale.y;

					if (Mathf.Abs(deltaPosX)>nudgeRange && Mathf.Abs(deltaPosY)>nudgeRange) { //Avoid moving while finger only nudge a little
						//calculate panning value
						deltaPosX = deltaPosX*Time.deltaTime*panSpeed;
						deltaPosY = deltaPosY*Time.deltaTime*panSpeed;

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

						//refresh first position
						firstPos.x = secPos.x;
						firstPos.y = secPos.y;
					}

				}
			}
			/*
			if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Stationary) {
				//store the first touch position
				touch = Input.GetTouch(0);
				firstPos.x = touch.position.x;
				firstPos.y = touch.position.y;
			}
			*/
		}


	}


	public void coloringScreenTouched(){
		isTouching = true;
	}

	public void coloringScreenReleased(){
		isTouching = false;
	}


	//fill
	
	public void DebugPoint(BaseEventData data){
		
		coloringScreenReleased ();

		if (Time.time < nextWait && !isFinish) { //fill delay condition
			
			PointerEventData ped = ( PointerEventData )data;
			Vector2 localCursor;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), ped.position, ped.pressEventCamera, out localCursor))
				return;

			//print("LocalCursor:" + localCursor);

			/*
		 * rumus convert cursor ke dimensi Texture
		 * xTexture = xCursor/widthRecTransform*widthTexture
		 * 
		 * Convert local cursor to 0,0 pivot standart
		 * xcursor = localCursor.x + defaultlimitX
		 */
			intVector2 cursorPosition;
			cursorPosition.x = (int)((localCursor.x + defaultlimitX)/this.GetComponent<RectTransform> ().sizeDelta.x*currentTexture.width);
			cursorPosition.y = (int)((localCursor.y + defaultlimitY)/this.GetComponent<RectTransform> ().sizeDelta.y*currentTexture.height);

			//Stopwatch stopwatch = Stopwatch.StartNew ();

			//check if there any change before storing to undo stack
			nodeColor = currentTexture.GetPixel (cursorPosition.x, cursorPosition.y);
			if (!colorEqual (nodeColor, Color.black) &&
			    !colorEqual (nodeColor, selectedColor) &&
			    cursorPosition.x >= 0 && cursorPosition.y >= 0 &&
			    cursorPosition.x < currentTexture.width && cursorPosition.y < currentTexture.height) {

				//store to undo stack
				oldState currentState = new oldState();
				currentState.cursor = cursorPosition;
				currentState.color = nodeColor;
				undoStack.Push (currentState);

				//apply color filling
				floodFill (cursorPosition, selectedColor, currentTexture);
				currentTexture.Apply ();

				redoStack.Clear ();
			}

			//stopwatch.Stop ();

			//print ("Elapse time: "+stopwatch.ElapsedMilliseconds);
		}

	}

	private bool colorEqual(Color me, Color other)
	{
		//method to fix bug where RGB value not equal by small margin
		return Mathf.Abs (me.r - other.r) < diffThreshold && 
			Mathf.Abs (me.g - other.g) < diffThreshold && 
			Mathf.Abs (me.b - other.b) < diffThreshold && 
			Mathf.Abs (me.a - other.a) < diffThreshold;
	}

	private void floodFill(intVector2 startNode, Color newColor, Texture2D texture){
		Queue<intVector2> myQueue = new Queue<intVector2> ();
		myQueue.Enqueue (startNode);

		intVector2 node;
		intVector2 nextNode;

		while (myQueue.Count>0) {
			node = myQueue.Dequeue ();
			nodeColor = texture.GetPixel (node.x, node.y);

			//fill condition
			if (!colorEqual(nodeColor,Color.black) && 
				!colorEqual(nodeColor,newColor) &&
				node.x>=0 && node.y >=0 &&
				node.x<texture.width && node.y<texture.height) {

				//replace color
				texture.SetPixel (node.x, node.y, newColor);


				//north
				nextNode.x = node.x;
				nextNode.y = node.y + 1;
				myQueue.Enqueue (nextNode);

				//south
				nextNode.x = node.x;
				nextNode.y = node.y - 1;
				myQueue.Enqueue (nextNode);

				//west
				nextNode.x = node.x - 1;
				nextNode.y = node.y;
				myQueue.Enqueue (nextNode);

				//east
				nextNode.x = node.x + 1;
				nextNode.y = node.y;
				myQueue.Enqueue (nextNode);

			}
		}
	}

	public void clickColorPallete(){
		GameObject palleteObject = EventSystem.current.currentSelectedGameObject;
		selectedColor = palleteObject.GetComponent<Image> ().color;
	}

	public void clickSaveTodevice(){
		string filepath = Application.persistentDataPath + "/../../../../Pictures/ColoringBookSavedImage";
		try {
			byte[] bytes = currentTexture.EncodeToPNG();

			if (!Directory.Exists(filepath))
			{
				Directory.CreateDirectory(filepath);
			}

			File.WriteAllBytes(filepath + "/" + filenameID + ".png", bytes);
			//GameObject.Find("Savemessage").GetComponent<Text>().text = "Saved to \n"+ filepath;

			/*
			//REFRESHING THE ANDROID PHONE PHOTO GALLERY IS BEGUN
			AndroidJavaClass classPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject>("currentActivity");        
			AndroidJavaClass classUri = new AndroidJavaClass("android.net.Uri");        
			AndroidJavaObject objIntent = new AndroidJavaObject("android.content.Intent", new object[2]{"android.intent.action.MEDIA_MOUNTED", classUri.CallStatic<AndroidJavaObject>("parse", "file://" + filepath + "/colored.png")});        
			objActivity.Call ("sendBroadcast", objIntent);
			//REFRESHING THE ANDROID PHONE PHOTO GALLERY IS COMPLETE
			*/

		} catch (System.Exception ex) {
			//GameObject.Find("Savemessage").GetComponent<Text>().text = "Error \n"+ ex;
		}
	}



	public void clickUndo(){
		if (undoStack.Count>0) {
			//take state from undo stack
			lastUndo = undoStack.Pop ();

			//store old state to redo stack
			nodeColor = currentTexture.GetPixel (lastUndo.cursor.x, lastUndo.cursor.y);
			oldState currentState;
			currentState.cursor = lastUndo.cursor;
			currentState.color = nodeColor;
			redoStack.Push (currentState);

			//apply color filling
			floodFill (lastUndo.cursor, lastUndo.color, currentTexture);
			currentTexture.Apply ();
		}
	}

	public void clickRedo(){
		if (redoStack.Count>0) {
			lastUndo = redoStack.Pop ();

			//store old state to undo stack
			nodeColor = currentTexture.GetPixel (lastUndo.cursor.x, lastUndo.cursor.y);
			oldState currentState;
			currentState.cursor = lastUndo.cursor;
			currentState.color = nodeColor;
			undoStack.Push (currentState);

			//apply color filling
			floodFill (lastUndo.cursor, lastUndo.color, currentTexture);
			currentTexture.Apply ();
		}
	}


	public void clickShare(){
		string destination = coloredPath + "/colored.png";
		if(!Application.isEditor)
		{
			//if UNITY_ANDROID
			string body = "Body of message to be shared";
			string subject = "Subject of message";

			AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
			AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
			intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
			AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
			AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse","file://" + destination);
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), body );
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
			intentObject.Call<AndroidJavaObject>("setType", "image/jpeg");
			AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

			// run intent from the current Activity
			currentActivity.Call("startActivity", intentObject);
		}
	}

	public void clickExit(){
		SceneManager.LoadScene (2);
	}

	public void clickRight(){
		if (activePaletteNumber <=0) {
			leftPalette.SetActive (true);
		}
		if (activePaletteNumber < 2) {
			paletteObject [0].SetActive (false);
			paletteObject [1].SetActive (false);
			paletteObject [2].SetActive (false);
			activePaletteNumber++;
			paletteObject [activePaletteNumber].SetActive (true);
			if (activePaletteNumber >= 2) {
				rightPalette.SetActive (false);
			}
		}
	}

	public void clickLeft(){
		if (activePaletteNumber >=2) {
			rightPalette.SetActive (true);
		}
		if (activePaletteNumber > 0) {
			paletteObject [0].SetActive (false);
			paletteObject [1].SetActive (false);
			paletteObject [2].SetActive (false);
			activePaletteNumber--;
			paletteObject [activePaletteNumber].SetActive (true);
			if (activePaletteNumber <= 0) {
				leftPalette.SetActive (false);
			}
		}
	}

	public void clickFinish(){
		save ();
		backgroundFinish.SetActive (true);
		backgroundFinishMessage.SetActive (true);
		backgroundOperation.SetActive (false);
		backgroundPalette.SetActive (false);
	}

	public void save(){
		string filepath = coloredPath;
		try {
			byte[] bytes = currentTexture.EncodeToPNG();

			if (!Directory.Exists(filepath))
			{
				Directory.CreateDirectory(filepath);
			}

			File.WriteAllBytes(filepath + "/" + filenameID + ".png", bytes);
			if (SaveLoad.SaveCounter<filenameID) {
				SaveLoad.addCounter();
			}


			//GameObject.Find("Savemessage").GetComponent<Text>().text = "Saved to \n"+ filepath;
		} catch (System.Exception ex) {
			//GameObject.Find("Savemessage").GetComponent<Text>().text = "Error \n"+ ex;
		}
	}


	public Texture2D load(string path){
		Texture2D tex = null;
		byte[] fileData;

		if (File.Exists(path))     {
			fileData = File.ReadAllBytes(path);
			tex = new Texture2D(2, 2);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}
		return tex;
	}

}
