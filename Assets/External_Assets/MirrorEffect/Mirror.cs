using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class Mirror : MonoBehaviour {

	public MirrorCamera mirrorCamera;
    private Material _mirrorMaterial;

    private void Awake() {
		_mirrorMaterial = GetComponent<MeshRenderer>().sharedMaterial;
    }

    private void OnWillRenderObject() {
		mirrorCamera.RenderIntoMaterial(_mirrorMaterial);
    }

}
