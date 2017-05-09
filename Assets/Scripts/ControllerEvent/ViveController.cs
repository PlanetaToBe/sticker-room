/// <summary>
/// Vive controller, reference: Unity's VREyeRaycaster.cs
/// In order to interact with
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class ViveController : MonoBehaviour {
	[Tooltip("The attach point for grabbing.")]
	public Rigidbody attachPoint;

	public event Action<GameObject> OnHover;
	public event Action<GameObject> OnHoverLeave;

	public event Action<GameObject> OnTriggerClick;
	public event Action<GameObject> OnTriggerDown;
	public event Action<GameObject> OnTriggerUp;
	public event Action<GameObject> OnTriggerTouch;

	public event Action<GameObject> OnTouchpadDown;
	public event Action<GameObject, SteamVR_Controller.Device> OnTouchpadUp;

	private VRInteractiveObject m_CurrentInteractible;		// The current interactive object
	//private VRInteractiveObject m_LastInteractible;			// The last interactive item

	private SteamVR_TrackedObject trackedObj;
	private GameObject touchedObj;
	private GameObject grabbedObj; // for grabbed obj (needs rigidBody)
	private GameObject stretchObj;

	private bool objHasRigidbody = false;
	private bool objIsKinematic = false;
	private FixedJoint joint; // for grabbed obj
	private bool grabSomething = false;

	// === Stretching
	private bool inStretchMode = false;
	private float initialControllersDistance;
	private Vector3 originalScale;

	private Vector3 m_TriggerClickPosition;
	private Vector3 m_TriggerDownPosition;
	private Vector3 m_TriggerUpPosition;
	private float m_LastUpTime;

	// Utility for other classes to get the current interactive object
	public VRInteractiveObject CurrentInteractible
	{
		get { return m_CurrentInteractible; }
	}

	// Reference: vrinput.OnClick += reticle's HandleClick
	// Not sure if it's a good idea to subscribe self's event? :/
	private void OnEnable()
	{
		OnTriggerClick += HandleClick;
		OnTriggerDown += HandleDown;
		OnTriggerUp += HandleUp;
		OnTriggerTouch += HandleTouch;

		OnTouchpadDown += HandlePadDown;
		OnTouchpadUp += HandlePadUp;
	}

	private void OnDisable()
	{
		OnTriggerClick -= HandleClick;
		OnTriggerDown -= HandleDown;
		OnTriggerUp -= HandleUp;
		OnTriggerTouch -= HandleTouch;

		OnTouchpadDown -= HandlePadDown;
		OnTouchpadUp -= HandlePadUp;
	}

	private void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject> ();
	}

	private void Update()
	{
		CheckInput ();
	}

	private void OnTriggerEnter(Collider collider)
	{
		// ignore if it's another controller
		if (collider.gameObject.tag == "GameController")
			return;

		// ignore if already grabbing something
		if (grabSomething)
			return;

		// If we hit an interactive item
		if (collider.gameObject.GetComponent<VRInteractiveObject> ())
		{
			DeviceVibrate();
			//Debug.Log (gameObject.name + " touch " + collider.name);

			touchedObj = collider.gameObject;
			m_CurrentInteractible = touchedObj.GetComponent<VRInteractiveObject> ();
			m_CurrentInteractible.StartTouching (gameObject);

			if (touchedObj.GetComponent<Rigidbody> ())
			{
				objHasRigidbody = true;

				if (touchedObj.GetComponent<Rigidbody> ().isKinematic)
					objIsKinematic = true;
				else
					objIsKinematic = false;
			}
			else
			{
				objHasRigidbody = false;
			}

			if (OnHover != null)
				OnHover (collider.gameObject);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		// ignore if it's another controller
		if (collider.gameObject.tag == "GameController")
			return;

		if (touchedObj == null)
			return;

		// what if it's still in stretching mode? => exit stretch mode
		/* -->Nah. DISABLE for better experience
		if (inStretchMode && stretchObj == collider.gameObject)
			ExitStretchMode();
		*/
		if (inStretchMode)
			return;

		if (collider == touchedObj.GetComponent<Collider> ())
		{
			// due to the parenting(aka non-physics) method will trigger this event for some reason :/
			if (!objHasRigidbody && grabSomething)
				return;

			if (OnHoverLeave != null)
				OnHoverLeave (collider.gameObject);
			
			// what if it's still grabbing?
			if (grabSomething)
				ExitGrabMode (false);

			//Debug.Log (gameObject.name + " exit touch " + collider.name);
			m_CurrentInteractible.StopTouching (gameObject);
			m_CurrentInteractible = null;
			touchedObj = null;
		}
	}

	private void CheckInput()
	{
		var device = SteamVR_Controller.Input ((int)trackedObj.index);

		if (device.GetTouchDown (SteamVR_Controller.ButtonMask.Trigger))
		{
			//Debug.Log ("GetTouchDown");
			m_TriggerDownPosition = attachPoint.position;

			if (OnTriggerDown != null)
				OnTriggerDown (gameObject);
		}

		if(device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
		{
			m_TriggerUpPosition = attachPoint.position;

			// TODO: SWIPE!

			if (OnTriggerUp != null)
				OnTriggerUp (gameObject);
		}
			
		if(device.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
		{
			// TODO: SWIPE!

			if (OnTriggerTouch != null)
				OnTriggerTouch (gameObject);
		}

		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			//Debug.Log ("GetPressDown");
			m_TriggerClickPosition = attachPoint.position;

			if (OnTriggerClick != null)
				OnTriggerClick (gameObject);
		}

		if(device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
		{
			//Debug.Log ("GetTouchDown");
			/*
			if (OnTouchpadDown != null)
				OnTouchpadDown (gameObject);*/
		}

		if(device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
		{
			//Debug.Log ("GetPressDown");

			if (OnTouchpadDown != null)
				OnTouchpadDown (gameObject);
		}

		if(device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
		{
			//Debug.Log ("GetPressDown");

			if (OnTouchpadUp != null)
				OnTouchpadUp (gameObject, device);
		}
	}

	#region Actions Handlers
	private void HandlePadDown (GameObject _object)
	{
		if(m_CurrentInteractible)
			m_CurrentInteractible.PadDown (_object);
	}

	private void HandleClick(GameObject clickObject)
	{

	}

	private void HandleDown(GameObject downObject)
	{	
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

				initialControllersDistance = (attachPoint.position - m_CurrentInteractible.CurrentGrabbedPoint ()).sqrMagnitude;
				Debug.Log (gameObject.name + "starts stretching!");
				DeviceVibrate ();
			}
			else
			{
				// be grabbed!
				m_CurrentInteractible.Down(gameObject);
				m_CurrentInteractible.grabbedPoint = attachPoint.position;

				// Grab in PHYSICS way if has rigidbody
				if (objHasRigidbody)
				{
					// disable the kinematic (if it is) so can be controlled by joint
					if (objIsKinematic)
					{
						touchedObj.GetComponent<Rigidbody> ().isKinematic = false;
					}
					// add fixed joint
					joint = touchedObj.AddComponent<FixedJoint> ();
					joint.connectedBody = attachPoint;
				}
				else // or NON-PHYSICS(hierarchy way)
				{
					touchedObj.transform.parent = gameObject.transform;
				}

				grabSomething = true;
				Debug.Log (gameObject.name + " grabs!");
				DeviceVibrate();
			}
		}
	}

	private void HandleTouch(GameObject touchObject)
	{
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
				// compare current distance of two controllers, with the start distance, to stretch the object
				var currentGrab = m_CurrentInteractible.CurrentGrabbedPoint ();
				var currentDist = (attachPoint.position - currentGrab).sqrMagnitude;
				var mag = currentDist - initialControllersDistance;

				if (stretchObj != null)
				{
					ScaleAroundPoint (stretchObj, currentGrab, mag); // only work on parenting ver. grabbing
				}
			}
		}
	}

	private void HandleUp(GameObject upObject)
	{
		if (grabSomething)
		{
			ExitGrabMode (true);	
		}

		if (inStretchMode)
		{
			ExitStretchMode ();
		}
	}

	private void HandlePadUp(GameObject upObject, SteamVR_Controller.Device device)
	{
		//
	}
	#endregion

	private void ScaleBasedOnDistance(GameObject target, float mag)
	{
		var endScale = target.transform.localScale * (1f + mag);
		target.transform.localScale = endScale;
	}

	private void ScaleAroundPoint(GameObject target, Vector3 pivot, float mag)
	{
		//var endScale = originalScale * (1f + mag);
		var endScale = target.transform.localScale * (1f + mag*0.1f);

		// diff from obj pivot to desired pivot
		var diffP = target.transform.position - pivot;
		var finalPos = (diffP * (1f + mag*0.1f)) + pivot;

		target.transform.localScale = endScale;
		target.transform.position = finalPos;
	}

	private void ExitStretchMode()
	{
		inStretchMode = false;
		stretchObj = null;
		DeviceVibrate();
		Debug.Log (gameObject.name + " exit Stretch Mode");
	}

	private void ExitGrabMode(bool destroyImmediate)
	{
		m_CurrentInteractible.Up (gameObject);

		if (objHasRigidbody)
		{
			var device = SteamVR_Controller.Input ((int)trackedObj.index);
			var obj = joint.gameObject;
			var rigidbody = obj.GetComponent<Rigidbody> ();

			// destroy the fixed joint
			if (destroyImmediate)
			{
				UnityEngine.Object.DestroyImmediate (joint);
			}
			else
			{
				UnityEngine.Object.Destroy (joint);
			}
			joint = null;

			// Apply force
			var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
			if (origin != null)
			{
				// of grabbed obj
				rigidbody.velocity = origin.TransformVector (device.velocity); //transform vector from local to world space
				rigidbody.angularVelocity = origin.TransformVector (device.angularVelocity);
			}
			else
			{
				rigidbody.velocity = device.velocity;
				rigidbody.angularVelocity = device.angularVelocity;
			}
			rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;

			// Reset kinematic status
			if (objIsKinematic) {
				touchedObj.GetComponent<Rigidbody> ().isKinematic = true;
			}
		}
		else
		{
			touchedObj.transform.parent = null;
		}

		grabSomething = false;
		DeviceVibrate();
		Debug.Log (gameObject.name + " exits grab!");
	}

	private void DeviceVibrate()
	{
		var device = SteamVR_Controller.Input ((int)trackedObj.index);
		device.TriggerHapticPulse (1000);
	}
}
