using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

	public LayerMask blockingLayer;

	private HeatController heatController;
	private BoxCollider2D boxCollider;
	private Rigidbody2D rb2d;

	private Transform target;

	private float moveDelay = 1f;
	private float moveTime = 0.5f;
	private float inverseMoveTime;

	void Start () {
		heatController = GetComponent<HeatController>();
		boxCollider = GetComponent<BoxCollider2D>();
		rb2d = GetComponent<Rigidbody2D>();

		target = GameObject.FindGameObjectWithTag ("Player").transform;

		inverseMoveTime = 1f / moveTime;

		InvokeRepeating("AttemptMove", moveDelay, moveDelay);
	}

	void Update() {
		if (!heatController.getIsFrozen ()) {
			Destroy (this.gameObject);
		}
	}

	void AttemptMove() {
		if (heatController.getIsFrozen()) {
			Vector2 start = transform.position;

			int xDir = 0;
			int yDir = 0;

			if (Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon) {
				if (target.position.y > transform.position.y) {
					yDir = 1;
				} else {
					yDir = -1;
				}
			} else {
				if (target.position.x > transform.position.x) {
					xDir = 1;
				} else {
					xDir = -1;
				}
			}

			Vector2 movement = new Vector2 (xDir, yDir);

			Vector2 end = start + movement;

			boxCollider.enabled = false;
			RaycastHit2D hit = Physics2D.Linecast(start, end, blockingLayer);
			boxCollider.enabled = true;

			if (hit.transform == null) {
				StartCoroutine(Move(end));
			} else if (hit.collider.CompareTag("Player")) {
				HeatController player = hit.collider.GetComponent<HeatController>();
				heatController.Unfreeze (heatController, player);
				Destroy (this.gameObject);
			}
		}
	}

	IEnumerator Move(Vector3 end) {
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		while (sqrRemainingDistance > float.Epsilon) {
			Vector3 newPosition = Vector3.MoveTowards (rb2d.position, end, inverseMoveTime * Time.deltaTime);
			rb2d.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;

			yield return null;
		}
	}
}
