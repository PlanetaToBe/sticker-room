using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolHub : MonoBehaviour {

	public SteamVR_TrackedController controller;
	public SteamVR_Controller.Device Device
	{
		get { return SteamVR_Controller.Input ((int)controller.controllerIndex); }
	}
	public float rotDegreePerStep = 5f;

	private bool isTouching = false;
	private List<GameObject> tools = new List<GameObject> ();
	private Vector2 pastTouchpadAxis;
	private List<float> toolRotateZones = new List<float> ();
	private int toolLayer;

	//============================================================
	//============================================================
	void Start()
	{
		var eachR = 360f / transform.childCount;
		for(int i=0; i<transform.childCount; i++)
		{
			var _t = transform.GetChild (i).gameObject;
			tools.Add (_t);
			toolRotateZones.Add (eachR*i);
		}

		toolLayer = 1 << 10;
	}

	void OnEnable()
	{
		controller.PadTouched += OnTouch;
		controller.PadUntouched += OnTouch;
		controller.PadTouching += OnTouching;
	}

	void OnDisalbe()
	{
		controller.PadTouched -= OnTouch;
		controller.PadUntouched -= OnTouch;
		controller.PadTouching -= OnTouching;
	}

	//============================================================
	//============================================================
	public void OnTouch (object sender, ClickedEventArgs e)
	{
		isTouching = true;
		pastTouchpadAxis = GetTouchpadAxis ();
		DeviceVibrate ();
	}

	public void OnTouchOut (object sender, ClickedEventArgs e)
	{
		isTouching = false;

		// snap to the closest tool
		// enable the function
	}

	public void OnTouching (object sender, ClickedEventArgs e)
	{
		Vector2 currTouchpadAxis = GetTouchpadAxis ();
		//Debug.Log (currTouchpadAxis.x);

		//steps on X-Axis: -1~1 break down into 10 steps, each step: 0.2f
		float dist = currTouchpadAxis.x - pastTouchpadAxis.x;
		if( Mathf.Abs(dist) > 0.2f )
		{
			if(dist>0)
			{
				// swipe right
				transform.Rotate(Vector3.forward * rotDegreePerStep);
			}
			else
			{
				// swipe left
				transform.Rotate(-Vector3.forward * rotDegreePerStep);
			}
			pastTouchpadAxis = currTouchpadAxis;
			DeviceVibrate ();

			// Raycasting to detect which tool is showing up
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.up, out hit, 5f, toolLayer))
			{
				if(hit.transform.localScale.x < 0.12f)
				{
					int hitIndex;
					int.TryParse(hit.transform.gameObject.name, out hitIndex);
					hit.transform.localScale *= 2;

					for(int i=0; i<tools.Count; i++)
					{
						if (i != hitIndex) {
							tools [i].transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
						}
					}
				}
			}
		}
		// if not, then wait until it's accumulated to 0.2f
	}

	//===========================================================================
	public Vector2 GetTouchpadAxis()
	{
		var device = SteamVR_Controller.Input((int)controller.controllerIndex);
		return device.GetAxis();
	}

	public void DeviceVibrate()
	{
		Device.TriggerHapticPulse (1000);
	}
}
