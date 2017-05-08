/// <summary>
/// Vive input - reference: Unity VR Samples
/// Encapsulates all the input required for vive controller
/// Need to be attached to main camera, cause controller will be on and off
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveInput : MonoBehaviour {

	public enum SwipeDirection
	{
		NONE,
		UP,
		DOWN,
		LEFT,
		RIGHT
	};

	public event Action OnClick;
	public event Action OnDown;
	public event Action OnUp;
	//TODO: SWIPE!

	private Vector3 m_ClickPosition;
	private Vector3 m_DownPosition;
	private Vector3 m_UpPosition;
	private Vector3 m_LastUpTime;

	private void Update()
	{
		CheckInput ();
	}

	private void CheckInput()
	{
		// Move to ViveController.cs
		/*
		var device = SteamVR_Controller.Input ((int)trackedObj.index);

		if (device.GetTouchDown (SteamVR_Controller.ButtonMask.Trigger))
		{
			m_TriggerDownPosition = attachPoint.position;

			if (OnTriggerDown != null)
				OnTriggerDown ();
		}

		if(device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
		{
			m_TriggerUpPosition = attachPoint.position;

			// TODO: SWIPE!

			if (OnTriggerUp != null)
				OnTriggerUp ();
		}

		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			m_TriggerClickPosition = attachPoint.position;

			if (OnTriggerClick != null)
				OnTriggerClick ();
		}
		*/
	}

	// Ensure all events are unsubscribed when this is destroyed
	private void OnDestroy()
	{
		OnClick = null;
		OnDown = null;
		OnUp = null;
	}
}
