using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Remover : MonoBehaviour {

	public event Action<Collider> OnCollide;
	public event Action<Collider> OnCollideExit;
//	public event Action<Collision> OnCollision;

	void OnTriggerEnter(Collider _col)
	{
		if (OnCollide != null)
			OnCollide (_col);
	}

	void OnTriggerExit(Collider _col)
	{
		if (OnCollideExit != null)
			OnCollideExit (_col);
	}

//	void OnCollisionEnter(Collision _coli)
//	{
//		if (OnCollision != null)
//			OnCollision (_coli);
//	}
}
