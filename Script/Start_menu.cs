using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Start_menu : MonoBehaviour {
	

	public void clickStart(){
		SceneManager.LoadScene (2);
	}
}
