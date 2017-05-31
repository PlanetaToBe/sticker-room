using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayRender : MonoBehaviour {

	public RenderTexture mirrorTexture;
	public Camera mirrorCamera;

//	mainCamera.targetTexture = renderTexture;
//
//	RenderTexture.active = renderTexture; 
//	mainCamera.Render();
//	screenTexture.ReadPixels( new Rect(0, 0, width , height ), 0, 0 );
//	screenTexture.Apply(false);
//
//	RenderTexture.active = null;
//	mainCamera.targetTexture = null;

	void LateUpdate()
	{
		RenderTexture.active = mirrorTexture;
		mirrorCamera.targetTexture = mirrorTexture;
		mirrorCamera.Render();

		RenderTexture.active = null;
		mirrorCamera.targetTexture = null;
		//Graphics.Blit(mirrorTexture, null as RenderTexture);
	}
}
