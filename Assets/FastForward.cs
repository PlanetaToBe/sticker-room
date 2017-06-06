using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FastForward : MonoBehaviour {

	public event Action OnControllerEnter;
	private bool isFastforwarded = false;

	void OnTriggerEnter(Collider _collider)
	{
		if(_collider.CompareTag("GameController"))
		{
			if (OnControllerEnter != null && !isFastforwarded) {
				OnControllerEnter ();
				isFastforwarded = true;
			}
		}
	}
}
