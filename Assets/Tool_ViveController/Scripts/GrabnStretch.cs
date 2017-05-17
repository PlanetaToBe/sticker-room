using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedController))]
public class GrabnStretch : MonoBehaviour {

	public Rigidbody wall;
	public Rigidbody attachPoint;
	public Transform cameraEye;

	private SteamVR_TrackedController controller;

	//--- Grab n Stretch ---
	//----------------------
	private VRInteractiveObject m_CurrentInteractible;		// The current interactive object
	private GameObject touchedObj;
	private GameObject grabbedObj; // for grabbed obj (needs rigidBody)
	private GameObject stretchObj;

	private bool grabSomething = false;
	private bool inStretchMode = false;
	private float initialControllersDistance;
	private Vector3 originalScale;
	//----------------------

	private Vector3 m_TriggerClickPosition;
	private Vector3 m_TriggerDownPosition;
	private Vector3 m_TriggerUpPosition;
	private float m_LastUpTime;

	//--- Scale Self ---
	//----------------------
	// SCALE_CameraRig_Parent => Player
	[Header("Self Scaling")]
	public float minSelfScale = 0.05f;
	public float maxSelfScale = 20f;
	private bool m_inSelfScalingMode = false;
	private bool m_inSelfScalingSupportMode = false;
	private GrabnStretch otherController;
	private Transform player;
	private float scaleWaitTime = 1f;
	private float firstTouchTime;

	public float PlayerScale
	{
		get { return player.localScale.x; }
	}

	public bool InSelfScalingSupportMode
	{
		get { return m_inSelfScalingSupportMode; }
	}

	public bool InSelfScalingMode
	{
		get { return m_inSelfScalingMode; }
	}
	//----------------------

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

		controller.PadClicked += HandlePadDown;
		controller.PadUnclicked += HandlePadUp;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= HandleTriggerDown;
		controller.TriggerUnclicked -= HandleTriggerUp;
		controller.TriggerDowning -= HandleTriggerTouch;

		controller.PadClicked -= HandlePadDown;
		controller.PadUnclicked -= HandlePadUp;
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

	public void HandleOver(Collider _collider)
	{
		// ignore if it's another controller
		if (_collider.gameObject.tag == "GameController")
		{
			otherController = _collider.gameObject.GetComponent<GrabnStretch> ();
			if(otherController.InSelfScalingMode)
			{
				m_inSelfScalingSupportMode = true;
				m_inSelfScalingMode = false;
			}
			else
			{
				m_inSelfScalingSupportMode = false;
				m_inSelfScalingMode = true;
			}
				
			if(m_inSelfScalingMode)
			{
				originalScale = player.localScale;
				initialControllersDistance = (attachPoint.position - otherController.attachPoint.position).sqrMagnitude;
				firstTouchTime = Time.time;
			}

			DeviceVibrate ();
			return;
		}

		// ignore if in self-stretching mode
		if (m_inSelfScalingMode || m_inSelfScalingSupportMode)
			return;

		// ignore if already grabbing something
		if (grabSomething)
			return;

		//if (inStretchMode)
			//return;

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
		// ignore if it's another controller
		if (_collider.gameObject.tag == "GameController")
		{
			if(m_inSelfScalingMode || m_inSelfScalingSupportMode)
			{
				m_inSelfScalingMode = false;
				m_inSelfScalingSupportMode = false;
				otherController = null;
				firstTouchTime = 0f;
			}
			return;
		}
			

		if (touchedObj == null)
			return;

		if (inStretchMode)
			return;

		if (_collider == touchedObj.GetComponent<Collider> ())
		{
			// checking cuz the parenting(aka non-physics) method will trigger this event for some reason :/
			if (!m_CurrentInteractible.HasRigidbody && grabSomething)
				return;

			// what if it's still grabbing?
			if (grabSomething)
				ExitGrabMode (false);

			//Debug.Log (gameObject.name + " exit touch " + collider.name);
			m_CurrentInteractible.StopTouching (gameObject);
			m_CurrentInteractible = null;
			touchedObj = null;
		}
	}

