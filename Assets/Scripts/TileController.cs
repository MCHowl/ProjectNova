using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour {

	private HeatController heatController;

	void Start () {
		heatController = GetComponent<HeatController>();	
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (heatController.getIsFrozen()) {
			// Attempt to unfreeze tile
			HeatController sourceHeatController = other.gameObject.GetComponent<HeatController> ();

			if (sourceHeatController != null) {
				heatController.Unfreeze(heatController, sourceHeatController);
			} else {
				Debug.LogWarning ("Unable to find 'HeatController' script on " + other.gameObject.name);
			}
		}
	}
}
