using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

	public GameController gameController;

	void Awake() {
		if (GameController.instance == null) {
			Instantiate(gameController);
		}
	}
}
