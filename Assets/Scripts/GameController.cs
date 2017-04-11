﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	private BoardController boardController;
	private DialogueController dialogueController;
	private PlayerController playerInstance;
	private HeatController playerInstanceHeat;

	private float startTime;

	private int remaining_Tiles = 0;
	private int total_Tiles;

	private float timePerTile = 1f;
	private float gameEndTime;

	private float playerWarningDelay = 5f;
	private float nextPlayerWarning;

	private float enemySpawnDelay;
	private float enemySpawnTime;
	private int enemySpawnCount = 3;
	private int enemyCount = 0;

	private float enemyHealth = 25f;

	private int stormSpawnCount = 4;
	private float stormSpawnDelay;
	private float stormSpawnTime;
	private float stormFrequency = 1f;

	private Text progressInfo;
	private Text playerInfo;
	private Text tileInfo;
	private Text timeInfo;

	public GameObject selector;
	private Transform selectorTransform;
	private Transform clockTransform;

	private Image heatBarImage;
	private Image progressBarImage;

	private GameObject tutorialFrame;
	private GameObject instructionFrame;
	
	private bool showStormDialogue = true;
	private bool isGameOver = false;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(this.gameObject);
		}

		DontDestroyOnLoad(this.gameObject);
		boardController = GetComponent<BoardController>();
		dialogueController = GetComponent<DialogueController>();

		progressInfo = (GameObject.Find("ProgressBar_Text")).GetComponent<Text>();
		playerInfo = (GameObject.Find("HeatBar_Text")).GetComponent<Text>();
		tileInfo = (GameObject.Find("Tile Text")).GetComponent<Text>();
		timeInfo = (GameObject.Find("Clock_Text")).GetComponent<Text>();

		clockTransform = (GameObject.Find("Clock_Image")).GetComponent<Transform>();

		heatBarImage = (GameObject.Find ("HeatBar_Overlay")).GetComponent<Image>();
		progressBarImage = (GameObject.Find ("ProgressBar_Overlay")).GetComponent<Image>();

		tutorialFrame = GameObject.Find ("Tutorial_Holder");
		tutorialFrame.SetActive (false);
		instructionFrame = GameObject.Find ("Instructions_Holder");
		instructionFrame.SetActive (false);

		InitGame();
	}

	void OnEnable() {
		// Set listeners for events
		HeatController.ObjectFrozenEvent += incrementFrozenCount;
		HeatController.ObjectUnfrozenEvent += decrementFrozenCount;
		PlayerController.PlayerWarningEvent += TriggerPlayerWarning;
	}

	void Start() {
		startTime = Time.time;
		total_Tiles = boardController.getTileCount();
		gameEndTime = total_Tiles * timePerTile + startTime;

		selectorTransform = Instantiate(selector, Input.mousePosition, Quaternion.identity).transform;

		playerInfo.text = "";
		timeInfo.text = "";
		tileInfo.text = "";

		//Set Enemy Spawn Frequency
		enemySpawnDelay = gameEndTime / (enemySpawnCount + 1);
		enemySpawnTime = enemySpawnDelay + startTime;

		//Set Storm Spawn Frequency
		stormSpawnDelay = gameEndTime / (stormSpawnCount + 1);
		stormSpawnTime = stormSpawnDelay + startTime;

		nextPlayerWarning = 0;

		dialogueController.StartDialogue("intro");
		StartCoroutine(ShowTutorial());
	}

	void Update() {
		//Game Time Check
		if (Time.time > gameEndTime && !isGameOver) {
			isGameOver = true;
			StartCoroutine(GameOver(false));
		}

		//Update Player Heat & Time
		int remainingTime = (int)(gameEndTime - Time.time);
		progressInfo.text = "Remaining Tiles: " +  remaining_Tiles + "/" + total_Tiles;
		playerInfo.text = "Remaining Heat: " + (playerInstanceHeat.getCurrentHeat()).ToString("#") + " J";
		timeInfo.text = remainingTime / 60 + ":" + (remainingTime % 60).ToString("00");

		//Performance Checks
		float percentTilesUnfrozen = (float)(total_Tiles - remaining_Tiles) / (float)total_Tiles;
		float percentTimePassed = (Time.time - startTime) / (gameEndTime - startTime);
		float percentHeat = playerInstanceHeat.getPercentHeat();

		//UI Updates
		float rotationAngle = 360f - (percentTimePassed * 180f);
		clockTransform.eulerAngles = new Vector3(0, 0, rotationAngle);

		heatBarImage.fillAmount = percentHeat;
		progressBarImage.fillAmount = percentTilesUnfrozen;

		//Spawn Enemy
		if (Time.time > enemySpawnTime && enemyCount < enemySpawnCount) {
			if (percentTilesUnfrozen > percentTimePassed) {
				enemyHealth += 20;
			} else {
				enemyHealth = Mathf.Max(0, enemyHealth - 10);
			}

			boardController.SpawnEnemy(enemyHealth);
			enemySpawnTime += enemySpawnDelay;

			dialogueController.StartDialogue("enemy" + enemyCount);
			enemyCount++;
		}

		//Start Storm
		if (Time.time > stormSpawnTime) {
			
			if (showStormDialogue) {
				dialogueController.StartDialogue("storm");
				showStormDialogue = false;
				StartCoroutine(boardController.SnowStorm());
			}

			// Update storm intensity based on performance
			if (percentTilesUnfrozen > percentTimePassed) {
				stormFrequency = Mathf.Max(0.01f, stormFrequency - 0.01f);
			} else {
				stormFrequency += 0.01f;
			}

			boardController.setHazardSpawnRate(stormFrequency);
			stormSpawnTime += stormSpawnDelay;
		}

		//Mouse Over Information
		Vector2 mousePosition = new Vector2 (Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

		RaycastHit2D hit = Physics2D.Raycast (mousePosition, Vector2.zero, 0f);
		if (hit) {
			HeatController selectedObject = hit.transform.gameObject.GetComponent<HeatController>();

			if (selectedObject != null) {
				selectorTransform.position = hit.transform.position;

				if (hit.collider.CompareTag ("Tile")) {
					tileInfo.text = "Mass: " + selectedObject.mass + " Kg\n"
					+ "Heat Capacity: " + selectedObject.heatCapacity + " J/K\n"
						+ "Current Temperature: " + (selectedObject.getTemperature (selectedObject.getCurrentHeat())).ToString("0.0") + " ‎°C\n"
						+ "Unfreezing Temperature: " + (selectedObject.unfreezeTemp).ToString("0.0") + " ‎°C";
				} else if (hit.collider.CompareTag ("Source")) {
					tileInfo.text = "Source Remaining Heat: " + (selectedObject.getCurrentHeat()).ToString("#") + " J";
				} else if (hit.collider.CompareTag ("Enemy")) { 
					tileInfo.text = "Heat Needed to Destroy: " + ((selectedObject.getUnfreezeThreshold() - selectedObject.getCurrentHeat())).ToString("#") + " J";
				} else if (hit.collider.CompareTag ("Player")) {
					tileInfo.text = "You notice your fabulous looking character sprite";
				}
			}/* else {
				tileInfo.text = "If you notice this notice,\nyou will notice this notice\nis not worth notice.";
			}*/
		}
	}

	void InitGame() {
		boardController.SetupGameArea();
		playerInstance = boardController.getPlayer();
		playerInstanceHeat = playerInstance.GetComponent<HeatController>();
	}

	private void incrementFrozenCount(GameObject gameObject) {
		if (gameObject.CompareTag("Tile")) {
			remaining_Tiles += 1;
		} else if (gameObject.CompareTag ("Player")) {
			StartCoroutine(GameOver(false));
		}
	}

	private void decrementFrozenCount(GameObject gameObject) {
		if (gameObject.CompareTag ("Tile")) {
			remaining_Tiles -= 1;
		} 

		if (remaining_Tiles == 0) {
			StartCoroutine(GameOver(true));
		}
	}

	private void TriggerPlayerWarning() {
		if (Time.time > nextPlayerWarning) {
			nextPlayerWarning = Time.time + playerWarningDelay;
			dialogueController.StartDialogue ("heatWarning");
		}
	}

	private IEnumerator ShowTutorial() {
		yield return new WaitForSeconds (1f);
		tutorialFrame.SetActive (true);
		yield return new WaitForSeconds (3f);
		tutorialFrame.SetActive (false);
		instructionFrame.SetActive (true);
		yield return new WaitForSeconds (5f);
		instructionFrame.SetActive (false);
	}

	private IEnumerator GameOver(bool win) {
		StopCoroutine(boardController.SnowStorm());
		if (win) {
			yield return new WaitForSeconds (1f);
			dialogueController.StartDialogue("win");
			yield return new WaitForSeconds (1f);
			SceneManager.LoadScene("Win");
		} else {
			yield return new WaitForSeconds (1f);
			dialogueController.StartDialogue("gameOver");
			yield return new WaitForSeconds (1f);
			SceneManager.LoadScene("Lose");
		}

		Destroy (this.gameObject);
	}
}
