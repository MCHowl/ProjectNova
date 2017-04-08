using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoverOver : MonoBehaviour {
public	Button button;
	public Image buttonimg;
public Image darkbutton;
	public Image lightbutton;


	// Use this for initialization
	void Start () {

	}

	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseOver()
	{
		transform.GetComponent<Image>();

	}

	void OnMouseExit()
	{
	}
}
