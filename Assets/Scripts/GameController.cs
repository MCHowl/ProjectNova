using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	private BoardController boardController;
	private PlayerController playerInstance;

	private int remaining_Tiles = 0;
	private int total_Tiles;

	private float timePerTile = 2.0f;
	private float gameEndTime;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(this.gameObject);
		}

		DontDestroyOnLoad(this.gameObject);
		boardController = GetComponent<BoardController>();
		InitGame(6);
	}

	void OnEnable() {
		HeatController.ObjectFrozenEvent += incrementFrozenCount;
		HeatController.ObjectUnfrozenEvent += decrementFrozenCount;
	}

	void Start() {
		total_Tiles = boardController.getTileCount();
		gameEndTime = total_Tiles * timePerTile;
	}

	void Update() {
		//Game Time Check
		if (Time.time > gameEndTime) {
			playerInstance.FreezePlayer();
		}

		//Mouse Over Information
		if (Input.GetButtonDown("Fire1")) {
			Vector2 mousePosition = new Vector2 (Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

			RaycastHit2D hit = Physics2D.Raycast (mousePosition, Vector2.zero, 0f);
			if (hit) {
				HeatController selectedObject = hit.transform.gameObject.GetComponent<HeatController>();

				if (selectedObject != null) {

					if (hit.collider.CompareTag("Tile")) {
						Debug.Log("Mass: " + selectedObject.mass + "kg "
							+ "Heat Capacity: " + selectedObject.heatCapacity + "J/K \n"
							+ "Current Temperature: " + selectedObject.getTemperature(selectedObject.getCurrentHeat()) + "C "
							+ "Unfreezing Temperature: " + selectedObject.getTemperature(selectedObject.heatThreshold_unfreeze) + "C");
					} else if (hit.collider.CompareTag("Source")) {
						Debug.Log("Source Remaining Heat: " + selectedObject.getCurrentHeat());
					} else if (hit.collider.CompareTag("Player")) {
						Debug.Log("Player's Current Heat: " + selectedObject.getCurrentHeat());
					}
				} else {
					Debug.Log ("Invalid Object Selected");
				}
			}
		}
	}

	void InitGame(int sourceCount) {
		boardController.SetupGameArea(sourceCount);
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
