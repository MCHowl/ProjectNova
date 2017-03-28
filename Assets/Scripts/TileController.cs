using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour {

	private HeatController heatController;

	void Start () {
		heatController = GetComponent<HeatController>();	
	}

	/**
	 * Attempt to transfer heat to the tile from the player if frozen,
	 * otherwise attempt to transfer heat to the enemy.
	 **/
	void OnTriggerEnter2D(Collider2D other) {
		HeatController sourceHeatController = other.gameObject.GetComponent<HeatController> ();

		if (heatController.getIsFrozen ()) {
			if (sourceHeatController != null) {
				if (other.gameObject.CompareTag ("Player")) {
					heatController.Unfreeze (heatController, sourceHeatController);
				}
			} else {
				Debug.LogWarning ("Unable to find 'HeatController' script on " + other.gameObject.name);
			}
		} else {
			if (sourceHeatController != null) {
				if (other.gameObject.CompareTag ("Enemy")) {
					heatController.Unfreeze (sourceHeatController, heatController);
				}
			} else {
				Debug.LogWarning ("Unable to find 'HeatController' script on " + other.gameObject.name);
			}
		}
	}
}
