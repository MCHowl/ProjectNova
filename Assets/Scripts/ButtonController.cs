using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour {

	public void LoadMain() {
		SceneManager.LoadScene("Main");
	}

	public void QuitApplication() {
		Application.Quit();
	}
}
