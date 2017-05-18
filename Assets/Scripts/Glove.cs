using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Glove : MonoBehaviour {

	public event Action<Collision> OnCollide;

	void OnCollisionEnter(Collision _collision)
	{
		if (OnCollide != null)
			OnCollide (_collision);
	}
}
