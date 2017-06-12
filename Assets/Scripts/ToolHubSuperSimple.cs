using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Tool hub super simple
/// - connect to levelManager
/// - switch tool automatically and strictly
/// - DebugMode to iterate tools
/// </summary>
public class ToolHubSuperSimple : MonoBehaviour {

	public SteamVR_TrackedController controller;
	public SteamVR_Controller.Device Device
	{
		get { return SteamVR_Controller.Input ((int)controller.controllerIndex); }
	}
	public LevelManager levelManager;
	public int[] toolForLevel;

	private List<GameObject> toolObjects = new List<GameObject> ();
	public int currToolIndex = -1;
	private List<StickerTool> stickerTools = new List<StickerTool> ();
	private StickerTool currStickerTool;

	private bool m_enable = true;
	public bool ToolsetEnable
	{
		get { return m_enable; }
		set { m_enable = value; }
	}

	public bool debugMode = false;
	//============================================================
	void Start()
	{
		for(int i=0; i<transform.childCount; i++)
		{
			var _t = transform.GetChild (i).gameObject;
			toolObjects.Add (_t);

			var s_t = _t.GetComponent<StickerTool> ();
			s_t.ToolIndex = i;
			stickerTools.Add (s_t);
		}

		DisableAllTools ();
	}

	void OnEnable()
	{
		if (controller != null)
		{
			if (debugMode)
			{
				controller.PadClicked += OnClick;
				controller.PadUnclicked += OnClickEnd;
			}
		}

		if (levelManager!=null)
		{
			levelManager.OnLevelTransition += OnLevelTransition;
			levelManager.OnLevelStart += OnLevelStart;
		}
	}

	void OnDisable()
	{
		if (controller != null)
		{
			if (debugMode)
			{
				controller.PadClicked -= OnClick;
				controller.PadUnclicked -= OnClickEnd;
			}
		}

		if (levelManager!=null)
		{
			levelManager.OnLevelTransition -= OnLevelTransition;
			levelManager.OnLevelStart -= OnLevelStart;
		}
	}

	//============================================================
	public void OnClick (object sender, ClickedEventArgs e)
	{
		if (LeanTween.isTweening(gameObject)) {
			Debug.Log ("in tweening");
			return;
		}

		if (!ToolsetEnable) {
			Debug.Log ("Toolset Enable");
			return;
		}
		
		// TODO: Switch to next tool
		currToolIndex++;
		if (currToolIndex >= toolObjects.Count)
			currToolIndex = 0;

		SwitchToTool (currToolIndex);

		DeviceVibrate ();
	}

	public void OnClickEnd (object sender, ClickedEventArgs e)
	{
		//
	}

	public void OnLevelTransition(int levelIndex)
	{
		//DisableAllTools ();
	}

	public void OnLevelStart(int levelIndex)
	{
		EnableAllTools ();

		SwitchToTool (toolForLevel [levelIndex]);
	}
	//===========================================================================
	public void DeviceVibrate()
	{
		Device.TriggerHapticPulse (1000);
	}

	public void DisableAllTools()
	{
		for(int i=0; i<stickerTools.Count; i++)
		{
			stickerTools [i].DisableTool ();
		}
		ToolsetEnable = false;
		currStickerTool = null;
		currToolIndex = -1;
	}

	public void EnableAllTools()
	{
		ToolsetEnable = true;
	}
		
	public void SwitchToTool(int toolIndex)
	{
		// disable
		for(int i=0; i<stickerTools.Count; i++)
		{
			if (i != toolIndex)
			{
				if(stickerTools[i].inUse)
					stickerTools [i].DisableTool ();
			}
		}

		// enable
		stickerTools[toolIndex].EnableTool ();
		currStickerTool = stickerTools[toolIndex];
		currToolIndex = toolIndex;

		// scale (happen in StickerTool)
	}
}
