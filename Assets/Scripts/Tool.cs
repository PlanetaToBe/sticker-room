using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tool : MonoBehaviour {

	public event Action<Collider> OnCollideEnter;
	public event Action<Collider> OnCollideExit;
	public event Action<Collider> OnCollideStay;

	private Animator animator;

	void Start()
	{
		animator = GetComponent<Animator> ();
		OnStart ();
	}

	public virtual void OnStart()
	{
		//
	}

	private void OnTriggerEnter(Collider _collider)
	{
		if (OnCollideEnter!=null)
			OnCollideEnter (_collider);
	}

	private void OnTriggerExit(Collider _collider)
	{
		if (OnCollideExit!=null)
			OnCollideExit (_collider);
	}

	private void OnTriggerStay(Collider _collider)
	{
		if (OnCollideStay!=null)
			OnCollideStay (_collider);
	}

	public void OnDown()
	{
		animator.SetTrigger ("Down");
	}

	public void OnUp()
	{
		animator.SetTrigger ("Up");
	}
}
