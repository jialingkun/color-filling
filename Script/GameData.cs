using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameData : MonoBehaviour {
	public Texture2D[] stageImages;
	public List<CollectionScript> collection;
	public GameObject collectionPrefab;
	private string coloredPath;


	// Use this for initialization
	void Start () {
		SaveLoad.loadCounter ();

		collection = new List<CollectionScript>();

		coloredPath = Application.persistentDataPath + "/ColoredPictures";
		string path;
		byte[] fileData;
		Texture2D tex;
		CollectionScript tempCollection;
		for (int i = 1; i <= SaveLoad.SaveCounter; i++) {
			path = coloredPath + "/"+ i +".png";
			if (File.Exists (path)) {
				fileData = File.ReadAllBytes(path);
				tex = new Texture2D(2, 2);
				tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
				tempCollection = new CollectionScript(tex,i);
				collection.Add(tempCollection);
			}
		}
	}

	void Awake() {
		GameObject.DontDestroyOnLoad (gameObject); 
	}
}
