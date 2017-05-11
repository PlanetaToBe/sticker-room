using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StickerGenerator : MonoBehaviour {

	// from Server
	public SocketManagement socketManagement;
	public GameObject stickerPrefab;

	void OnEnable()
	{
		if(socketManagement != null)
			socketManagement.OnSticker += CreateSticker;

	}

	void OnDisable()
	{
		if(socketManagement != null)
			socketManagement.OnSticker -= CreateSticker;
	}

	void CreateSticker(int _index, string _name)
	{
		Instantiate (stickerPrefab, transform.position, Quaternion.identity, transform);
	}

}
