using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Remover : MonoBehaviour {

	public event Action<Collider> OnCollide;

	void OnTriggerEnter(Collider _col)
	{
		if (OnCollide != null)
			OnCollide (_col);
	}

}
