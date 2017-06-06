﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyPathFinder : MonoBehaviour {

	public GameObject passenger;

	private List<Vector3> pathPoints;
	private LTSpline pathSpline;

	private LTDescr autoFlyTween;
	private int autoFlyTweenId;

	void Start ()
	{
		pathPoints = new List<Vector3> ();

		Mesh mesh = GetComponent<MeshFilter> ().sharedMesh;
		for(int i=0; i<mesh.vertices.Length-32; i+=32)
		{
//			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//			sphere.transform.localScale = Vector3.one / 5f;
//			sphere.transform.position = mesh.vertices [i] + transform.position;
			pathPoints.Add (mesh.vertices [i] + transform.position);
		}

		Vector3[] ptsArray = new Vector3[pathPoints.Count + 2];
		ptsArray [0] = pathPoints [0];
		for(int i=0; i<pathPoints.Count; i++)
		{
			ptsArray [i+1] = pathPoints [i];
		}
		ptsArray [ptsArray.Length-1] = pathPoints [pathPoints.Count-1];

		pathSpline = new LTSpline (ptsArray);
		//LeanTween.moveSpline (passenger, pathSpline, 10f);
	}
	
	public void DoAutoFly(GameObject thePassenger, float duration)
	{
		autoFlyTween = LeanTween.moveSpline (thePassenger, pathSpline, duration)
			.setEaseInOutQuad ();
		autoFlyTweenId = autoFlyTweenId;
	}

	public void PauseAutoFly()
	{
		autoFlyTween.pause ();
	}

	public void ResuemAutoFly()
	{
		autoFlyTween.resume ();
	}

	public void CancelAutoFly()
	{
		LeanTween.cancel (autoFlyTweenId);
	}

}
