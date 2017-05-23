using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour {

	//public GameObject glove;
	public Remover removerScript;
	public StickerTool myTool;

	private SteamVR_TrackedController controller;
	private bool showGlove = false;
	private bool inUse;
	private GameObject objectToRemove;
	private bool triggerIsDown = false;
	private VRInteractiveObject m_CurrentInteractible;

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
		controller.TriggerClicked += HandleDown;
		controller.TriggerUnclicked += HandleUp;
		//removerScript.OnCollide += OnRemoverCollide;

		removerScript.OnCollide += OnRemoverTouch;
		removerScript.OnCollideExit += OnRemoverTouchExit;

		if(myTool!=null)
			myTool.OnChangeToolStatus += OnToolStatusChange;
	}

	void OnDisable()
	{
		controller.TriggerClicked -= HandleDown;
		controller.TriggerUnclicked -= HandleUp;
		//removerScript.OnCollide -= OnRemoverCollide;

		removerScript.OnCollide -= OnRemoverTouch;
		removerScript.OnCollideExit -= OnRemoverTouchExit;

		if(myTool!=null)
			myTool.OnChangeToolStatus -= OnToolStatusChange;
	}

	private void OnToolStatusChange(bool _inUse, int toolIndex)
	{
		inUse = _inUse;
	}

	public void HandleDown(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
//		showGlove = true;
//		removerScript.gameObject.SetActive (true);

		triggerIsDown = true;
	}

	public void HandleUp(object sender, ClickedEventArgs e)
	{
		if (!inUse)
			return;
		
//		showGlove = false;
//		removerScript.gameObject.SetActive (false);
		objectToRemove = null;
		triggerIsDown = false;
	}

	public void OnRemoverCollide(Collider _col)
	{
		if (!inUse)
			return;

		if(_col.gameObject.tag == "Sticker")
		{
			//Destroy (_col.gameObject);
			objectToRemove = _col.gameObject;
		}
	}

	public void OnRemoverTouchExit(Collider _col)
	{
		if (!inUse)
			return;

		if (_col.gameObject == objectToRemove)
		{
			objectToRemove = null;
		}
	}

	public void OnRemoverTouch(Collider _col)
	{
		if (!inUse)
			return;

		if(_col.gameObject.tag == "Sticker" && triggerIsDown)
		{
			objectToRemove = _col.gameObject;
			m_CurrentInteractible = objectToRemove.GetComponent<VRInteractiveObject> ();

			if(!m_CurrentInteractible.IsHammered)
			{
				// if no rigidbody, add one
				if (m_CurrentInteractible.TheRigidbody==null)
				{
					objectToRemove.AddComponent<Rigidbody> ();
				}
				//m_CurrentInteractible.PrepColliderForHammer ();

				// add forward force
				m_CurrentInteractible.TheRigidbody.velocity = Device.velocity * 2;
				m_CurrentInteractible.TheRigidbody.angularVelocity = Device.angularVelocity * 2;

				// the object should self-destroy once hit something else
				m_CurrentInteractible.IsHammered = true;

				DeviceVibrate ();
			}				
		}
	}

	public void DeviceVibrate()
	{
		Device.TriggerHapticPulse (1000);
	}
}
