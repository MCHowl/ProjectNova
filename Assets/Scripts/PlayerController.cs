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

	private Vector2 collisionVector;
	private float moveDistance = 1.0f;

	private float moveTime = 0.125f;
	private float inverseMoveTime;

	private bool isMove = true;

	private float[][] direction = new float[][] {
									new float[]{0,1},
									new float[]{0,-1},
									new float[]{1,0},
									new float[]{-1,0} };
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

		// Check in 4 directions for any heat sources
		if (Input.GetKeyDown (KeyCode.Space)) {
			for (int i = 0; i < direction.Length; i++) {
				float moveHorizontal = direction[i][0];
				float moveVertical = direction[i][1];

				Vector2 collisionCheck = new Vector2 ((moveDistance + collisionVector.x / 2) * moveHorizontal,
					(moveDistance + collisionVector.y / 2) * moveVertical);

				Vector2 start = new Vector2 (Mathf.Round(transform.position.x * 10) / 10, Mathf.Round(transform.position.y * 10) / 10);
				Vector2 collisionEnd = start + collisionCheck;

				boxCollider.enabled = false;
				RaycastHit2D hit = Physics2D.Linecast (start, collisionEnd, blockingLayer);
				boxCollider.enabled = true;

				if (hit.transform != null) {
					if (hit.collider.gameObject.CompareTag ("Source")) {
						HeatController targetHeatController = hit.collider.gameObject.GetComponent<HeatController>();

						if (targetHeatController == null) {
							Debug.LogError ("Unable to find 'HeatController' script on " + hit.collider.name);
						} else {
							audioSources[1].Play();
							heatController.Unfreeze(heatController, targetHeatController);
						}
					}
				}
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
					StartCoroutine(Turn("TurnUp"));
				}
			} else if (Input.GetKey (KeyCode.DownArrow)) {
				isMove = false;
				if (curr_dir == dir.down) {
					AttemptMove (0, -1);
				} else {
					curr_dir = dir.down;
					StartCoroutine(Turn("TurnDown"));
				}
			} else if (Input.GetKey (KeyCode.LeftArrow)) {
				isMove = false;
				if (curr_dir == dir.left) {
					AttemptMove (-1, 0);
				} else {
					curr_dir = dir.left;
					StartCoroutine(Turn("TurnRight"));  //Note: Left and Right animations are muddled up
				}
			} else if (Input.GetKey (KeyCode.RightArrow)) {
				isMove = false;
				if (curr_dir == dir.right) {
					AttemptMove (1, 0);
				} else {
					curr_dir = dir.right;
					StartCoroutine(Turn("TurnLeft"));
				}
			}
		}
	}

	public void AttemptMove(float moveHorizontal, float moveVertical) {
		if (!heatController.getIsFrozen ()) {
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
				StartCoroutine(Move (end));
			} else {
				isMove = true;
			}
		}
	}

	private IEnumerator Move(Vector3 end) {
		audioSources[0].Play();
		animator.SetTrigger("Walk");

		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		while (sqrRemainingDistance > float.Epsilon) {
			Vector3 newPosition = Vector3.MoveTowards (rb2d.position, end, inverseMoveTime * Time.deltaTime);
			rb2d.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}

		isMove = true;
	}

	private IEnumerator Turn(string trigger) {
		animator.SetTrigger (trigger);
		yield return new WaitForSeconds (0.1f);
		isMove = true;
	}

	public void FreezePlayer() {
		heatController.setIsFrozen(true);
	}
}
