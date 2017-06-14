using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTrigger : MonoBehaviour {

	public System.Action OnTrigger;

	void OnTriggerEnter() {
		OnTrigger ();
	}
}
