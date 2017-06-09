using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewSticker : MonoBehaviour {

	void Start () {
        SwapArtist stickerTracker = FindObjectOfType<SwapArtist>();
        stickerTracker.StickerChanged += UpdateSticker;

        GetComponent<Sticker>().SetSticker(stickerTracker.GetStickerData().id);
    }

    void UpdateSticker(StickerData sticker)
    {
        GetComponent<Sticker>().SetSticker(sticker.id);
    }
	
}
