using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class Start_menu : MonoBehaviour {
	private GameData permanentData;
	private GameObject collectionButton;
	private RectTransform content;
	private string coloredPath;
	public int padding;
	public int gap;

	// Use this for initialization
	void Start () {
		//global data
		permanentData = GameObject.Find ("permanentData").GetComponent<GameData>();
		coloredPath = Application.persistentDataPath + "/ColoredPictures";
		content = GameObject.Find ("Content").GetComponent<RectTransform> ();

		RectTransform collectionButtonTransform;
		float CollectionButtonOffset = permanentData.collectionPrefab.GetComponent<RectTransform> ().sizeDelta.x;
		Vector2 CollectionButtonPosition = new Vector2 (CollectionButtonOffset/2+CollectionButtonOffset/padding, 0);
		int CollectionLength = permanentData.collection.Count;
		for (int i = 0; i < CollectionLength; i++) {
			collectionButton = Instantiate (permanentData.collectionPrefab);

			//fix bug call by reference on clickedit() parameter
			int index = i;
			GameObject edit = collectionButton.transform.Find("Edit").gameObject;
			GameObject delete = collectionButton.transform.Find("Delete").gameObject;
			edit.SetActive (false);
			delete.SetActive (false);
			collectionButton.GetComponent<Button> ().onClick.AddListener (delegate() {
				clickCollection(edit, delete);
			});
			edit.GetComponent<Button> ().onClick.AddListener (delegate() {
				clickEdit(index);
			});
			delete.GetComponent<Button> ().onClick.AddListener (delegate() {
				clickDelete(index);
			});

			collectionButton.GetComponent<RawImage> ().texture = permanentData.collection [i].tex;
			collectionButtonTransform = collectionButton.GetComponent<RectTransform> ();
			collectionButtonTransform.SetParent (content,false);
			collectionButtonTransform.anchoredPosition = CollectionButtonPosition;
			CollectionButtonPosition.x = CollectionButtonPosition.x + CollectionButtonOffset + CollectionButtonOffset / gap;
		}

		//scroll space width = (last position + gap) - gap + padding
		//The last position after for loop only store (last position + gap), so convert it back to last position by - gap
		content.sizeDelta = new Vector2 (CollectionButtonPosition.x - (CollectionButtonOffset + CollectionButtonOffset / gap) + (CollectionButtonOffset/2 + CollectionButtonOffset/padding), content.sizeDelta.y);



	}

	public void clickStart(){
		SceneManager.LoadScene (2);
	}

	public void clickCollection(GameObject localEdit, GameObject LocalDelete){
		//hide other edit and delete button
		GameObject[] otherEdit = null;
		GameObject[] otherDelete = null;
		otherEdit = GameObject.FindGameObjectsWithTag ("Edit");
		otherDelete = GameObject.FindGameObjectsWithTag ("Delete");
		foreach (GameObject buttonObject in otherEdit) {
			buttonObject.SetActive (false);
		}
		foreach (GameObject buttonObject in otherDelete) {
			buttonObject.SetActive (false);
		}

		//show edit and delete button
		localEdit.SetActive (true);
		LocalDelete.SetActive (true);
	}

	public void clickEdit(int index){
		print ("Edit " + index);
		PlayerPrefs.SetInt ("collectionIndex", index);
		PlayerPrefs.SetInt ("filenameID", permanentData.collection[index].fileID);
		SceneManager.LoadScene (3);
	}

	public void clickDelete(int index){
		string path = coloredPath +"/"+ permanentData.collection[index].fileID +".png";
		File.Delete (path);
		permanentData.collection.RemoveAt (index);

		GameObject[] collectionClone;
		collectionClone = GameObject.FindGameObjectsWithTag ("Collection");
		foreach (GameObject buttonObject in collectionClone) {
			Destroy (buttonObject);
		}

		refreshCollection ();


		print ("Delete " + index);
	}

	public void refreshCollection(){
		RectTransform collectionButtonTransform;
		float CollectionButtonOffset = permanentData.collectionPrefab.GetComponent<RectTransform> ().sizeDelta.x;
		Vector2 CollectionButtonPosition = new Vector2 (CollectionButtonOffset/2+CollectionButtonOffset/padding, 0);
		int CollectionLength = permanentData.collection.Count;
		for (int i = 0; i < CollectionLength; i++) {
			collectionButton = Instantiate (permanentData.collectionPrefab);

			//fix bug call by reference on clickedit() parameter
			int index = i;
			GameObject edit = collectionButton.transform.Find("Edit").gameObject;
			GameObject delete = collectionButton.transform.Find("Delete").gameObject;
			edit.SetActive (false);
			delete.SetActive (false);
			collectionButton.GetComponent<Button> ().onClick.AddListener (delegate() {
				clickCollection(edit, delete);
			});
			edit.GetComponent<Button> ().onClick.AddListener (delegate() {
				clickEdit(index);
			});
			delete.GetComponent<Button> ().onClick.AddListener (delegate() {
				clickDelete(index);
			});

			collectionButton.GetComponent<RawImage> ().texture = permanentData.collection [i].tex;
			collectionButtonTransform = collectionButton.GetComponent<RectTransform> ();
			collectionButtonTransform.SetParent (content,false);
			collectionButtonTransform.anchoredPosition = CollectionButtonPosition;
			CollectionButtonPosition.x = CollectionButtonPosition.x + CollectionButtonOffset + CollectionButtonOffset / gap;
		}

		//scroll space width = (last position + gap) - gap + padding
		//The last position after for loop only store (last position + gap), so convert it back to last position by - gap
		content.sizeDelta = new Vector2 (CollectionButtonPosition.x - (CollectionButtonOffset + CollectionButtonOffset / gap) + (CollectionButtonOffset/2 + CollectionButtonOffset/padding), content.sizeDelta.y);

	}
}