	public void HandleStay(Collider _collider)
	{
		if (_collider.gameObject.tag == "GameController")
		{
			// just in case
			if (!m_inSelfScalingMode && !m_inSelfScalingSupportMode)
			{
				otherController = _collider.gameObject.GetComponent<GrabnStretch> ();
				if(otherController.InSelfScalingMode)
				{
					m_inSelfScalingSupportMode = true;
				}
				else
				{
					m_inSelfScalingMode = true;
					firstTouchTime = Time.time;
				}
			}

			float threshold = firstTouchTime + scaleWaitTime;
			if( m_inSelfScalingMode && otherController.InSelfScalingSupportMode && (Time.time > threshold) )
			{
				ScaleSelf (player);
			}
		}
	}

	public void HandleTriggerDown(object sender, ClickedEventArgs e)
	{
//		if(m_inSelfScalingMode && otherController.InSelfScalingSupportMode)
//		{
//			originalScale = player.localScale;
//			initialControllersDistance = (attachPoint.position - otherController.attachPoint.position).sqrMagnitude;
//			return;
//		}

		if (touchedObj == null)
		{
			return;			// not possible but just double check
		}

		// if haven't grab anything && not in stretch mode
		if(!grabSomething && !inStretchMode)
		{
			// if already been grabbed
			if (m_CurrentInteractible.IsGrabbing)
			{
				// enter stretch mode!
				inStretchMode = true;
				stretchObj = touchedObj;
				originalScale = stretchObj.transform.localScale;

				initialControllersDistance = (attachPoint.position - m_CurrentInteractible.GrabbedPos).sqrMagnitude;
				//Debug.Log (gameObject.name + "starts stretching!");
				DeviceVibrate ();

				// remote the joint attached to other controller
				if(m_CurrentInteractible.Joint)
				{
					stretchObj.GetComponent<Rigidbody> ().isKinematic = true;
					m_CurrentInteractible.RemoveJoint ();
				}
			}
			else
			{
				// be grabbed!
				m_CurrentInteractible.Down(gameObject);
				m_CurrentInteractible.grabbedPoint = attachPoint.position;

				// Grab in PHYSICS way if has rigidbody
				if (m_CurrentInteractible.HasRigidbody)
				{
					// set the kinematic false (if it is) so can be controlled by joint
					if (m_CurrentInteractible.IsKinematic)
					{
						touchedObj.GetComponent<Rigidbody> ().isKinematic = false;
					}
					// add fixed joint
					m_CurrentInteractible.AddJoint(attachPoint);
				}
				else // or NON-PHYSICS(hierarchy way)
				{
					touchedObj.transform.parent = gameObject.transform;
				}

				grabSomething = true;
				//Debug.Log (gameObject.name + " grabs!");
				DeviceVibrate();
			}
		}
	}

	public void HandleTriggerUp(object sender, ClickedEventArgs e)
	{
//		if(m_inSelfScalingMode || m_inSelfScalingSupportMode)
//		{
//			m_inSelfScalingMode = false;
//			m_inSelfScalingSupportMode = false;
//			otherController = null;
//		}

		if (grabSomething)
		{
			ExitGrabMode (true);	
		}

		if (inStretchMode)
		{
			ExitStretchMode ();
		}
	}

	public void HandleTriggerTouch(object sender, ClickedEventArgs e)
	{
//		if(m_inSelfScalingMode && otherController.InSelfScalingSupportMode)
//		{
//			ScaleSelf (player);
//		}

		if(m_CurrentInteractible)
			m_CurrentInteractible.Touch(gameObject);

		if (inStretchMode)
		{
			// check if the object is still be grabbed
			if(!m_CurrentInteractible.IsGrabbing)
			{
				//if not, exit stretch mode
				ExitStretchMode();
			}
			else
			{
				if (stretchObj != null)
					ScaleAroundPoint (stretchObj);
			}
		}
	}

