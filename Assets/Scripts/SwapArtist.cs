using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapArtist : MonoBehaviour {

	[System.Serializable]
	public class ArtistData
	{
		public int[] workIndex = new int[10];
	}

	public ArtistData[] allArtists;

	private int artistCount;
	private int m_currentArtist;
	public int CurrentArtist
	{
		get { return m_currentArtist; }
		set { m_currentArtist = value; }
	}

	public ToolHub[] toolHubs;

	void Start()
	{
		artistCount = allArtists.Length;
	}

	void OnEnable()
	{
		if (toolHubs.Length > 0) {
			for(int i=0; i<toolHubs.Length; i++)
			{
				toolHubs [i].OnTouchpadClick += SetArtistOnClick;
			}
		}
	}

	void OnDisable()
	{
		if (toolHubs.Length > 0) {
			for(int i=0; i<toolHubs.Length; i++)
			{
				toolHubs [i].OnTouchpadClick -= SetArtistOnClick;
			}
		}
	}

	public StickerData GetStickerData()
	{
		ArtistData c_artist = allArtists [m_currentArtist];
		return StickerSceneManager.instance.data[ c_artist.workIndex[Random.Range(0, c_artist.workIndex.Length)] ];
	}

	public void SetArtist(int _index)
	{
		CurrentArtist = _index % artistCount;
	}

	public void SetArtistOnClick(bool goUp)
	{
		if (goUp)
			m_currentArtist++;
		else
			m_currentArtist--;

		if (m_currentArtist >= artistCount)
			m_currentArtist = 0;
		else if (m_currentArtist < 0)
			m_currentArtist = artistCount-1;
	}
}
