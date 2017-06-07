using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupDispenser : MonoBehaviour {

	public GameObject cupPrefab;
	public Transform dispensor;

	private IEnumerator dispenseCoroutine;
	private bool doDispense = true;

	void Start()
	{
		dispenseCoroutine = CupDispense ();
		StartCoroutine (dispenseCoroutine);
	}

	void OnDisable()
	{
		StopCoroutine (dispenseCoroutine);
	}

	IEnumerator CupDispense()
	{
		while (doDispense)
		{
			yield return new WaitForSeconds (Random.Range(1f, 3f));
			Instantiate (cupPrefab, dispensor.position, dispensor.rotation);
		}
	}
}