	public void HandlePadDown(object sender, ClickedEventArgs e)
	{
		if(m_CurrentInteractible)
			m_CurrentInteractible.PadDown (gameObject);

		// SHOOT_OUT
		if (grabSomething)
		{
			ExitGrabMode (true);
		}

		if (inStretchMode)
		{
			ExitStretchMode ();
		}

		if(m_CurrentInteractible && wall != null)
			m_CurrentInteractible.AddSpringJoint (wall);
	}

	public void HandlePadUp(object sender, ClickedEventArgs e)
	{

	}

	private void ScaleSelf(Transform target)
	{
		// v.1
		/*
		// compare current distance of two controllers, with the start distance, to stretch the object
		var pivot = cameraEye.transform.position;
		pivot.y = target.transform.position.y; // set pivot to be on the floor
		var mag = (attachPoint.position - otherController.attachPoint.position).sqrMagnitude - initialControllersDistance;			
		var endScale = target.transform.localScale * (1f + mag*0.01f);

		// diff from obj pivot to desired pivot
		var diffP = target.transform.position - pivot;
		var finalPos = (diffP * (1f + mag*0.01f)) + pivot;

		target.transform.localScale = endScale;
		target.transform.position = finalPos;
		*/

		float scaleFactor;
		if(transform.position.y > cameraEye.position.y)
		{
			// scale up
			scaleFactor = 1f + 0.01f;
		}
		else {
			// scale down
			scaleFactor = 1f - 0.01f;
		}
		var endScale = target.transform.localScale * scaleFactor;
		var idealScaleValue = Mathf.Clamp (endScale.x, minSelfScale, maxSelfScale);
		endScale.Set (idealScaleValue, idealScaleValue, idealScaleValue);

		if (Mathf.Approximately (target.transform.localScale.x, endScale.x))
			return;

		var pivot = cameraEye.transform.position;
		pivot.y = target.transform.position.y; // set pivot to be on the floor

		var diffP = target.transform.position - pivot;
		var finalPos = (diffP * scaleFactor) + pivot;

		target.transform.localScale = endScale;
		target.transform.position = finalPos;

		DeviceVibrate ();
	}

	private void ScaleAroundPoint(GameObject target)
	{
		// compare current distance of two controllers, with the start distance, to stretch the object
		var pivot = m_CurrentInteractible.GrabbedPos;
		var mag = (attachPoint.position - pivot).sqrMagnitude - initialControllersDistance;			
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
		DeviceVibrate();
		//Debug.Log ("m_CurrentInteractible.IsGrabbing: " + m_CurrentInteractible.IsGrabbing + ", " + gameObject.name + " exit Stretch Mode");
	}

	private void ExitGrabMode(bool destroyImmediate)
	{
		//Debug.Log (gameObject.name + " exits grab!");

		m_CurrentInteractible.Up (gameObject);

		if (m_CurrentInteractible.HasRigidbody)
		{
			// remote joint!
			if(m_CurrentInteractible.Joint!=null)
			{
				var device = Device;
				var rigidbody = m_CurrentInteractible.Joint.GetComponent<Rigidbody> ();

				// destroy the fixed joint
				m_CurrentInteractible.RemoveJoint ();

				// Apply force
//				var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
//				if (origin != null)
//				{
//					// of grabbed obj
//					rigidbody.velocity = origin.TransformVector (device.velocity); //transform vector from local to world space
//					rigidbody.angularVelocity = origin.TransformVector (device.angularVelocity);
//				}
//				else
//				{
					rigidbody.velocity = device.velocity;
					rigidbody.angularVelocity = device.angularVelocity;
//				}
				rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
			}
				
			// Reset kinematic status
			if (m_CurrentInteractible.IsKinematic) {
				touchedObj.GetComponent<Rigidbody> ().isKinematic = true;
			}
		}
		else
		{
			touchedObj.transform.parent = null;
		}

		//m_CurrentInteractible.Up (gameObject);
		grabSomething = false;
		DeviceVibrate();
	}

	public void DeviceVibrate()
	{
		Device.TriggerHapticPulse (1000);
	}
}
