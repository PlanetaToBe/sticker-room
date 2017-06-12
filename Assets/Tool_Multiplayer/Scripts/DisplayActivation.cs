using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayActivation : MonoBehaviour {

	void Start()
	{
		Debug.Log ("Display connected: " + Display.displays.Length);
		if (Display.displays.Length > 1)
		{
			// if not active yet
			if (!Display.displays [1].active)
			{
				Debug.Log ("about to activate Display_1");
				Display.displays [1].Activate ();
				Debug.Log ("activate Display_1");
			}
		}
//		if (Display.displays.Length > 2)
//			Display.displays [2].Activate ();
	}
}
