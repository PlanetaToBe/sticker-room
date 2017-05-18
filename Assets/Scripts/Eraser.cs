using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eraser : MonoBehaviour {

	//public GameObject glove;
	public Glove gloveScript;
	private SteamVR_TrackedController controller;
	private bool showGlove = false;

	void OnEnable()
	{
		if(controller==null)
		{
			controller = GetComponent<SteamVR_TrackedController>();
		}
		controller.Gripped += HandleDown;
		controller.Ungripped += HandleUp;

		gloveScript.OnCollide += OnGloveCollide;
	}

	void OnDisable()
	{
		controller.Gripped -= HandleDown;
		controller.Ungripped -= HandleUp;

		gloveScript.OnCollide -= OnGloveCollide;
	}

	public void HandleDown(object sender, ClickedEventArgs e)
	{
		showGlove = true;
		gloveScript.gameObject.SetActive (true);
	}

	public void HandleUp(object sender, ClickedEventArgs e)
	{
		showGlove = false;
		gloveScript.gameObject.SetActive (false);
	}

	public void OnGloveCollide(Collider _col)
	{
		if(_col.gameObject.tag == "Sticker")
		{
			if(showGlove)
				Destroy (_col.gameObject);
		}
	}
}
