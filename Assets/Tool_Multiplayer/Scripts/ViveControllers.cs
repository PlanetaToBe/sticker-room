using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllers : MonoBehaviour {

	public SteamVR_TrackedObject controller1;
	public SteamVR_TrackedObject controller2;
	[HideInInspector]
	public SocketManagement socketManagement;
	[HideInInspector]
	public PlayerManagement playerManagement;

	private bool controller1_down = false;
	private bool controller2_down = false;

	void Update()
	{
		if(!controller1.isActiveAndEnabled || !controller2.isActiveAndEnabled)
			return;
		
		// check if both controllers are triggered
		var device1 = SteamVR_Controller.Input ((int)controller1.index);
		var device2 = SteamVR_Controller.Input ((int)controller2.index);

		if (device1.GetTouch (SteamVR_Controller.ButtonMask.Trigger) && device2.GetTouch (SteamVR_Controller.ButtonMask.Trigger))
		{
			Debug.Log ("Get both TouchDown");
			controller1_down = true;
			controller2_down = true;
		}
			
		if(device1.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
		{
			//Debug.Log ("Get c_1 TouchUp");
			if (!controller2_down)
			{
				Debug.Log ("Get both TouchUp");
			}

			controller1_down = false;
		}

		if(device2.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
		{
			//Debug.Log ("Get c_2 TouchUp");
			if (!controller1_down)
			{
				Debug.Log ("Get both TouchUp");
			}

			controller2_down = false;
		}

		if(device1.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad) || device2.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
		{
			int _duration =  Mathf.FloorToInt(Time.time - playerManagement.startTime);
			// Send data to Firebase
			/*
			socketManagement.firebaseManager.SaveNewUser (
				socketManagement.myName,
				socketManagement.firebaseManager.scoreManager.scoreCount,
				_duration
			);
			*/
			//Debug.Log ("Save data!");
			device1.TriggerHapticPulse (1000);
			device2.TriggerHapticPulse (1000);
		}

	}

	public void DeviceVibrate()
	{
		var	device = SteamVR_Controller.Input ((int)controller1.index);		
		device.TriggerHapticPulse (1000);
		var	device2 = SteamVR_Controller.Input ((int)controller2.index);		
		device2.TriggerHapticPulse (1000);
	}
}
