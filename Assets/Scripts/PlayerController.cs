using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public delegate void PlayerWarning();
	public static event PlayerWarning PlayerWarningEvent;

	public LayerMask blockingLayer;

	private AudioSource[] audioSources;
	private Animator animator;
	private Rigidbody2D rb2d;
	private BoxCollider2D boxCollider;
	private HeatController heatController;

	private GameObject inCollisionWith = null;

	private Vector2 collisionVector;
	private float moveDistance = 1.0f;

	private float moveTime = 0.125f;
	private float moveSensitivity = 0.5f;
	private float inverseMoveTime;

	private bool isMove = true;

	private enum dir {up, down, left, right};
	private dir curr_dir = dir.down;

	void Start() {
		audioSources = GetComponents<AudioSource>();
		animator = GetComponent<Animator>();
		rb2d = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		heatController = GetComponent<HeatController>();

		inverseMoveTime = 1f / moveTime;
		collisionVector = boxCollider.size;
	}

	void Update() {
		if (heatController.getPercentHeat() <= 0.35) {
			PlayerWarningEvent ();
		}


		if (inCollisionWith != null) {
			HeatController targetHeatController = inCollisionWith.GetComponent<HeatController>();

			if (targetHeatController != null) {
				if (inCollisionWith.CompareTag("Source")) {
					if (Input.GetKeyDown(KeyCode.Space)) {
						audioSources[1].Play();
						heatController.Unfreeze(heatController, targetHeatController);
					}
				}
			} else {
				Debug.LogError("Unable to find 'HeatController' script on " + inCollisionWith.name);
			}
		}
	}
		
	void FixedUpdate () {
		// Check if player can move
		if (isMove && Input.anyKey) {
			// Check orientation for turn/movement
			if (Input.GetKey (KeyCode.UpArrow)) {
				isMove = false;
				if (curr_dir == dir.up) {
					AttemptMove (0, 1);
				} else {
					curr_dir = dir.up;
					animator.SetTrigger ("TurnUp");
					isMove = true;
				}
			} else if (Input.GetKey (KeyCode.DownArrow)) {
				isMove = false;
				if (curr_dir == dir.down) {
					AttemptMove (0, -1);
				} else {
					curr_dir = dir.down;
					animator.SetTrigger ("TurnDown");
					isMove = true;
				}
			} else if (Input.GetKey (KeyCode.LeftArrow)) {
				isMove = false;
				if (curr_dir == dir.left) {
					AttemptMove (-1, 0);
				} else {
					curr_dir = dir.left;
					animator.SetTrigger ("TurnRight");
					isMove = true;
				}
			} else if (Input.GetKey (KeyCode.RightArrow)) {
				isMove = false;
				if (curr_dir == dir.right) {
					AttemptMove (1, 0);
				} else {
					curr_dir = dir.right;
					animator.SetTrigger ("TurnLeft");
					isMove = true;
				}
			}
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
		if (!heatController.getIsFrozen ()) {
			audioSources[0].Play();

			animator.SetTrigger("Walk");

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
				ResetCollision();
				StartCoroutine(Move (end));
			} else {
				EnterCollision(hit.collider);
				isMove = true;
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
