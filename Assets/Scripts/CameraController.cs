using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	private GameObject player;
	private Vector3 offset;

	void Start () {
		offset = new Vector3(0, 0, -10);

		player = GameObject.FindWithTag("Player");
		if (player != null) {
			transform.position = player.transform.position + offset;
		} else {
			Debug.Log("Cannot find 'PlayerController' script");
		}
	}

	void LateUpdate () {
		transform.position = player.transform.position + offset;
	}
}
