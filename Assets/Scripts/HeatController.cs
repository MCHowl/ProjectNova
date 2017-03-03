using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatController : MonoBehaviour {

	private const float zeroTemp = -273.15f;
	private const float minHeat = 0f;

	private float currentHeat, maxHeat;
	private bool isFrozen;

	public float mass, heatCapacity;
	public float heatThreshold_freeze, heatThreshold_unfreeze; //heatThreshold_unfreeze acts as maximum Heat


	void Start() {
		if (gameObject.tag == "Entity" || gameObject.tag == "Tile") {
			currentHeat = heatThreshold_freeze;
			maxHeat = heatThreshold_unfreeze;
			isFrozen = true;
		} else if (gameObject.tag == "Player") {
			currentHeat = heatThreshold_unfreeze;
			maxHeat = heatThreshold_unfreeze;
			isFrozen = false;
		} else {
			Debug.LogWarning("'HeatController' script attached to invalid object " + gameObject.name);
		}

	}

	void LateUpdate() {
		if (currentHeat <= heatThreshold_freeze) {
			isFrozen = true;
		} else if (currentHeat >= heatThreshold_unfreeze) {
			isFrozen = false;
		}
	}

	private void transferHeat(float transferRequest, HeatController source, HeatController destination) {
		float heatTransferred;

		if (source.currentHeat >= transferRequest) {
			heatTransferred = transferRequest;
		} else {
			heatTransferred = source.currentHeat;
		}

		source.currentHeat = Mathf.Max(minHeat, source.currentHeat - heatTransferred);
		destination.currentHeat = Mathf.Min(maxHeat, destination.currentHeat + heatTransferred);
	}

	public void Burn(HeatController target, HeatController heatReceiver) {
		transferHeat(target.currentHeat, target, heatReceiver);
		Destroy(target.gameObject);
	}

	public void Unfreeze(HeatController target, HeatController heatSource) {
		float heatRequest = target.heatThreshold_unfreeze - target.currentHeat;
		transferHeat(heatRequest, heatSource, target);
	}

	/**
	 * Getters And Setters
	 **/

	public float getCurrentHeat() {
		return currentHeat;
	}

	public float getTemperature(float heat) {
		return ((mass * heatCapacity * zeroTemp) + heat) / (mass * heatCapacity);
	}

	public void setHeatToMaximum() {
		currentHeat = maxHeat;
	}

	public bool getIsFrozen() {
		return isFrozen;
	}
}
