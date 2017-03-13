using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {

	public GameObject player;
	public GameObject[] entities_Source;
	public GameObject[] entities_Static;
	public GameObject[] entities_Mobile;
	public GameObject[] tiles_Wall;
	public GameObject[] tiles_Floor;

	private int board_Width = 20;
	private int board_Height = 20;

	private bool[,] spawnableArea;

	private PlayerController playerController;

	private Transform holder_GameBoard;
	private Transform holder_Entities_Mobile;
	private Transform holder_Entities_Static;
	private Transform holder_Entities_Source;

	private void InstantiateHolders() {
		holder_GameBoard = new GameObject ("Board").transform;
		holder_Entities_Mobile = new GameObject ("Mobile Entities").transform;
		holder_Entities_Static = new GameObject ("Static Entities").transform;
		holder_Entities_Source = new GameObject ("Source Entities").transform;
	}

	private void SetupBoard() {
		// Instantiate gameTile holder
		spawnableArea = new bool[board_Width, board_Height];

		// Instantiate Floor Tiles
		for (int i = 0; i < board_Width; i++) {
			for (int j = 0; j < board_Height; j++) {

				Vector3 newTilePostition = new Vector3 (i, j, 0.0f);
				InstantiateObject(tiles_Floor[Random.Range (0, tiles_Floor.Length)],
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
										new Vector3 (i, j, 0.0f), holder_GameBoard);

				}
			} else {
				// Otherwise spawn walls at x = -1 and x = board_Width
				InstantiateObject(tiles_Wall[Random.Range (0, tiles_Wall.Length)],
									new Vector3 (-1, j, 0.0f), holder_GameBoard);
				InstantiateObject(tiles_Wall[Random.Range (0, tiles_Wall.Length)],
									new Vector3 (board_Width, j, 0.0f), holder_GameBoard);
			}
		}
	}

	private void SpawnEntities(int staticCount, int mobileCount, int sourceCount) {
		for (int i = 0; i < sourceCount; i++) {
			InstantiateObject(entities_Source [Random.Range(0, entities_Source.Length)], getRandomVaildBoardPosition(), holder_Entities_Source);
		}

		for (int i = 0; i < staticCount; i++) {
			InstantiateObject(entities_Static [Random.Range(0, entities_Static.Length)], getRandomVaildBoardPosition(), holder_Entities_Static);
		}

		for (int i = 0; i < mobileCount; i++) {
			InstantiateObject(entities_Mobile [Random.Range(0, entities_Mobile.Length)], getRandomVaildBoardPosition(), holder_Entities_Mobile);
		}
	}

	private void SpawnPlayer() {
		//Choose a random point for the player to spawn
		Vector3 playerSpawn = getRandomVaildBoardPosition();
		GameObject newPlayer = Instantiate (player, playerSpawn, Quaternion.identity);
		playerController = newPlayer.GetComponent<PlayerController>();

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

	}

	public bool RespawnPlayer() {
		int remainingSourceCount = holder_Entities_Source.childCount;
		if (remainingSourceCount > 0) {
			Transform respawnSourceTransform = holder_Entities_Source.GetChild (Random.Range (0, remainingSourceCount));
			playerController.RespawnAtLocation(respawnSourceTransform.position.x, respawnSourceTransform.position.y);
			Destroy(respawnSourceTransform.gameObject);
			return true;
		} else {
			Destroy (playerController);
			return false;
		}
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

	public void SetupGameArea() {
		InstantiateHolders();
		SetupBoard();
		SpawnPlayer();
		SpawnEntities (10, 5, 2);
	}
}
