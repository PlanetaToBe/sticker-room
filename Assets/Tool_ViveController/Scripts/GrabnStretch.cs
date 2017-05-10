using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ViveSimpleController))]
public class GrabnStretch : MonoBehaviour {

	public Rigidbody wall;

	private ViveSimpleController viveController;

	private VRInteractiveObject m_CurrentInteractible;		// The current interactive object
	private SteamVR_TrackedObject trackedObj;
	private GameObject touchedObj;
	private GameObject grabbedObj; // for grabbed obj (needs rigidBody)
	private GameObject stretchObj;

	private bool grabSomething = false;

	// === Stretching
	private bool inStretchMode = false;
	private float initialControllersDistance;
	private Vector3 originalScale;

	private Vector3 m_TriggerClickPosition;
	private Vector3 m_TriggerDownPosition;
	private Vector3 m_TriggerUpPosition;
	private float m_LastUpTime;

	void OnEnable()
	{
		if (viveController == null)
		{
			viveController = GetComponent<ViveSimpleController> ();
		}

		viveController.OnHover += HandleOver;
		viveController.OnHoverLeave += HandleOut;

		viveController.OnTriggerDown += HandleTriggerDown;
		viveController.OnTriggerUp += HandleTriggerUp;
		viveController.OnTriggerTouch += HandleTriggerTouch;

		viveController.OnTouchpadDown += HandlePadDown;
		viveController.OnTouchpadUp += HandlePadUp;
	}

	void OnDisable()
	{
		viveController.OnHover -= HandleOver;
		viveController.OnHoverLeave -= HandleOut;

		viveController.OnTriggerDown -= HandleTriggerDown;
		viveController.OnTriggerUp -= HandleTriggerUp;
		viveController.OnTriggerTouch -= HandleTriggerTouch;

		viveController.OnTouchpadDown -= HandlePadDown;
		viveController.OnTouchpadUp -= HandlePadUp;
	}
	
	void Start ()
	{
		trackedObj = viveController.TrackedObj;
	}

	public void HandleOver(Collider collider)
	{
		// ignore if it's another controller
		if (collider.gameObject.tag == "GameController")
			return;

		// ignore if already grabbing something
		if (grabSomething)
			return;

		//if (inStretchMode)
			//return;

		// If we hit an interactive item
		if (collider.gameObject.GetComponent<VRInteractiveObject> ())
		{
			viveController.DeviceVibrate();

			touchedObj = collider.gameObject;
			m_CurrentInteractible = touchedObj.GetComponent<VRInteractiveObject> ();
			m_CurrentInteractible.StartTouching (gameObject);
		}
	}

	public void HandleOut(Collider collider)
	{
		// ignore if it's another controller
		if (collider.gameObject.tag == "GameController")
			return;

		if (touchedObj == null)
			return;

		if (inStretchMode)
			return;

		if (collider == touchedObj.GetComponent<Collider> ())
		{
			// due to the parenting(aka non-physics) method will trigger this event for some reason :/
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

	public void HandleTriggerDown(GameObject _obj)
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

				initialControllersDistance = (viveController.attachPoint.position - m_CurrentInteractible.GrabbedPos).sqrMagnitude;
				Debug.Log (gameObject.name + "starts stretching!");
				viveController.DeviceVibrate ();

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
				m_CurrentInteractible.grabbedPoint = viveController.attachPoint.position;

				// Grab in PHYSICS way if has rigidbody
				if (m_CurrentInteractible.HasRigidbody)
				{
					// set the kinematic false (if it is) so can be controlled by joint
					if (m_CurrentInteractible.IsKinematic)
					{
						touchedObj.GetComponent<Rigidbody> ().isKinematic = false;
					}
					// add fixed joint
					m_CurrentInteractible.AddJoint(viveController.attachPoint);
				}
				else // or NON-PHYSICS(hierarchy way)
				{
					touchedObj.transform.parent = gameObject.transform;
				}

				grabSomething = true;
				Debug.Log (gameObject.name + " grabs!");
				viveController.DeviceVibrate();
			}
		}
	}

	public void HandleTriggerUp(GameObject _obj)
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

	public void HandleTriggerTouch(GameObject _obj)
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
				if (stretchObj != null)
					ScaleAroundPoint (stretchObj); // only work on parenting ver. grabbing?
			}
		}
	}

	public void HandlePadDown(GameObject _obj)
	{
		if(m_CurrentInteractible)
			m_CurrentInteractible.PadDown (_obj);

		// SHOOT_OUT
		if (grabSomething)
		{
			ExitGrabMode (true);
		}

		if (inStretchMode)
		{
			ExitStretchMode ();
		}

		if(m_CurrentInteractible)
			m_CurrentInteractible.AddSpringJoint (wall);
	}

	public void HandlePadUp(GameObject _obj, SteamVR_Controller.Device _device)
	{

	}

	private void ScaleAroundPoint(GameObject target)
	{
		// compare current distance of two controllers, with the start distance, to stretch the object
		var pivot = m_CurrentInteractible.GrabbedPos;
		var mag = (viveController.attachPoint.position - pivot).sqrMagnitude - initialControllersDistance;
			
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
		viveController.DeviceVibrate();
		Debug.Log ("m_CurrentInteractible.IsGrabbing: " + m_CurrentInteractible.IsGrabbing + ", " + gameObject.name + " exit Stretch Mode");
	}

	private void ExitGrabMode(bool destroyImmediate)
	{
		Debug.Log (gameObject.name + " exits grab!");

		m_CurrentInteractible.Up (gameObject);

		if (m_CurrentInteractible.HasRigidbody)
		{
			// remote joint!
			if(m_CurrentInteractible.Joint!=null)
			{
				var device = viveController.Device;
				var rigidbody = m_CurrentInteractible.Joint.GetComponent<Rigidbody> ();

				// destroy the fixed joint
				m_CurrentInteractible.RemoveJoint ();

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
		viveController.DeviceVibrate();
	}
}
