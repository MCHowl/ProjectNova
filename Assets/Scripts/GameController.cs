using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	private BoardController boardController;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(this.gameObject);
		}

		DontDestroyOnLoad(this.gameObject);
		boardController = GetComponent<BoardController>();
		InitGame();
	}

	void InitGame() {
		boardController.SetupGameArea();
	}
}
