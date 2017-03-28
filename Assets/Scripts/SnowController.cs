using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowController : MonoBehaviour {

	HeatController heat;
	float end_y;
	float fallDistance = 5;

	void Start () {
		heat = GetComponent<HeatController>();
		end_y = transform.position.y - fallDistance;
	}

	void Update () {
		if (transform.position.y < end_y) {
			Destroy(this.gameObject);
		}

		if (!heat.getIsFrozen ()) {
			Destroy(this.gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag("Player")) {
			HeatController player = other.gameObject.GetComponent<HeatController>();

			heat.Unfreeze (heat, player);
		}
	}


}
