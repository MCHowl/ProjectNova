using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour {

	HeatController heatController;
	Rigidbody2D rb2d;

	private int move;

	void Start () {
		heatController = GetComponent<HeatController>();
		rb2d = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate () {
		if (!heatController.getIsFrozen()) {
			//Move Randomly
			//Refer to 2D Rougelike Enemy Movement
			Vector2 newPos = new Vector2 (rb2d.position.x + Mathf.Pow(-1, move), rb2d.position.y);
			rb2d.MovePosition (newPos);		
			move++;
		}
	}
}
