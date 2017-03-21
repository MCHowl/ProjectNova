using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatController : MonoBehaviour {

	public delegate void ObjectUnfrozen(GameObject gameObject);
	public static event ObjectUnfrozen ObjectUnfrozenEvent;

	public delegate void ObjectFrozen(GameObject gameObject);
	public static event ObjectFrozen ObjectFrozenEvent;

	private const float zeroTemp = -273.15f;
	private const float minHeat = 0f;

	private float currentHeat, maxHeat;
	private bool isFrozen;

	public float mass, heatCapacity;
	public float heatThreshold_freeze, heatThreshold_unfreeze;

	public Sprite frozen, unfrozen;
	private SpriteRenderer spriteRenderer;

	void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();

		if (gameObject.CompareTag("Tile")) {
			currentHeat = heatThreshold_freeze;
			maxHeat = heatThreshold_unfreeze;
			setFrozen();
		} else if (gameObject.CompareTag("Player") || gameObject.CompareTag("Source")) {
			currentHeat = heatThreshold_unfreeze;
			maxHeat = heatThreshold_unfreeze;
			setUnfrozen();
		} else {
			Debug.LogWarning("'HeatController' script attached to invalid object " + gameObject.name);
		}
	}

	void LateUpdate() {
		if (currentHeat <= heatThreshold_freeze && !getIsFrozen()) {
			setFrozen();

		} else if (currentHeat >= heatThreshold_unfreeze && getIsFrozen()) {
			setUnfrozen();
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
		if (isFrozen) {
			setUnfrozen();
		}
	}

	public bool getIsFrozen() {
		return isFrozen;
	}

	private void setFrozen() {
		isFrozen = true;
		spriteRenderer.sprite = frozen;
		ObjectFrozenEvent(gameObject);
	}

	private void setUnfrozen() {
		isFrozen = false;
		spriteRenderer.sprite = unfrozen;
		ObjectUnfrozenEvent(gameObject);
	}
}
