using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedController))]
public class Pump : MonoBehaviour {

	public Transform pointyPoint;
	private SteamVR_TrackedController controller;

	//--- Grab n Stretch ---
	//----------------------
	private VRInteractiveObject m_CurrentInteractible;		// The current interactive object
	private GameObject touchedObj;
	private GameObject stretchObj;

	private bool inStretchMode = false;
	private float initialControllersDistance;
	private Vector3 originalScale;
	//----------------------

	private Vector3 m_TriggerClickPosition;
	private Vector3 m_TriggerDownPosition;
	private Vector3 m_TriggerUpPosition;
	private float m_LastUpTime;

	public StickerTool myTool;
	private bool inUse;

	//--- Player Scale ---
	private Transform player;

	public float PlayerScale
	{
		get { return player.localScale.x; }
	}
		
	public SteamVR_Controller.Device Device
	{
		get { return SteamVR_Controller.Input ((int)controller.controllerIndex); }
	}

	void OnEnable()
	{
		if(controller==null)
		{
			controller = GetComponent<SteamVR_TrackedController>();
		}
			
		controller.TriggerClicked += HandleTriggerDown;
		controller.TriggerUnclicked += HandleTriggerUp;
		controller.TriggerDowning += HandleTriggerTouch;

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= HandleTriggerDown;
		controller.TriggerUnclicked -= HandleTriggerUp;
		controller.TriggerDowning -= HandleTriggerTouch;

		if(myTool!=null)
			myTool.OnChangeToolStatus -= OnToolStatusChange;
	}
	
	void Start ()
	{
		player = transform.parent.parent;
	}

	private void OnTriggerEnter(Collider _collider)
	{
		HandleOver (_collider);
	}

	private void OnTriggerExit(Collider _collider)
	{
		HandleOut (_collider);
	}

	private void OnTriggerStay(Collider _collider)
	{
		HandleStay (_collider);
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;

		if (!inUse)
			Reset ();
	}

	public void HandleOver(Collider _collider)
	{
		if (_collider.gameObject.tag == "GameController")
			return;

		if (!inUse)
			return;

		if (inStretchMode)
			return;

		// If we hit an interactive item
		if (_collider.gameObject.GetComponent<VRInteractiveObject> ())
		{
			DeviceVibrate();

			touchedObj = _collider.gameObject;
			m_CurrentInteractible = touchedObj.GetComponent<VRInteractiveObject> ();
			m_CurrentInteractible.StartTouching (gameObject);
		}
	}

	public void HandleOut(Collider _collider)
	{
		if (_collider.gameObject.tag == "GameController")
			return;

		if (!inUse)
			return;

		if (touchedObj == null)
			return;

		if (inStretchMode)
			return;
		
		if (_collider == touchedObj.GetComponent<Collider> ())
		{
			m_CurrentInteractible.StopTouching (gameObject);
			m_CurrentInteractible = null;
			touchedObj = null;
		}
	}

	public void HandleStay(Collider _collider)
	{
		//
	}

	public void HandleTriggerDown(object sender, ClickedEventArgs e)
	{
		if (touchedObj == null || !inUse)
			return;			// not possible but just double check

		// if not in stretch mode
		if(!inStretchMode)
		{
			// enter stretch mode!
			inStretchMode = true;
			stretchObj = touchedObj;
			originalScale = stretchObj.transform.localScale;

			// if thing is currently been grabbed
			if (m_CurrentInteractible.IsGrabbing)
			{
				initialControllersDistance = (pointyPoint.position - m_CurrentInteractible.GrabbedPos).sqrMagnitude;

				// remote the joint attached to other controller
				if(m_CurrentInteractible.Joint)
				{
					stretchObj.GetComponent<Rigidbody> ().isKinematic = true;
					m_CurrentInteractible.RemoveJoint ();
				}
			}
		}
		DeviceVibrate ();
	}

	public void HandleTriggerUp(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;

		if (inStretchMode)
		{
			ExitStretchMode ();
		}
	}

	public void HandleTriggerTouch(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;

		if(m_CurrentInteractible)
			m_CurrentInteractible.Touch(gameObject);

		if (inStretchMode)
		{
			// TODO: what if the object stop being be grabbed???
			// if(!m_CurrentInteractible.IsGrabbing)

			if (stretchObj != null)
					ScaleAroundPoint (stretchObj);
		}
	}
		
	private void ScaleAroundPoint(GameObject target)
	{		
		Vector3 pivot;
		if (m_CurrentInteractible.IsGrabbing)
		{
			// compare current distance of two controllers, with the start distance, to stretch the object
			pivot = m_CurrentInteractible.GrabbedPos;
		}
		else
		{
			pivot = pointyPoint.position;
		}
		//var mag = (pointyPoint.position - pivot).sqrMagnitude - initialControllersDistance;
		var mag = 0.1f;
		var endScale = target.transform.localScale * (1f + mag*0.1f);

		// diff from obj pivot to desired pivot
		var diffP = target.transform.position - pivot;
		var finalPos = (diffP * (1f + mag*0.1f)) + pivot;

		target.transform.localScale = endScale;
		target.transform.position = finalPos;
	}

	private void ExitStretchMode()
	{
		// if it's physics object 
		if(m_CurrentInteractible.HasRigidbody)
		{
			// and be grabbed
			if(m_CurrentInteractible.IsGrabbing)
			{
				m_CurrentInteractible.AddJoint (m_CurrentInteractible.Grabber);
				stretchObj.GetComponent<Rigidbody> ().isKinematic = false;
			}

			if(!m_CurrentInteractible.IsKinematic)
			{
				stretchObj.GetComponent<Rigidbody> ().isKinematic = false;
			}
		}
						
		inStretchMode = false;
		stretchObj = null;
	}

	private void Reset()
	{
		if (inStretchMode)
			ExitStretchMode ();

		if(m_CurrentInteractible)
			m_CurrentInteractible.StopTouching (gameObject);
		
		m_CurrentInteractible = null;
		touchedObj = null;
	}

	public void DeviceVibrate()
	{
		Device.TriggerHapticPulse (1000);
	}
}
