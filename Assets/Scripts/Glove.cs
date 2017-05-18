using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Glove : MonoBehaviour {

	public event Action<Collider> OnCollide;

	void OnTriggerEnter(Collider _col)
	{
		if (OnCollide != null)
			OnCollide (_col);
	}
}
