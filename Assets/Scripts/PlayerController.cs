using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public LayerMask blockingLayer;

	private Rigidbody2D rb2d;
	private BoxCollider2D boxCollider;
	private HeatController heatController;

	private GameObject inCollisionWith = null;

	private Vector2 collisionVector;
	private float moveDistance = 1.0f;

	private float moveTime = 0.1f;
	private float inverseMoveTime;

	private bool isMove = true;

	void Start() {
		rb2d = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		heatController = GetComponent<HeatController>();

		inverseMoveTime = 1f / moveTime;
		collisionVector = boxCollider.size;
	}

	void Update() {
		//Debug.Log ("Current Player Heat: " + heatController.getCurrentHeat ());

		if (inCollisionWith != null) {
			HeatController destinationHeatController = inCollisionWith.GetComponent<HeatController>();

			if (destinationHeatController != null) {
				if (destinationHeatController.getIsFrozen()) {
					if (Input.GetKeyDown(KeyCode.R)) {
						//Unfreeze Object
						heatController.Unfreeze(destinationHeatController, heatController);
					}
				}

				if (Input.GetKeyDown(KeyCode.B)) {
					//Burn Object
					heatController.Burn(destinationHeatController, heatController);
					ResetCollision();
				}
			} else {
				Debug.LogWarning("Unable to find 'HeatController' script on " + inCollisionWith.name);
			}
		}
	}
		
	void FixedUpdate () {
		int moveHorizontal = (int) Input.GetAxis("Horizontal");
		int moveVertical = (int) Input.GetAxis("Vertical");

		if (moveHorizontal != 0 || moveVertical != 0) {
			AttemptMove(moveHorizontal, moveVertical);
		}
	}

	void EnterCollision(Collider2D other) {
		if (other.gameObject.CompareTag("Wall")) {
			return;
		} else if (other.gameObject.CompareTag("Entity")) {
			inCollisionWith = other.gameObject;
			Debug.Log ("In collision with " + other.gameObject.name + "\nPress 'R' to unfreeze, Press 'B' to burn");
		} else if (other.gameObject.CompareTag("Source")) {
			heatController.setHeatToMaximum();
			Debug.Log ("Heat restored");
		}
	}

	private void ResetCollision() {
		inCollisionWith = null;
	}

	public void RespawnAtLocation(float x, float y) {
		StartCoroutine (spawnCoroutine (x, y));
	}

	public void AttemptMove(float moveHorizontal, float moveVertical) {
		if (!heatController.getIsFrozen () && isMove) {
			if (moveHorizontal != 0) {
				moveVertical = 0;
			}

			Vector2 movement = new Vector2 (moveHorizontal * moveDistance, moveVertical * moveDistance);
			Vector2 collisionCheck = new Vector2 ((moveDistance + collisionVector.x / 2) * moveHorizontal,
													(moveDistance + collisionVector.y / 2) * moveVertical);

			Vector2 start = new Vector2 (Mathf.Round(transform.position.x * 10) / 10, Mathf.Round(transform.position.y * 10) / 10);
			Vector2 end = start + movement;
			Vector2 collisionEnd = start + collisionCheck;

			//Debug.Log (start.x + ", " + start.y + "\n" + collisionEnd.x + ", " + collisionEnd.y);

			boxCollider.enabled = false;
			RaycastHit2D hit = Physics2D.Linecast (start, collisionEnd, blockingLayer);
			boxCollider.enabled = true;

			if (hit.transform == null) {
				isMove = false;
				ResetCollision();
				StartCoroutine(Move (end));
			} else {
				EnterCollision(hit.collider);
			}
		}
	}

	private IEnumerator Move(Vector3 end) {
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		while (sqrRemainingDistance > float.Epsilon) {
			Vector3 newPosition = Vector3.MoveTowards (rb2d.position, end, inverseMoveTime * Time.deltaTime);
			rb2d.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}

		isMove = true;
	}

	private IEnumerator spawnCoroutine(float x, float y) {
		yield return new WaitForSeconds(1);
		ResetCollision ();
		heatController.setHeatToMaximum();
		rb2d.position = new Vector2(x, y);
		yield return null;
	}
}
