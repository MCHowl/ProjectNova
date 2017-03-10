using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float speed;

	private Rigidbody2D rb2d;
	private HeatController heatController;

	private GameObject inCollisionWith = null;

	void Start() {
		rb2d = GetComponent<Rigidbody2D>();
		heatController = GetComponent<HeatController>();
	}

	void Update() {
		Debug.Log ("Current Player Heat: " + heatController.getCurrentHeat ());

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
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");

		if (!heatController.getIsFrozen ()) {
			Vector2 movement = new Vector2 (moveHorizontal, moveVertical);
			rb2d.AddForce (movement * speed);
		}

	}

	void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.CompareTag("Wall")) {
			return;
		} else if (other.gameObject.CompareTag("Entity")) {
			inCollisionWith = other.gameObject;
		} else if (other.gameObject.CompareTag("Source")) {
			heatController.setHeatToMaximum();
		}
	}

	void OnCollisionExit2D() {
		ResetCollision();
	}

	private void ResetCollision() {
		inCollisionWith = null;
	}

	public void RespawnAtLocation(Transform transform) {
		StartCoroutine (spawnCoroutine (transform));
	}

	public IEnumerator spawnCoroutine(Transform transform) {
		yield return new WaitForSeconds(1);
		ResetCollision ();
		heatController.setHeatToMaximum();
		rb2d.position = new Vector2(transform.position.x, transform.position.y);
		yield return null;
	}
}
