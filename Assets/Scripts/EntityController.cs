using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour {

	public LayerMask blockingLayer;

	private HeatController heatController;
	private BoxCollider2D boxCollider;
	private Rigidbody2D rb2d;

	private Vector2[] directionList = { new Vector2(0,0),
										new Vector2(0,1),
										new Vector2(1,0),
										new Vector2(0,-1),
										new Vector2(-1,0)};

	private float moveDelay = 2f;
	private float moveTime = 0.25f;
	private float inverseMoveTime;

	void Start () {
		heatController = GetComponent<HeatController>();
		boxCollider = GetComponent<BoxCollider2D>();
		rb2d = GetComponent<Rigidbody2D>();

		inverseMoveTime = 1f / moveTime;

		Debug.Log(heatController.getCurrentHeat() +", " + heatController);

		InvokeRepeating("AttemptMove", moveDelay, moveDelay);
	}

	void AttemptMove() {
		if (!heatController.getIsFrozen()) {
			Vector2 start = transform.position;
			Vector2 end = start + directionList [Random.Range (0, directionList.Length)];

			boxCollider.enabled = false;
			RaycastHit2D hit = Physics2D.Linecast(start, end, blockingLayer);
			boxCollider.enabled = true;

			if (hit.transform == null) {
				StartCoroutine(Move(end));
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
