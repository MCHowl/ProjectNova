using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public delegate void PlayerWarning();
	public static event PlayerWarning PlayerWarningEvent;

	public LayerMask blockingLayer;

	private Animator animator;
	private Rigidbody2D rb2d;
	private BoxCollider2D boxCollider;
	private HeatController heatController;

	private GameObject inCollisionWith = null;

	private Vector2 collisionVector;
	private float moveDistance = 1.0f;

	private float moveTime = 0.25f;
	private float moveSensitivity = 0.9f;
	private float inverseMoveTime;

	private bool isMove = true;

	void Start() {
		animator = GetComponent<Animator>();
		rb2d = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		heatController = GetComponent<HeatController>();

		inverseMoveTime = 1f / moveTime;
		collisionVector = boxCollider.size;
	}

	void Update() {
		if (heatController.getPercentHeat() <= 0.2) {
			PlayerWarningEvent ();
		}


		if (inCollisionWith != null) {
			HeatController targetHeatController = inCollisionWith.GetComponent<HeatController>();

			if (targetHeatController != null) {
				if (inCollisionWith.CompareTag("Source")) {
					if (Input.GetKeyDown(KeyCode.Space)) {
						heatController.Unfreeze(heatController, targetHeatController);
					}
				}
			} else {
				Debug.LogError("Unable to find 'HeatController' script on " + inCollisionWith.name);
			}
		}
	}
		
	void FixedUpdate () {
		if (Input.GetAxis ("Horizontal") > moveSensitivity) {
			AttemptMove (1, 0);
		} else if (Input.GetAxis ("Horizontal") < -moveSensitivity) {
			AttemptMove (-1, 0);
		} else if (Input.GetAxis ("Vertical") > moveSensitivity) {
			AttemptMove (0, 1);
		} else if (Input.GetAxis ("Vertical") < -moveSensitivity) {
			AttemptMove (0, -1);
		}
	}

	void EnterCollision(Collider2D other) {
		if (other.gameObject.CompareTag("Wall")) {
			return;
		} else {
			inCollisionWith = other.gameObject;
			//Debug.Log ("In collision with " + inCollisionWith.name);
		}
	}

	private void ResetCollision() {
		inCollisionWith = null;
	}

	public void AttemptMove(float moveHorizontal, float moveVertical) {
		if (!heatController.getIsFrozen () && isMove) {
			if (moveHorizontal != 0) {
				if (moveHorizontal > 0) {
					animator.SetTrigger ("WalkLeft");
				} else {
					animator.SetTrigger ("WalkRight");
				}
				moveVertical = 0;
			} else {
				if (moveVertical > 0) {
					animator.SetTrigger ("WalkUp");
				} else {
					animator.SetTrigger ("WalkDown");
				}
			}

			Vector2 movement = new Vector2 (moveHorizontal * moveDistance, moveVertical * moveDistance);
			Vector2 collisionCheck = new Vector2 ((moveDistance + collisionVector.x / 2) * moveHorizontal,
													(moveDistance + collisionVector.y / 2) * moveVertical);

			Vector2 start = new Vector2 (Mathf.Round(transform.position.x * 10) / 10, Mathf.Round(transform.position.y * 10) / 10);
			Vector2 end = start + movement;
			Vector2 collisionEnd = start + collisionCheck;

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

	public void FreezePlayer() {
		heatController.setIsFrozen(true);
	}
}
