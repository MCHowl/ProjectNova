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
	public float freezeTemp, unfreezeTemp;
	private float heatThreshold_freeze, heatThreshold_unfreeze;

	public Sprite frozen, unfrozen;
	private SpriteRenderer spriteRenderer;

	void Awake() {
		heatThreshold_freeze = getHeatValue (freezeTemp);
		heatThreshold_unfreeze = getHeatValue (unfreezeTemp);
	}

	void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();

		// Set the starting heat of all objects
		maxHeat = heatThreshold_unfreeze;

		if (gameObject.CompareTag("Tile") || gameObject.CompareTag("Enemy") || gameObject.CompareTag("Hazard")) {
			currentHeat = heatThreshold_freeze;
			setFrozen();
		} else if (gameObject.CompareTag("Player") || gameObject.CompareTag("Source")) {
			currentHeat = heatThreshold_unfreeze;
			setUnfrozen();
		} else {
			Debug.LogWarning("'HeatController' script attached to invalid object " + gameObject.name);
		}
	}

	void LateUpdate() {
		// Check if the object is in the correct state
		if (currentHeat <= heatThreshold_freeze && !getIsFrozen()) {
			setFrozen();
		} else if (currentHeat >= heatThreshold_unfreeze && getIsFrozen()) {
			setUnfrozen();
		}
	}

	/**
	 * Attempt to transfer heat from the source HeatController to the destination HeatController
	 * The heat transfer will not exceed the maximum & minimum heat level of the respective HeatControllers
	 **/
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

	/**
	 * Attempt to bring the target to its unfreeze threshold heat level
	 * using heat taken from the source HeatController.
	 **/

	public void Unfreeze(HeatController target, HeatController heatSource) {
		float heatRequest = target.heatThreshold_unfreeze - target.currentHeat;
		transferHeat(heatRequest, heatSource, target);
	}

	/**
	 * Getters And Setters
	 **/

	public void setEnemyHealth(float health) {
		heatThreshold_unfreeze = heatThreshold_freeze + health;
		maxHeat = heatThreshold_unfreeze;
	}

	public float getCurrentHeat() {
		return currentHeat;
	}

	public float getFreezeThreshold() {
		return heatThreshold_freeze;
	}

	public float getUnfreezeThreshold() {
		return heatThreshold_unfreeze;
	}

	public float getPercentHeat() {
		return currentHeat / heatThreshold_unfreeze;
	}

	public float getTemperature(float heat) {
		return ((mass * heatCapacity * zeroTemp) + heat) / (mass * heatCapacity);
	}

	public void setIsFrozen(bool state) {
		if (getIsFrozen() != state) {
			if (state) {
				currentHeat = heatThreshold_freeze;
			} else {
				currentHeat = heatThreshold_unfreeze;
			}
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

		if (gameObject.CompareTag ("Tile")) {
			StartCoroutine (Fade ());
		} else {
			spriteRenderer.sprite = unfrozen;
		}
			
		ObjectUnfrozenEvent(gameObject);
	}

	private float getHeatValue (float temperature) {
		return mass * heatCapacity * (temperature + 273.15f);

	}

	IEnumerator Fade()	{
		Color spriteColor = Color.white;
		float fade = 1f;
		float fadeInTime = 0.2f;
		float fadeOutTime = 0.1f;
		float startTime = Time.time;

		while(fade > 0f) {
			fade = Mathf.Lerp(1f, 0f, (Time.time - startTime) / fadeOutTime);
			spriteColor.a = fade;
			spriteRenderer.color = spriteColor;
			yield return null;
		}
		fade = 0f;
		spriteColor.a = fade;
		spriteRenderer.color = spriteColor;
		spriteRenderer.sprite = unfrozen;
		yield return new WaitForSeconds(0.1f);

		startTime = Time.time;
		while(fade < 1f) {
			fade = Mathf.Lerp(0f, 1f, (Time.time - startTime) / fadeInTime);
			spriteColor.a = fade;
			spriteRenderer.color = spriteColor;
			yield return null;
		}

		//Make sure it's set to exactly 1f
		fade = 1f;
		spriteColor.a = fade;
		spriteRenderer.color = spriteColor;
		yield return null;
	}

}
