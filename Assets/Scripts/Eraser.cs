using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eraser : MonoBehaviour {

	//public GameObject glove;
	public Remover removerScript;
	private SteamVR_TrackedController controller;
	private bool showGlove = false;

	public StickerTool myTool;
	private bool inUse;
	private GameObject objectToRemove;

	void OnEnable()
	{
		if(controller==null)
		{
			controller = GetComponent<SteamVR_TrackedController>();
		}
		controller.TriggerClicked += HandleDown;
		controller.TriggerUnclicked += HandleUp;

		removerScript.OnCollide += OnRemoverCollide;

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= HandleDown;
		controller.TriggerUnclicked -= HandleUp;

		removerScript.OnCollide -= OnRemoverCollide;

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
		
//		showGlove = true;
//		removerScript.gameObject.SetActive (true);

		if (objectToRemove)
			Destroy (objectToRemove);
	}

	public void HandleUp(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
//		showGlove = false;
//		removerScript.gameObject.SetActive (false);
		objectToRemove = null;
	}

	public void OnRemoverCollide(Collider _col)
	{
		if (!inUse)
			return;

		if(_col.gameObject.tag == "Sticker")
		{
			//Destroy (_col.gameObject);
			objectToRemove = _col.gameObject;
		}
	}
}
