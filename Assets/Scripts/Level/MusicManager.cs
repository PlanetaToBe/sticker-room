using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour {

	[System.Serializable]
	public class AudioSet
	{
		public int genreIndex = 0; // pop, classical, instrumental, jazz
		public AudioClip[] clips = new AudioClip[3];
	}

	[SerializeField]private Dropdown waterDropD;
	[SerializeField]private Dropdown stereoDropD;
	[SerializeField]private Dropdown airDropD;

	public AudioSet[] audioFiles;
	// level + audioSet
	public Dictionary<int, AudioSet> levelAudioDict;

	void Start()
	{
		levelAudioDict = new Dictionary<int, AudioSet> ();

		waterDropD.onValueChanged.AddListener (delegate {
			SetLevelAudio(0, waterDropD.value);
		});

		stereoDropD.onValueChanged.AddListener (delegate {
			SetLevelAudio(1, stereoDropD.value);
		});

		airDropD.onValueChanged.AddListener (delegate {
			SetLevelAudio(4, airDropD.value);
		});
	}

	void OnDestroy()
	{
		waterDropD.onValueChanged.RemoveAllListeners();
		stereoDropD.onValueChanged.RemoveAllListeners();
		airDropD.onValueChanged.RemoveAllListeners();
	}

	public void SetLevelAudio(int _level, int _audioIndex)
	{
		int audioToPlay = _audioIndex;

		if (_audioIndex == 0)
		{
			// Play reset version
			if (levelAudioDict.ContainsKey (_level))
				levelAudioDict [_level] = null;
			else
				levelAudioDict.Add (_level, null);
		}
		else
		{
			audioToPlay--;

			if (levelAudioDict.ContainsKey (_level))
			{
				levelAudioDict [_level] = audioFiles [audioToPlay];
			}
			else
			{
				levelAudioDict.Add (_level, audioFiles [audioToPlay]);
			}
		}

		Debug.Log ("Level #" + _level + " plays audio set #" + _audioIndex);
	}

	public AudioSet GetLevelAudioSet(int _level)
	{
		if (levelAudioDict.ContainsKey (_level))
		{
			return levelAudioDict [_level];
		}
		else
		{
			return null;
		}
	}
		
}
