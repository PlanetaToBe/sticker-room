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
	private List<GameObject> toolObjects = new List<GameObject> ();
	private Vector2 pastTouchpadAxis;
	private float pastTouchValue;
	private List<float> toolRotateZones = new List<float> ();
	private int toolLayer;
	public int currToolInUse;
	private List<StickerTool> stickerTools = new List<StickerTool> ();
	private bool touchStop = true;
	private StickerTool currStickerTool;

	//============================================================
	//============================================================
	void Start()
	{
		var eachR = 360f / transform.childCount;
		for(int i=0; i<transform.childCount; i++)
		{
			var _t = transform.GetChild (i).gameObject;
			toolObjects.Add (_t);
			toolRotateZones.Add (eachR*i);

			var s_t = _t.GetComponent<StickerTool> ();
			s_t.ToolIndex = i;
			s_t.IdealAngle = eachR * i;
			stickerTools.Add (s_t);
		}

		toolLayer = 1 << 10;
	}

	void OnEnable()
	{
		if (controller != null)
		{
			controller.PadTouched += OnTouch;
			controller.PadUntouched += OnTouchOut;
			controller.PadTouching += OnTouching;
		}
	}

	void OnDisalbe()
	{
		if (controller != null)
		{
			controller.PadTouched -= OnTouch;
			controller.PadUntouched -= OnTouchOut;
			controller.PadTouching -= OnTouching;
		}
	}

	void Update()
	{
		if(controller==null)
		{
			float moveHorizontal = Input.GetAxis ("Mouse X");
			ToolSwiping (moveHorizontal);

			if (moveHorizontal == 0 && !touchStop) {
				touchStop = true;
				OnTouchStop ();
			} else if (moveHorizontal != 0 && touchStop) {
				touchStop = false;
			}
		}
	}

	//============================================================
	//============================================================
	public void OnTouch (object sender, ClickedEventArgs e)
	{
		isTouching = true;
		pastTouchpadAxis = GetTouchpadAxis ();
		DeviceVibrate ();
	}

	public void OnTouchStop ()
	{
		// snap to the closest tool
		if (currStickerTool != null)
		{
			currStickerTool.TurnToIdealAngle(transform.localEulerAngles.z);
		}
			
		// enable the function
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
				transform.Rotate(transform.forward * rotDegreePerStep);
			}
			else
			{
				// swipe left
				transform.Rotate(-transform.forward * rotDegreePerStep);
			}
			pastTouchpadAxis = currTouchpadAxis;
			DeviceVibrate ();

			// Raycasting to detect which tool is showing up
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.up, out hit, 5f, toolLayer))
			{
				var s_t = hit.transform.gameObject.GetComponent<StickerTool> ();
				if(!s_t.inUse)
				{
					s_t.EnableTool ();

					for(int i=0; i<stickerTools.Count; i++)
					{
						if (i != s_t.ToolIndex && stickerTools[i].inUse)
						{
							stickerTools [i].DisableTool ();
						}
					}
				}
			}
		}
		// if not, then wait until it's accumulated to 0.2f
	}

	void ToolSwiping(float currTouchValue)
	{
//		float dist = currTouchValue - pastTouchValue;
		if( Mathf.Abs(currTouchValue) > 0.15f )
		{
			if(currTouchValue>0)
			{
				// swipe right
				transform.Rotate(transform.forward * rotDegreePerStep);
			}
			else
			{
				// swipe left
				transform.Rotate(-transform.forward * rotDegreePerStep);
			}
			pastTouchValue = currTouchValue;
//			DeviceVibrate ();

//			float fullRotation = (transform.localEulerAngles.z + 360f) % 360f;

			// Raycasting to detect which tool to show up
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.parent.up, out hit, 5f, toolLayer))
			{
				var s_t = hit.transform.gameObject.GetComponent<StickerTool> ();
				if(!s_t.inUse)
				{					
					// disable
					for(int i=0; i<stickerTools.Count; i++)
					{
						if (i != s_t.ToolIndex && stickerTools[i].inUse)
						{
							stickerTools [i].DisableTool ();
						}
					}

					// enable
					s_t.EnableTool ();
					currStickerTool = s_t;
					currToolInUse = s_t.ToolIndex;
				}
			}
		}
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
