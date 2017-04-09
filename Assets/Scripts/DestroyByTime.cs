using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByTime : MonoBehaviour {

	private float destroyTime;
	public float persistanceDuration;

	// Use this for initialization
	void Start () {
		destroyTime = Time.time + persistanceDuration;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > destroyTime) {
			Destroy (this.gameObject);
		}
	}
}
