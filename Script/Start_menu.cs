using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Start_menu : MonoBehaviour {
	private GameData permanentData;
	private GameObject collectionButton;
	private GameObject content;

	// Use this for initialization
	void Start () {
		//global data
		permanentData = GameObject.Find ("permanentData").GetComponent<GameData>();
		content = GameObject.Find ("Content");
		RectTransform collectionButtonTransform;
		float CollectionButtonOffset = permanentData.collectionPrefab.GetComponent<RectTransform> ().sizeDelta.x;
		Vector2 CollectionButtonPosition = new Vector2 (CollectionButtonOffset/2+CollectionButtonOffset/8, 0);
		int CollectionLength = permanentData.collection.Count;
		for (int i = 0; i < CollectionLength; i++) {
			collectionButton = Instantiate (permanentData.collectionPrefab);

			//fix bug call by reference on clickedit() parameter
			int index = i;

			collectionButton.GetComponent<Button> ().onClick.AddListener (delegate() {
				clickEdit(index);
			});

			collectionButton.GetComponent<RawImage> ().texture = permanentData.collection [i].tex;
			collectionButtonTransform = collectionButton.GetComponent<RectTransform> ();
			collectionButtonTransform.SetParent (content.GetComponent<RectTransform> (),false);
			collectionButtonTransform.anchoredPosition = CollectionButtonPosition;
			CollectionButtonPosition.x = CollectionButtonPosition.x + CollectionButtonOffset + CollectionButtonOffset / 5;
		}



	}

	public void clickStart(){
		SceneManager.LoadScene (2);
	}

	public void clickEdit(int localIndex){
		print ("HEI " + localIndex);
	}
}
