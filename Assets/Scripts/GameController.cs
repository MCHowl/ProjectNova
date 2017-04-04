using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	private BoardController boardController;
	private PlayerController playerInstance;
	private HeatController playerInstanceHeat;

	private int remaining_Tiles = 0;
	private int total_Tiles;

	private float timePerTile = 2f;
	private float gameEndTime;
	private int sourceBlockSpawnCount = 68; //Note: Total blocks spawned will be this number + 1

	private float enemySpawnDelay;
	private float enemySpawnTime;
	private int enemySpawnCount = 3;

	private float enemySpeed = 0.5f;
	private float enemyHealth = 10000000f;

	private float stormSpawnDelay;
	private float stormSpawnTime;
	private int stormSpawnCount = 4;

	private int stormLength = 100;
	private float stormIntensity = 0.25f;

	public GameObject selector;
	private Transform selectorPosition;

	private Text playerInfo;
	private Text tileInfo;
	private Text timeInfo;


	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(this.gameObject);
		}

		DontDestroyOnLoad(this.gameObject);
		boardController = GetComponent<BoardController>();

		playerInfo = (GameObject.Find("Player Text")).GetComponent<Text>();
		tileInfo = (GameObject.Find("Tile Text")).GetComponent<Text>();
		timeInfo = (GameObject.Find("Time Text")).GetComponent<Text>();
		InitGame(sourceBlockSpawnCount);
	}

	void OnEnable() {
		// Set listeners for freeze/unfreeze events
		HeatController.ObjectFrozenEvent += incrementFrozenCount;
		HeatController.ObjectUnfrozenEvent += decrementFrozenCount;
	}

	void Start() {
		total_Tiles = boardController.getTileCount();
		gameEndTime = total_Tiles * timePerTile;

		selectorPosition = Instantiate(selector, Input.mousePosition, Quaternion.identity).transform;

		playerInfo.text = "";
		timeInfo.text = "";
		tileInfo.text = "";

		//Set Enemy Spawn Frequency
		enemySpawnDelay = gameEndTime / enemySpawnCount;
		enemySpawnTime = enemySpawnDelay;

		//Set Storm Spawn Frequency
		stormSpawnDelay = gameEndTime / stormSpawnCount;
		stormSpawnTime = stormSpawnDelay;
	}

	void Update() {
		//Game Time Check
		if (Time.time > gameEndTime) {
			playerInstance.FreezePlayer();
		}

		//Update Player Heat & Time
		int remainingTime = (int)(gameEndTime - Time.time);
		playerInfo.text = "Remaining Heat: " + playerInstanceHeat.getCurrentHeat();
		timeInfo.text = "Remaining Time: " + remainingTime + "s\n"
						+ "Remaining Tiles: " +  remaining_Tiles + "/" + total_Tiles;

		//Performance Checks
		float percentTilesPassed = ((float)(total_Tiles - remaining_Tiles) / (float)total_Tiles) * 100;
		float percentTimePassed = (Time.time / gameEndTime) * 100;

		//Spawn Enemy
		if (Time.time > enemySpawnTime) {
			boardController.SpawnEnemy(enemySpeed, enemyHealth);
			enemySpawnTime += enemySpawnDelay;
		}

		//Start Storm
		if (Time.time > stormSpawnTime) {
			StartCoroutine(boardController.SnowStorm(stormLength, stormIntensity));
		}

		//Mouse Over Information
		Vector2 mousePosition = new Vector2 (Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

		RaycastHit2D hit = Physics2D.Raycast (mousePosition, Vector2.zero, 0f);
		if (hit) {
			HeatController selectedObject = hit.transform.gameObject.GetComponent<HeatController>();

			if (selectedObject != null) {
				selectorPosition.position = hit.transform.position;

				if (hit.collider.CompareTag ("Tile")) {
					tileInfo.text = "Mass: " + selectedObject.mass + "kg\n"
					+ "Heat Capacity: " + selectedObject.heatCapacity + "J/K\n"
					+ "Current Temperature: " + selectedObject.getTemperature (selectedObject.getCurrentHeat()) + "C\n"
					+ "Unfreezing Temperature: " + selectedObject.getTemperature (selectedObject.heatThreshold_unfreeze) + "C";
				} else if (hit.collider.CompareTag ("Source")) {
					tileInfo.text = "Source Remaining Heat: " + selectedObject.getCurrentHeat();
				} else if (hit.collider.CompareTag ("Enemy")) { 
					tileInfo.text = "Heat Needed to Destroy: " + (selectedObject.heatThreshold_unfreeze - selectedObject.getCurrentHeat());
				} else if (hit.collider.CompareTag ("Player")) {
					tileInfo.text = "You notice your fabulous looking character sprite";
				}
			}/* else {
				tileInfo.text = "If you notice this notice,\nyou will notice this notice\nis not worth notice.";
			}*/
		}
	}

	void InitGame(int sourceCount) {
		boardController.SetupGameArea(sourceCount);
		playerInstance = boardController.getPlayer();
		playerInstanceHeat = playerInstance.GetComponent<HeatController>();
	}

	private void incrementFrozenCount(GameObject gameObject) {
		if (gameObject.CompareTag("Tile")) {
			remaining_Tiles += 1;
		} else if (gameObject.CompareTag ("Player")) {
			Debug.Log ("You Lose");
			GameOver();
		}
	}

	private void decrementFrozenCount(GameObject gameObject) {
		if (gameObject.CompareTag ("Tile")) {
			remaining_Tiles -= 1;
		} 

		if (remaining_Tiles == 0) {
			Debug.Log ("You Win");
			GameOver();
		}
	}

	private void GameOver(){
		Debug.Log("Game Over");
	}
}
