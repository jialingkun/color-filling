using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Panel_Effect : MonoBehaviour {
	public float splash_DelayTime;
	private Image panelImage;
	// Use this for initialization
	void Start () {
		panelImage = this.GetComponent<Image> ();
		StartCoroutine(FadeSplash());
	}


	IEnumerator FadeSplash(){
		panelImage.CrossFadeAlpha(0.0f, 1.0f, false); //(alpha value, fade speed, not important)
		yield return new WaitForSeconds(splash_DelayTime);
		panelImage.CrossFadeAlpha(1.0f, 1.0f, false);
		yield return new WaitForSeconds(1.0f);
		SceneManager.LoadScene (1);
	}
}
