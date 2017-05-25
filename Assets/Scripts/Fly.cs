using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour {

	public ToolHub[] toolHubs;
	public SteamVR_TrackedController[] controllers;

	private int padDownCount = 0;
	private List<Transform> controllerTrans = List<Transform> ();

	void OnEnable()
	{
		for(int i=0; i<controllers.Length; i++)
		{
			controllers[i].PadClicked += HandlePadDown;
			controllers[i].PadUnclicked += HandlePadUp;
			controllers[i].PadTouching += HandlePadTouching;
		}
	}

	void OnDisable()
	{
		for(int i=0; i<controllers.Length; i++)
		{
			controllers[i].PadClicked -= HandlePadDown;
			controllers[i].PadUnclicked -= HandlePadUp;
			controllers[i].PadTouching -= HandlePadTouching;
		}
	}

	void Start()
	{
		for(int i=0; i<controllers.Length; i++)
		{
			controllerTrans.Add(controllers[i].transform);
		}
	}

	private void HandlePadDown(object sender, ClickedEventArgs e)
	{
		padDownCount++;

		if(padDownCount==2)
		{
			// disable all tool functions

		}
	}

	private void HandlePadUp(object sender, ClickedEventArgs e)
	{
		padDownCount--;
		Debug.Log ("padDownCount: " + padDownCount);
	}

	private void HandlePadTouching(object sender, ClickedEventArgs e)
	{
		
	}
}
