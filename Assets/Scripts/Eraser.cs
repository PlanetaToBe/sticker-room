using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eraser : MonoBehaviour {

	public GameObject glove;
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
	}

	void OnDisable()
	{
		controller.Gripped -= HandleDown;
		controller.Ungripped -= HandleUp;
	}

	public void HandleDown(object sender, ClickedEventArgs e)
	{
		showGlove = true;
		glove.SetActive (true);
	}

	public void HandleUp(object sender, ClickedEventArgs e)
	{
		showGlove = false;
		glove.SetActive (false);
	}
}
