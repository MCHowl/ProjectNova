using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	private BoardController boardController;
	private PlayerController playerInstance;

	private int remaining_Tiles = 0;

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
		playerInstance = boardController.getPlayer();
	}

	private void incrementFrozenCount(GameObject gameObject) {
		if (gameObject.CompareTag("Tile")) {
			remaining_Tiles += 1;
		} else if (gameObject.CompareTag ("Player")) {
			Debug.Log ("You Lose");
			GameOver();
		}

		//Debug.Log ("Entities Remaining: " + remaining_Entities + "\nTiles Remaining: " + remaining_Tiles);
	}

	private void decrementFrozenCount(GameObject gameObject) {
		if (gameObject.CompareTag ("Tile")) {
			remaining_Tiles -= 1;
		} 

		//Debug.Log ("Entities Remaining: " + remaining_Entities + "\nTiles Remaining: " + remaining_Tiles);

		if (remaining_Tiles == 0) {
			Debug.Log ("You Win");
			GameOver();
		}
	}

	private void GameOver(){
		Debug.Log("Game Over");
	}
}
