using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eraser : MonoBehaviour {

	//public GameObject glove;
	public Glove gloveScript;
	private SteamVR_TrackedController controller;
	private bool showGlove = false;

	public StickerTool myTool;
	private bool inUse;

	void OnEnable()
	{
		if(controller==null)
		{
			controller = GetComponent<SteamVR_TrackedController>();
		}
		controller.Gripped += HandleDown;
		controller.Ungripped += HandleUp;

		gloveScript.OnCollide += OnGloveCollide;

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		controller.Gripped -= HandleDown;
		controller.Ungripped -= HandleUp;

		gloveScript.OnCollide -= OnGloveCollide;

		if(myTool!=null)
			myTool.OnChangeToolStatus -= OnToolStatusChange;
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;
	}

	public void HandleDown(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
		showGlove = true;
		gloveScript.gameObject.SetActive (true);
	}

	public void HandleUp(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
		showGlove = false;
		gloveScript.gameObject.SetActive (false);
	}

	public void OnGloveCollide(Collider _col)
	{
		if (!inUse)
			return;
		
		if(_col.gameObject.tag == "Sticker")
		{
			if(showGlove)
				Destroy (_col.gameObject);
		}
	}
}
