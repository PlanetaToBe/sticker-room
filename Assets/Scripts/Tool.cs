using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tool : MonoBehaviour {

	public virtual event Action<Collider> OnCollide;

	void OnTriggerEnter(Collider _col)
	{
		//Debug.Log ( gameObject.name + " collide!");

		if (OnCollide != null)
			OnCollide (_col);
	}
}
