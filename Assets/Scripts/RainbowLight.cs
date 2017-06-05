using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowLight : MonoBehaviour {

	private Light light;
	private LTDescr tween;

	void Start ()
	{
		light = GetComponent<Light> ();
	}

	void OnEnable()
	{
		if (tween!=null)
		{
			tween.resume ();
		}
	}

	void OnDisable()
	{
		if (tween!=null)
		{
			tween.pause ();
		}
	}

	public void StartRainbowLight()
	{
		LeanTween.value (gameObject, 0f, 1f, 2f)
			.setOnUpdate ((float val) => {
				light.color = Color.HSVToRGB (val, val/5f, 1f);
			})
			.setOnComplete(DoRainbow);
	}

	void DoRainbow()
	{
		tween = LeanTween.value(gameObject, 0f, 1f, 5f)
			.setOnUpdate((float val)=>{
				light.color = Color.HSVToRGB(val, 0.2f, 1f);
			})
			.setLoopPingPong(-1);
	}
}
