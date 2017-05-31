using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkyColorManager : MonoBehaviour {

    [System.Serializable]
    public class SkyboxColorSet
    {
        public Color32[] colors = new Color32[2];
    }

    public SkyboxColorSet[] skyboxColors;
    public Material skyboxMaterial;
    public Color[] ambientColors;
    public float animationDuration = 2f;
    public bool animateColorTransitions = true;
    public bool animateAmbientLightTransitions = true;
	public bool animateDirLightTransitions = true;

	public Light directionLight;
	public float[] dirLightIntensity;

    float brightnessFrom = 0.1f;
    float brightnessTo = 0.1f;

    private List<int> runningTweenIds = new List<int>();

    public void OnDisable()
    {
        ClearRunningTweens();
    }

    public void SetFloor(int floorIndex)
    {
        ClearRunningTweens();
        //UpdateSkyboxColor(floorIndex);
        
		UpdateAmbientColor(floorIndex);
		UpdateDirectionLight (floorIndex);
    }

	public void UpdateDirectionLight(int floorIndex)
	{
		if(Mathf.Abs(directionLight.intensity - dirLightIntensity[floorIndex]) < 0.1f)
		{
			Debug.Log ("too small to change dir light");
			return;
		}

		if (animateDirLightTransitions)
		{
			UpdateDirLightAnimated(floorIndex);

		} else
		{
			UpdateDirLightStatic(floorIndex);
		}
	}

    public void UpdateSkyboxColor(int floorIndex) {
        if (animateColorTransitions)
        {
            UpdateSkyboxColorAnimated(floorIndex);
        } else
        {
            UpdateSkyBoxColorSimple(floorIndex);
        }
    }

    private void UpdateSkyBoxColorSimple(int floorIndex)
    {
        skyboxMaterial.SetColor("_Color1", skyboxColors[floorIndex].colors[1]);
        skyboxMaterial.SetColor("_Color2", skyboxColors[floorIndex].colors[0]);
    }

    private void UpdateSkyboxColorAnimated(int floorIndex)
    {
        Color firstStartColor = skyboxMaterial.GetColor("_Color1");
        LTDescr firstTween = LeanTween.value(transform.gameObject, firstStartColor, skyboxColors[floorIndex].colors[1], animationDuration);
        firstTween.setOnUpdate((Color color) => {
            skyboxMaterial.SetColor("_Color1", color);
        });

        Color secondStartColor = skyboxMaterial.GetColor("_Color2");
        LTDescr secondTween = LeanTween.value(transform.gameObject, secondStartColor, skyboxColors[floorIndex].colors[0], animationDuration);
        secondTween.setOnUpdate((Color color) => {
            skyboxMaterial.SetColor("_Color2", color);
        });

        runningTweenIds.Add(firstTween.id);
        runningTweenIds.Add(secondTween.id);
    }

    public void UpdateAmbientColor(int floorIndex)
    {
        if (animateAmbientLightTransitions)
        {
            UpdateAmbientColorAnimated(floorIndex);

        } else
        {
            UpdateAmbientColorStatic(floorIndex);
        }
    }

    private void UpdateAmbientColorAnimated(int floorIndex)
    {
        LTDescr tween = LeanTween.value(transform.gameObject, RenderSettings.ambientLight, ambientColors[floorIndex], animationDuration);
        tween.setOnUpdate((Color color) => {
            RenderSettings.ambientLight = color;
        });

        runningTweenIds.Add(tween.id);
    }

    private void UpdateAmbientColorStatic(int floorIndex)
    {
        RenderSettings.ambientLight = ambientColors[floorIndex];
    }

	private void UpdateDirLightAnimated(int floorIndex)
	{
		LTDescr tween = LeanTween.value(directionLight.gameObject, directionLight.intensity, dirLightIntensity[floorIndex], animationDuration);
		tween.setOnUpdate((float value) => {
			directionLight.intensity = value;
		});
		runningTweenIds.Add(tween.id);
	}

	private void UpdateDirLightStatic(int floorIndex)
	{
		directionLight.intensity = dirLightIntensity[floorIndex];
	}

    public void UpdateAmbientColorBrightness(int floorIndex) {
        if (floorIndex == 0)
        {
            brightnessFrom = 0.6f;
            brightnessTo = 0.1f;
        }
        else
        {
            brightnessFrom = 0.1f * (floorIndex);
            brightnessTo = 0.1f * (floorIndex + 1);
        }
        Debug.Log("change ac brightness from " + brightnessFrom + "to " + brightnessTo);
        //LeanTween.value( transform.gameObject, UpdateAC, brightnessFrom, brightnessTo, 1f);
    }
    
    private void ClearRunningTweens()
    {
        foreach (int tweenId in runningTweenIds)
        {
            LeanTween.cancel(tweenId);
        }

        runningTweenIds.Clear();
    }
    
}
