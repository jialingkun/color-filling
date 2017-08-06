using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class stageScript : MonoBehaviour {
	private RectTransform panel;
	private GameObject leftButton;
	private GameObject rightButton;
	private float limitx;
	private Vector2 nextPosition;

	public float transitionSpeed;

	private bool isMoving;
	// Use this for initialization
	void Start () {
		panel = this.GetComponent<RectTransform> ();
		leftButton = GameObject.Find ("Left");
		rightButton = GameObject.Find ("Right");

		if (panel.localPosition.x >= limitx/2) {
			leftButton.SetActive (false);
		}else if(panel.localPosition.x <= -limitx / 2){
			rightButton.SetActive (false);
		}

		limitx = panel.offsetMax.x;
		nextPosition = new Vector2 (0,panel.localPosition.y);
		isMoving = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (Vector2.Distance (panel.localPosition, nextPosition) < 0.3f && isMoving) {
			isMoving = false;

			if (panel.localPosition.x >= limitx/2.2) {
				leftButton.SetActive (false);
			} else {
				leftButton.SetActive (true);
			}

			if (panel.localPosition.x <= -limitx / 2.2) {
				rightButton.SetActive (false);
			} else {
				rightButton.SetActive (true);
			}

		} else if (isMoving) {
			panel.localPosition=Vector2.MoveTowards (panel.localPosition, nextPosition, Time.deltaTime * transitionSpeed * Screen.width);
		}

	}

	public void clickRight(){
		if (!isMoving && panel.localPosition.x > -limitx/2) {
			nextPosition.x = panel.localPosition.x - limitx / 3;
			isMoving = true;
		}

	}

	public void clickLeft(){
		if (!isMoving && panel.localPosition.x < limitx/2) {
			nextPosition.x = panel.localPosition.x + limitx / 3;
			isMoving = true;
		}
	}

	public void clickBack(){
		SceneManager.LoadScene (1);
	}


	public void clickStage(int stageID){
		PlayerPrefs.SetInt ("stageID", stageID);
		PlayerPrefs.SetInt ("filenameID", SaveLoad.SaveCounter+1);
		PlayerPrefs.SetInt ("collectionIndex", -1);
		SceneManager.LoadScene (3);
	
	}
}
