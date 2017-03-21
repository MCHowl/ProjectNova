using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	private BoardController boardController;

	private int remaining_Tiles = 0;
	private int remaining_Entities = 0;

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

	void OnEnable() {
		HeatController.ObjectFrozenEvent += incrementFrozenCount;
		HeatController.ObjectUnfrozenEvent += decrementFrozenCount;
	}

	void InitGame() {
		boardController.SetupGameArea();
	}

	private void incrementFrozenCount(GameObject gameObject) {
		if (gameObject.CompareTag("Entity")) {
			remaining_Entities += 1;
		} else if (gameObject.CompareTag("Tile")) {
			remaining_Tiles += 1;
		} else if (gameObject.CompareTag ("Player")) {
			if (!boardController.RespawnPlayer()) {
				Debug.Log ("Game Over");
			}
		}

		//Debug.Log ("Entities Remaining: " + remaining_Entities + "\nTiles Remaining: " + remaining_Tiles);
	}

	private void decrementFrozenCount(GameObject gameObject) {
		if (gameObject.CompareTag ("Entity")) {
			remaining_Entities -= 1;
		} else if (gameObject.CompareTag ("Tile")) {
			remaining_Tiles -= 1;
		} 

		//Debug.Log ("Entities Remaining: " + remaining_Entities + "\nTiles Remaining: " + remaining_Tiles);

		if (remaining_Tiles == 0 && remaining_Entities == 0) {
			Debug.Log ("Game Over. You Win");
		}
	}
}
