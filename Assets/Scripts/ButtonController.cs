using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour {

	public void LoadMain() {
		Application.LoadLevel("Main");
	}

	public void QuitApplication() {
		Application.Quit();
	}
}
