using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour {
	public Texture2D[] stageImages;
	private Texture2D[] collection;


	// Use this for initialization
	void Start () {
		SaveLoad.loadCounter ();

		collection = new Texture2D[SaveLoad.SaveCounter];
		for (int i = 1; i <= SaveLoad.SaveCounter; i++) {
			
		}
	}

	void Awake() {
		GameObject.DontDestroyOnLoad (gameObject); 
	}
}
