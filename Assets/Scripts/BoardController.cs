using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {

	public GameObject snow;
	public GameObject player;
	public GameObject enemy;
	public GameObject[] entities_Source;
	public GameObject[] tiles_Wall;
	public GameObject[] tiles_Floor;

	private PlayerController playerController;

	private int tile_Count;
	private int board_Width = 75;
	private int board_Height = 40;

	private int enemySpawnRange = 10;

	private bool[,] spawnableArea;
	private GameObject[,] gameBoard;

	private Transform holder_GameBoard;
	private Transform holder_GameBoarder;
	private Transform holder_Entities_Source;

	private void InstantiateHolders() {
		holder_GameBoard = new GameObject ("Board").transform;
		holder_GameBoarder = new GameObject ("Walls").transform;
		holder_Entities_Source = new GameObject ("Source Entities").transform;
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

	private void SpawnEntities(int sourceCount) {
		for (int i = 0; i < sourceCount; i++) {
			Vector3 spawnPosition = getRandomVaildBoardPosition();
			InstantiateObject(entities_Source [Random.Range(0, entities_Source.Length)], spawnPosition, holder_Entities_Source);
			Destroy(gameBoard [(int)spawnPosition.x, (int)spawnPosition.y]);
		}
	}

	private void SpawnPlayer() {
		//Choose a random point for the player to spawn
		Vector3 playerSpawn = getRandomVaildBoardPosition();
		GameObject playerInstance = Instantiate (player, playerSpawn, Quaternion.identity);
		playerController = playerInstance.GetComponent<PlayerController>();

		//Spawn a source next to the player
		float spawn_x;
		if (playerSpawn.x <= 0) {
			spawn_x = playerSpawn.x + 1;
		} else {
			spawn_x = playerSpawn.x - 1;
		}

		Vector3 sourceSpawn = new Vector3(spawn_x, playerSpawn.y, playerSpawn.z);
		InstantiateObject(entities_Source [Random.Range(0, entities_Source.Length)], sourceSpawn, holder_Entities_Source);

		//Remove Tile from spawnableArea
		spawnableArea[(int)spawn_x, (int)playerSpawn.y] = false;
		Destroy(gameBoard [(int)spawn_x, (int)playerSpawn.y]);

	}

	public void SpawnEnemy(float health, float moveDelay) {
		float player_x = playerController.transform.position.x;
		float player_y = playerController.transform.position.y;

		int spawn_x = (int) Random.Range (Mathf.Max(1, player_x - enemySpawnRange), Mathf.Min(player_x + enemySpawnRange, board_Width - 1));
		int spawn_y = (int) Random.Range (Mathf.Max(1, player_y - enemySpawnRange), Mathf.Min(player_y + enemySpawnRange, board_Height - 1));

		Vector3 spawnPosition = new Vector3 (spawn_x, spawn_y, 0);
		GameObject newEnemy = Instantiate(enemy, spawnPosition, Quaternion.identity);

		HeatController enemyHeatController = newEnemy.GetComponent<HeatController>();
		EnemyController enemyController = newEnemy.GetComponent<EnemyController>();

		enemyHeatController.setEnemyHealth(health);
		enemyController.setMoveDelay(moveDelay);
	}

	/**
	 * Helper Functions
	 **/
	private GameObject InstantiateObject(GameObject newObject, Vector3 position, Transform parent) {
		GameObject newInstance = Instantiate (newObject, position, Quaternion.identity);
		newInstance.transform.SetParent(parent);

		return newInstance;
	}

	private Vector3 getRandomVaildBoardPosition() {
		bool isSearching = true;
		int count = 0;
		int x, y;

		while (isSearching) {
			x = Random.Range (0, board_Width);
			y = Random.Range (0, board_Height);

			if (spawnableArea[x, y]) {
				spawnableArea[x, y] = false;
				return new Vector3 (x, y, 0);
			} else {
				count++;

				if (count >= Mathf.Sqrt (board_Width * board_Height)) {
					Debug.LogError ("Unable to find valid spawn position");
					return new Vector3 (-1, -1, -1);
				}
			}
		}
		Debug.LogError ("You are not supposed to reach this line of code");
		return new Vector3 (-1, -1, -1);
	}

	public void SetupGameArea(int sourceCount) {
		tile_Count = board_Width * board_Height - sourceCount - 1;

		InstantiateHolders();
		SetupBoard();
		SpawnPlayer();
		SpawnEntities(sourceCount);
	}

	public IEnumerator SnowStorm(int count, float frequency) {
		for (int i=0; i < count; i++) {
			Vector3 spawnPosition = new Vector3 (Random.Range(0, board_Width),
				Random.Range(5, board_Height), 0);
			Instantiate (snow, spawnPosition, Quaternion.identity);
			yield return new WaitForSeconds(frequency);
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
}
