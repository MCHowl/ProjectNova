using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {

	public GameObject snow;
	public GameObject player;
	public GameObject enemy;
	public GameObject heatSource;
	public GameObject[] tiles_Wall;
	public GameObject[] tiles_Floor;

	private PlayerController playerController;

	private int tile_Count;
	private int board_Width = 20;
	private int board_Height = 20;
	private int source_Count = 5;

	private int sourceSpawnRange = 5;
	private int enemySpawnRange = 5;
	private float hazardSpawnRate = 1;

	private bool[,] spawnableArea;
	private GameObject[,] gameBoard;

	private Transform holder_GameBoard;
	private Transform holder_GameBoarder;
	private Transform holder_Entities_Source;
	private Transform holder_Entities_Hazards;

	private int[] pattern1 = {1, 0, 1, 1, 3, 2, 1,
							  2, 0, 2, 2, 0, 2, 2,
							  2, 0, 3, 0, 0, 3, 1,
							  3, 0, 0,-1, 0, 3, 0,
							  2, 3, 3, 0, 3, 2, 1,
							  3, 0, 2, 1, 3, 3, 1,
							  1, 1, 3, 2, 2, 1, 1};

	private int[] pattern2 = {3, 1, 1, 2, 1, 3, 2,
							  2, 0, 3, 2, 3, 1, 3,
							  1, 3, 0, 0, 1, 0, 2,
							  1, 3, 0,-1, 0, 0, 3,
							  2, 2, 0, 0, 1, 0, 0,
							  1, 2, 3, 3, 0, 3, 2,
							  2, 1, 1, 2, 3, 1, 2};

	private void InstantiateHolders() {
		holder_GameBoard = new GameObject ("Board").transform;
		holder_GameBoarder = new GameObject ("Walls").transform;
		holder_Entities_Source = new GameObject ("Source Entities").transform;
		holder_Entities_Hazards = new GameObject ("Hazard Entities").transform;
	}

	private void SetupBoard() {
		// Instantiate gameTile holder
		spawnableArea = new bool[board_Width, board_Height];
		gameBoard = new GameObject[board_Width, board_Height];

		// Instantiate Floor Tiles
		for (int i = 0; i < board_Width; i++) {
			for (int j = 0; j < board_Height; j++) {

				Vector3 newTilePostition = new Vector3 (i, j, 0.0f);
				gameBoard[i,j] = InstantiateObject(tiles_Floor[Random.Range (0, tiles_Floor.Length)],
													newTilePostition, holder_GameBoard);
				spawnableArea[i,j] = true;
			}
		}

		// Instantiate Wall Tiles
		for (int j = -1; j < board_Height + 1; j++) {

			if (j == -1 || j == board_Height) {
				// Build full row for top and bottom of map
				for (int i = -1; i < board_Width + 1; i++) {
					InstantiateObject(tiles_Wall[Random.Range (0, tiles_Wall.Length)],
										new Vector3 (i, j, 0.0f), holder_GameBoarder);

				}
			} else {
				// Otherwise spawn walls at x = -1 and x = board_Width
				InstantiateObject(tiles_Wall[Random.Range (0, tiles_Wall.Length)],
									new Vector3 (-1, j, 0.0f), holder_GameBoard);
				InstantiateObject(tiles_Wall[Random.Range (0, tiles_Wall.Length)],
									new Vector3 (board_Width, j, 0.0f), holder_GameBoarder);
			}
		}
	}

	/**
	 * Spawn 5 source tiles spread evenly across the map
	 **/
	private void SpawnSources() {
		//Spawn middle source
		int x_pos = (int) Random.Range(board_Width/2 - sourceSpawnRange, board_Width/2 + sourceSpawnRange);
		int y_pos = (int) Random.Range(board_Height/2 - sourceSpawnRange, board_Height/2 + sourceSpawnRange);
		Vector3 spawnPosition = new Vector3(x_pos, y_pos, 0);
		InstantiateObject(heatSource, spawnPosition, holder_Entities_Source);

		spawnableArea[(int) spawnPosition.x, (int) spawnPosition.y] = false;
		Destroy(gameBoard [(int)spawnPosition.x, (int)spawnPosition.y]);
		//CreateTilePattern((int) spawnPosition.x, (int) spawnPosition.y);

		//Spawn 4 corner sources
		int x_offset = board_Width / 4;
		int y_offset = board_Height / 4;

		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 2; j++) {
				x_pos = (int) Random.Range(x_offset + (i * board_Width/2) - sourceSpawnRange, x_offset + (i * board_Width/2) + sourceSpawnRange);
				y_pos = (int) Random.Range(y_offset + (j * board_Height/2) - sourceSpawnRange, y_offset + (j * board_Height/2) + sourceSpawnRange);
				spawnPosition = new Vector3(x_pos, y_pos, 0);
				InstantiateObject(heatSource, spawnPosition, holder_Entities_Source);

				spawnableArea[(int) spawnPosition.x, (int) spawnPosition.y] = false;
				Destroy(gameBoard [(int)spawnPosition.x, (int)spawnPosition.y]);
				//CreateTilePattern((int) spawnPosition.x, (int) spawnPosition.y);
			}
		}
	}

	private void CreateTilePattern(int x, int y) {
		int[] pattern = pattern1;
		if (Random.Range (0, 1) > 0.5) {
			pattern = pattern2;
		}

		int start_x = x - 3;
		int start_y = y - 3;

		for (int i = 0; i < 7; i++) {
			for (int j = 0; j < 7; j++) {
				int position = pattern [(i * 7) + j];

				if (position >= 0) {
					Destroy (gameBoard [start_x + i, start_y + j]);

					Vector3 newTilePostition = new Vector3 (start_x + i, start_y + j, 0.0f);
					gameBoard [i, j] = InstantiateObject (tiles_Floor[position], newTilePostition, holder_GameBoard);
				}
			}
		}
	}

	private void SpawnPlayer() {
		Transform startSource = holder_Entities_Source.GetChild(Random.Range (0, holder_Entities_Source.childCount));

		float playerSpawn_x;

		if (startSource.position.x <= 1) {
			playerSpawn_x = startSource.position.x + 1;
		} else {
			playerSpawn_x = startSource.position.x + 1;
		}

		Vector3 playerSpawn = new Vector3 (playerSpawn_x, startSource.position.y, 0f);
		GameObject playerInstance = Instantiate (player, playerSpawn, Quaternion.identity);
		playerController = playerInstance.GetComponent<PlayerController>();
	}

	public void SpawnEnemy(float health) {
		float player_x = playerController.transform.position.x;
		float player_y = playerController.transform.position.y;

		int spawn_x, spawn_y;

		while (true) {
			spawn_x = (int) Random.Range (Mathf.Max(1, player_x - enemySpawnRange), Mathf.Min(player_x + enemySpawnRange, board_Width - 1));
			spawn_y = (int) Random.Range (Mathf.Max(1, player_y - enemySpawnRange), Mathf.Min(player_y + enemySpawnRange, board_Height - 1));

			if (spawnableArea[spawn_x, spawn_y]) {
				break;
			}
		}
		Vector3 spawnPosition = new Vector3 (spawn_x, spawn_y, 0);
		GameObject newEnemy = Instantiate(enemy, spawnPosition, Quaternion.identity);

		HeatController enemyHeatController = newEnemy.GetComponent<HeatController>();
		enemyHeatController.setEnemyHealth(health);
	}

	/**
	 * Helper Functions
	 **/
	private GameObject InstantiateObject(GameObject newObject, Vector3 position, Transform parent) {
		GameObject newInstance = Instantiate (newObject, position, Quaternion.identity);
		newInstance.transform.SetParent(parent);

		return newInstance;
	}

	public void SetupGameArea() {
		tile_Count = board_Width * board_Height - source_Count;

		InstantiateHolders();
		SetupBoard();
		SpawnSources();
		SpawnPlayer();
	}

	public IEnumerator SnowStorm() {
		while(true) {
			Vector3 spawnPosition = new Vector3 (Random.Range(0, board_Width),
				Random.Range(5, board_Height), 0);
			//Instantiate (snow, , Quaternion.identity);
			InstantiateObject (snow, spawnPosition, holder_Entities_Hazards);
			yield return new WaitForSeconds(hazardSpawnRate);
		}
	}

	/**
	 * Getters & Setters
	 **/
	public PlayerController getPlayer(){
		return playerController;
	}

	public int getTileCount() {
		return tile_Count;
	}

	public void setHazardSpawnRate(float newRate) {
		hazardSpawnRate = newRate;
	}
}
