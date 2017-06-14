using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Level : MonoBehaviour {

	public int levelNum = 0;
	public GameObject[] thingsToToggle;
	public GameObject[] thingsToStrictToggle;
	public event Action<bool> OnToggle;
	public event Action<bool> OnStrictToggle;
	public event Action<int> OnAudiosetChange;
	[HideInInspector]
	public bool onMode = false;

	[Header("Sound file settings")]
	public GvrAudioSource[] audioSource;
	private float[] audioSource_ori_volume;

	[Header("Light settings")]
	public float transitionDuration = 1f;
	public Light[] lights;
	private float[] light_ori_inensity;

	void Awake()
	{
		// ===AUDIO===
		audioSource_ori_volume = new float[audioSource.Length];
		for (int i = 0; i < audioSource.Length; i++)
		{
			audioSource_ori_volume [i] = audioSource [i].volume;
		}

		// ===LIGHT===
		light_ori_inensity = new float[lights.Length];
		for (int i = 0; i < lights.Length; i++)
		{
			light_ori_inensity [i] = lights [i].intensity;
		}
	}

	public void Activate()
	{
		for(int i=0; i<thingsToToggle.Length; i++)
		{
			thingsToToggle[i].SetActive(true);
		}

		if (OnToggle != null)
			OnToggle (true);

		onMode = true;
	}

	public void Deactivate()
	{
		for(int i=0; i<thingsToToggle.Length; i++)
		{
			thingsToToggle[i].SetActive(false);
		}

		if (OnToggle != null)
			OnToggle (false);

		onMode = false;
		Debug.Log ("deactivate level things: #" + levelNum);
	}

	public void StrictActivate()
	{
		for(int i=0; i<thingsToStrictToggle.Length; i++)
		{
			thingsToStrictToggle[i].SetActive(true);
		}
		if (OnStrictToggle != null)
			OnStrictToggle (true);
	}

	public void StrictDeactivate()
	{
		for(int i=0; i<thingsToStrictToggle.Length; i++)
		{
			thingsToStrictToggle[i].SetActive(false);
		}
		if (OnStrictToggle != null)
			OnStrictToggle (false);
	}

	public void ToggleAudio(bool turnOn)
	{
		for (int i = 0; i < audioSource.Length; i++)
		{	
			GvrAudioSource a_source = audioSource [i];

			if (turnOn)
			{
				a_source.UnPause();

				if (!a_source.isPlaying)
				{
					a_source.Play();
					//Debug.Log ("play " + audioSource [i].name);
				}

				// volume up!
				LeanTween.value(a_source.gameObject, 0f, audioSource_ori_volume[i], 1f)
					.setOnUpdate((float val)=>{
						a_source.volume = val;
					});
			}
			else
			{
				if (a_source.isPlaying)
				{
					LeanTween.value(a_source.gameObject, a_source.volume, 0f, 1f)
						.setOnUpdate((float val)=>{
							a_source.volume = val;
						})
						.setOnComplete(()=>{
							//a_source.Pause();
							a_source.Stop();
						});
				}
			}
		}
	}

	public void ToggleLight(bool turnOn)
	{
		for (int i = 0; i < lights.Length; i++)
		{	
			Light li = lights [i];

			if (turnOn)
			{
				LeanTween.value(li.gameObject, 0f, light_ori_inensity[i], transitionDuration)
					.setOnUpdate((float val)=>{
						li.intensity = val;
					});
			}
			else
			{
				LeanTween.value(li.gameObject, light_ori_inensity[i], 0f, transitionDuration)
					.setOnUpdate((float val)=>{
						li.intensity = val;
					});
			}
		}
	}
}
