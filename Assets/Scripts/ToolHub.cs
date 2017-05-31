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
	private Vector2 currTouchpadAxis;
	private Vector2 pastTouchpadAxis;
	private float pastTouchValue;

	private List<float> toolRotateZones = new List<float> ();
	private int toolLayer;
	public int currToolIndex = -1;
	private int toolIndexCount = -1;
	private List<StickerTool> stickerTools = new List<StickerTool> ();
	private StickerTool currStickerTool;

	// for macbook touchpad simulating vive controller
	private bool touchStop = true;
	private float eachR;
	private bool inRotating = false;

	private bool m_enable = true;
	public bool ToolsetEnable
	{
		get { return m_enable; }
		set { m_enable = value; }
	}

	//============================================================
	void Start()
	{
		eachR = 360f / transform.childCount;
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

		CheckRaycast();
	}

	void OnEnable()
	{
		if (controller != null)
		{
			controller.PadClicked += OnClick;

			controller.PadTouched += OnTouch;
			controller.PadUntouched += OnTouchOut;
			controller.PadTouching += OnTouching;
		}
	}

	void OnDisable()
	{
		if (controller != null)
		{
			controller.PadClicked -= OnClick;

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
	public void OnTouch (object sender, ClickedEventArgs e)
	{
		if (!ToolsetEnable)
			return;
		
		isTouching = true;
		currTouchpadAxis = pastTouchpadAxis = GetTouchpadAxis ();
		DeviceVibrate ();
	}

	public void OnTouchOut (object sender, ClickedEventArgs e)
	{
		if (!ToolsetEnable)
			return;
		
		isTouching = false;

		// snap to the closest tool
		if (currStickerTool != null)
		{
			currStickerTool.TurnToIdealAngle(transform.localEulerAngles.z);
		}
		// enable the function
	}

	public void OnTouching (object sender, ClickedEventArgs e)
	{
		if (!ToolsetEnable)
			return;

		if (inRotating)
			return;
		
		//steps on X-Axis: -1~1 break down into 10 steps, each step: 0.2f
		float dist = currTouchpadAxis.x - pastTouchpadAxis.x;
		float absDist = Mathf.Abs (dist);

		// Swiping
		if (absDist > 0.4f)
		{
			if(dist > 0) {
				toolIndexCount--;
			} else {
				toolIndexCount++;
			}

			if (toolIndexCount < 0)
				toolIndexCount = stickerTools.Count - 1;
			else if (toolIndexCount >= stickerTools.Count)
				toolIndexCount = 0;
			
			SnapToTargetAngleAction(toolIndexCount, 0.3f);
			inRotating = true;

			pastTouchpadAxis = currTouchpadAxis = GetTouchpadAxis ();
			DeviceVibrate ();
			Debug.Log ("swipe! : " + absDist);
		}
		// Wait until dist is accumulated to 0.1f
		else if (absDist > 0.1f && absDist < 0.2f) {
			if (dist > 0) {
				// swipe right
				transform.Rotate (-Vector3.forward * rotDegreePerStep);
			} else {
				// swipe left
				transform.Rotate (Vector3.forward * rotDegreePerStep);
			}
			pastTouchpadAxis = currTouchpadAxis = GetTouchpadAxis ();
			DeviceVibrate ();
//			Debug.Log ("rotate! : " + absDist);

			// Raycasting to detect which tool is showing up
			CheckRaycast();
		}
		// if not, then wait until it's accumulated to 0.2f
		else {
			currTouchpadAxis = GetTouchpadAxis ();
		}
	}

	public void OnClick (object sender, ClickedEventArgs e)
	{
		if (inRotating)
			return;

		if (!ToolsetEnable)
			return;
		
		Vector2 currTouchpadAxis = GetTouchpadAxis ();
		if(currTouchpadAxis.x > 0)
		{
			// rotate to left
			SnapToAngleAction(eachR, 0.3f);
		}
		else
		{
			// rotate to right
			SnapToAngleAction(-eachR, 0.3f);
		}
		inRotating = true;
		DeviceVibrate ();
	}

	//===========================================================================
	/// <summary>
	/// Touch Stop event of Touchpad on Macbook. For testing only.
	/// </summary>
	public void OnTouchStop ()
	{
		// snap to the closest tool
		if (currStickerTool != null)
		{
			currStickerTool.TurnToIdealAngle(transform.localEulerAngles.z);
		}

		// enable the function
	}

	/// <summary>
	/// Tool Swiping of Touchpad on Macbook. For testing only.
	/// </summary>
	/// <param name="currTouchValue">Curr touch value.</param>
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
					currToolIndex = toolIndexCount = s_t.ToolIndex;
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

	public void SnapToAngle(float angle, float time)
	{
		LeanTween.rotateAroundLocal ( gameObject, Vector3.forward, angle, time );
	}

	public void SnapToAngleAction(float angle, float time)
	{
		LeanTween.rotateAroundLocal ( gameObject, Vector3.forward, angle, time ).setOnComplete(CheckRaycast).setEaseInOutBack();
	}

	public void SnapToTargetAngleAction(int targetToolIndex, float time)
	{
		float c_angle = (360f + transform.localEulerAngles.z) % 360f;
		float t_angle = stickerTools [targetToolIndex].IdealAngle - c_angle;
//		Debug.Log("c_angle: " + c_angle + ", t_angle: " + t_angle);
		LeanTween.rotateAroundLocal ( gameObject, Vector3.forward, t_angle, time ).setOnComplete(CheckRaycast).setEaseInOutBack();
	}

	void CheckRaycast()
	{
		// Raycasting to detect which tool is showing up
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.parent.up, out hit, 10f, toolLayer))
		{
			StickerTool s_t = hit.collider.gameObject.GetComponent<StickerTool> ();
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
				currToolIndex = toolIndexCount = s_t.ToolIndex;
			}
		}
		inRotating = false;
	}

	public void DisableAllTools()
	{
		for(int i=0; i<stickerTools.Count; i++)
		{
			stickerTools [i].DisableTool ();
		}
		ToolsetEnable = false;
		currStickerTool = null;
		currToolIndex = toolIndexCount = -1;
	}

	public void EnableAllTools()
	{
		ToolsetEnable = true;
	}
}
