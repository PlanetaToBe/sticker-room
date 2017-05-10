using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickerGenerator : MonoBehaviour {

	public SocketManagement socketManagement;
	public GameObject stickerPrefab;

	void OnEnable()
	{
		socketManagement.OnSticker += CreateSticker;
	}

	void OnDisable()
	{
		socketManagement.OnSticker -= CreateSticker;
	}

	void CreateSticker(int _index, string _name)
	{
		Instantiate (stickerPrefab, transform.position, Quaternion.identity, transform);
	}
}
