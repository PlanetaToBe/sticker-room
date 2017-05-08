/// <summary>
/// Vive interactive object, referece: Unity's VR
/// Should be added to gameObject in the scene
/// It contains events that can be subscribed to by classes that
/// need to know about input specifics to this gameobject
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInteractiveObject : MonoBehaviour {

	public event Action<GameObject> OnOver;
	public event Action<GameObject> OnOut;
	public event Action<GameObject> OnClick;
	public event Action<GameObject> OnDown;
	public event Action<GameObject> OnUp;
	public event Action<GameObject> OnTouch;
	public event Action<GameObject> OnPadDown;

	public bool usePhysics = false;
	public GameObject scaleTarget;
	[HideInInspector]
	public GameObject theThingGrabMe = null;
	[HideInInspector]
	public Vector3 grabbedPoint;

	private Rigidbody rigidbody;
	private List<GameObject> touchingObjects = new List<GameObject>();

	protected bool m_IsGrabbing = false;
	protected bool m_IsTouching = false;

	public bool IsTouching
	{
		get { return m_IsTouching; }		// Is the controller currently over this object?
	}

	public bool IsGrabbing
	{
		get { return m_IsGrabbing; }
	}
		
	private void Awake() {
		rigidbody = GetComponent<Rigidbody> ();

		if (usePhysics && rigidbody==null) {
			// Get the material
			PhysicMaterial artPhyMat = GameObject.Instantiate(
				Resources.Load("Materials/artPhyMat", typeof(PhysicMaterial)) as PhysicMaterial
			) as PhysicMaterial;
			rigidbody = gameObject.AddComponent<Rigidbody> ();
			rigidbody.mass = 2f;
			rigidbody.drag = 0.01f;
			rigidbody.angularDrag = 0.05f;
			GetComponent<Collider> ().material = artPhyMat;
		}
	}

	#region Controller Events
	// Functions are called by the controller when the physical input is detected
	// Functions in turn call the appropriate events should they have subscribers
	public void StartTouching(GameObject touchingObj)
	{
		if(!touchingObjects.Contains(touchingObj))
		{
			touchingObjects.Add (touchingObj);
		}

		m_IsTouching = true;

		if (OnOver != null)
			OnOver (touchingObj);
	}

	public void StopTouching(GameObject touchingObj)
	{
		if(touchingObjects.Contains(touchingObj))
		{
			touchingObjects.Remove (touchingObj);
		}

		if (touchingObjects.Count == 0)
		{
			m_IsTouching = false;
		}

		if (OnOut != null)
			OnOut (touchingObj);
	}

	public void Click(GameObject grabbingObj)
	{
		if (OnClick != null)
			OnClick (grabbingObj);
	}

	public void Down(GameObject grabbingObj)
	{
		if (m_IsGrabbing)
			return;

		theThingGrabMe = grabbingObj;
		m_IsGrabbing = true;

		if (OnDown != null)
			OnDown (grabbingObj);
	}

	public void Up(GameObject grabbingObj)
	{
		theThingGrabMe = null;
		m_IsGrabbing = false;

		if (OnUp != null)
			OnUp (grabbingObj);
	}

	public void Touch(GameObject touchingObj)
	{
		if (OnTouch != null)
			OnTouch (touchingObj);
	}

	public void PadDown (GameObject touchingObj)
	{
		if (OnPadDown != null)
			OnPadDown (touchingObj);
	}
	#endregion

	public Vector3 CurrentGrabbedPoint()
	{
		return theThingGrabMe.GetComponent<ViveController> ().attachPoint.position;
	}
}
