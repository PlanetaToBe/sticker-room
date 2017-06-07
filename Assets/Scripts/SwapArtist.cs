using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapArtist : MonoBehaviour {

	public ToolHub[] toolHubs;
	public ToolHubSimple[] toolHubSimples;

    private int currentArtistIndex = 0;
    private int currentStickerIndex = 0;

	void OnEnable()
	{
		if (toolHubs.Length > 0) {
			for(int i=0; i<toolHubs.Length; i++)
			{
				toolHubs [i].OnTouchpadClick += ChangeArtist;
				toolHubs [i].OnTouchpadClickCenter += ChangeSticker;
			}
		}

		if (toolHubSimples.Length > 0) {
			for(int i=0; i<toolHubSimples.Length; i++)
			{
				toolHubSimples [i].OnTouchpadClick += ChangeArtist;
				//toolHubSimples [i].OnTouchpadClickCenter += ChangeSticker;
			}
		}
	}

	void OnDisable()
	{
		if (toolHubs.Length > 0) {
			for(int i=0; i<toolHubs.Length; i++)
			{
				toolHubs [i].OnTouchpadClick -= ChangeArtist;
				toolHubs [i].OnTouchpadClickCenter -= ChangeSticker;
			}
		}

		if (toolHubSimples.Length > 0) {
			for(int i=0; i<toolHubSimples.Length; i++)
			{
				toolHubSimples [i].OnTouchpadClick -= ChangeArtist;
			}
		}
	}

	public StickerData GetStickerData()
	{
        Debug.Log("Giving sticker " + GetArtistList()[currentArtistIndex] + " " + currentStickerIndex);
        return GetCurrentArtistStickers()[currentStickerIndex];
	}

	public StickerData GetStickerDataRandom()
	{
		//Debug.Log("Giving sticker " + GetArtistList()[currentArtistIndex] + " " + currentStickerIndex);
		return GetCurrentArtistStickers()[Random.Range(0, GetCurrentArtistStickers().Count - 1)];
	}

    public void ChangeArtist(bool up)
    {
        if (up)
        {
            NextArtist();
        } else
        {
            PreviousArtist();
        }
    }

    public void NextArtist()
    {
        ++currentArtistIndex;
        if (currentArtistIndex > GetArtistCount() - 1)
        {
            currentArtistIndex = 0;
        }

        currentStickerIndex = 0;
    }

    public void PreviousArtist()
    {
        --currentArtistIndex;
        if (currentArtistIndex < 0)
        {
            currentArtistIndex = GetArtistCount() - 1;
        }

        currentStickerIndex = 0;
    }

    public void ChangeSticker(bool up)
    {
        if (up)
        {
            NextSticker();
        } else
        {
            PreviousSticker();
        }
    }

    public void NextSticker()
    {
        ++currentStickerIndex;
        if (currentStickerIndex > GetCurrentArtistStickers().Count - 1)
        {
            currentStickerIndex = 0;
        }
    }

    public void PreviousSticker()
    {
        --currentStickerIndex;
        if (currentStickerIndex < 0)
        {
            currentStickerIndex = GetCurrentArtistStickers().Count - 1;
        }
    }

    private List<string> GetArtistList()
    {
        return StickerSceneManager.instance.allArtists;
    }

    private int GetArtistCount()
    {
        return GetArtistList().Count;
    }

    private List<StickerData> GetCurrentArtistStickers()
    {
        string artistName = GetArtistList()[currentArtistIndex];
        return StickerSceneManager.instance.stickersByArtist[artistName];
    }
}
