using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

	public event Action<int> OnLevelTransition;
	public event Action<int> OnLevelStart;
	public event Action<int> OnLevelEnd;
	public event Action OnEndStart;

	public int currentLevel = -1;
	public int oldLevel = -1;
	public GameObject[] levels;

	// even: transition =>(#/2), odd: level => Math.Floor(#/2)
	public int[] times = new int[] {0, 1, 60, 70, 180, 190, 210};
	public SteamVR_TrackedController[] controllers;

	[Header("Automation")]
	public FastForward forwardToLevel1;

	private Dictionary<int, GameObject> levelsDict;
	private Dictionary<int, Level> levelScriptDict;
	private List<Level> levelScripts;

	private SkyColorManager skyManager;
	private SkyManager azureSkyManager;
	private MusicManager musicManager;

	private int currentState = -1;
	private int passState = -1;

	private float startTime = -1f;
	private bool m_start = false;
	public bool ToStart
	{
		get { return m_start; }
		set {
			m_start = value;
			if (m_start) {
				startTime = Time.time;
			}
		}
	}

	private int padDownCount = 0;

	void Awake()
	{
		SteamVR_Fade.Start(Color.black, 0f);
		SteamVR_Fade.Start(Color.clear, 3f);

		levelsDict = new Dictionary<int, GameObject> ();
		levelScriptDict = new Dictionary<int, Level> ();
		levelScripts = new List<Level> ();

		for(int i=0; i<levels.Length; i++)
		{
			var l_script = levels [i].GetComponent<Level> ();

			levelScripts.Add (l_script);
			levelsDict.Add (l_script.levelNum, levels[i]);
			levelScriptDict.Add (l_script.levelNum, l_script);

			l_script.StrictDeactivate ();
			l_script.Deactivate ();
			l_script.ToggleAudio (false);
		}

		skyManager = GetComponent<SkyColorManager> ();
		azureSkyManager = GetComponent<SkyManager> ();

		// 
		//VisitorEnterIndex (0);
	}

	void Start()
	{
		// toggle lights in Start(), after getting original intensity in Awake() of each level
		for(int i=0; i<levelScripts.Count; i++)
		{
			levelScripts[i].ToggleLight(false);
		}
		startTime = -1f;
		// TEST
		//ToStart = true;
	}

	void OnEnable()
	{
		for(int i=0; i<controllers.Length; i++)
		{
			controllers[i].TriggerClicked += HandlePadDown;
			controllers[i].TriggerUnclicked += HandlePadUp;
		}

		if (forwardToLevel1) {
			forwardToLevel1.OnControllerEnter += DoForwardToLevel;
		}
	}

	void OnDisable()
	{
		for(int i=0; i<controllers.Length; i++)
		{
			controllers[i].TriggerClicked -= HandlePadDown;
			controllers[i].TriggerUnclicked -= HandlePadUp;
		}

		if (forwardToLevel1) {
			forwardToLevel1.OnControllerEnter -= DoForwardToLevel;
		}
	}

	void Update()
	{
		if (!m_start)
		{
			if (Input.GetKey (KeyCode.Return) || Input.GetKey (KeyCode.KeypadEnter) || padDownCount==2) {
				ToStart = true;
			}
			return;
		}
		
		currentState = time_state (Time.time - startTime);

		if(passState != currentState)
		{
			if (currentState < times.Length - 1)
			{
				int phase_index = (int)(currentState / 2);
				if (currentState % 2 == 0)
				{
					int fake_phase = phase_index - 1;
					if (OnLevelTransition != null && fake_phase >= 0)
					{
						OnLevelTransition (fake_phase);
						Debug.Log ("OnLevelTransition: " + fake_phase);

						if(fake_phase==2)
						{
							if (azureSkyManager)
								azureSkyManager.SetFloor (phase_index);
						}
					}
				} 
				else
				{
					VisitorEnterIndex (phase_index);

					if (OnLevelStart != null)
						OnLevelStart (phase_index);
					
					Debug.Log ("OnLevelStart: " + phase_index);

					if(phase_index>0 && OnLevelEnd!=null)
						OnLevelEnd (phase_index-1);
				}
			}
			else
			{
				Debug.Log ("end end!");

				if (OnEndStart != null)
					OnEndStart ();

				// TODO: pause all tools

				OnLevelEnd (2);

				// fade out => reload
				SteamVR_Fade.Start(Color.black, 3f);
				Invoke ("Reload", 5f);
				m_start = false;
			}
		}
		passState = currentState;
	}

	void Reload()
	{
		SceneManager.LoadSceneAsync (0, LoadSceneMode.Single);
	}

	private void VisitorEnterIndex(int l_index)
	{
		if(l_index == currentLevel)
		{
			Debug.Log ("Visitor already entered Level #" + l_index);
			return;
		}

		Debug.Log ("Visitor enter Level #" + l_index);
		currentLevel = l_index;
		var current_l_script = levelScriptDict [currentLevel];

		if(skyManager)
			skyManager.SetFloor (l_index);

		if (azureSkyManager)
			azureSkyManager.SetFloor (l_index);

		// activate current, if not already
		if (!current_l_script.onMode)
		{
			current_l_script.Activate ();
		}
		current_l_script.StrictActivate ();
		current_l_script.ToggleLight (true);
		current_l_script.ToggleAudio (true);

		// Deactivate (strictly) the old level
		if(levelScriptDict.ContainsKey(oldLevel))
		{
			levelScriptDict [oldLevel].StrictDeactivate ();
			levelScriptDict [oldLevel].ToggleLight (false);
			levelScriptDict [oldLevel].ToggleAudio (false);
		}

		// Toggle in advance for smooth transition
		int loseOnIndex, loseOffIndex;

		// INDEX_DECREASE (when decline)
		if(oldLevel - currentLevel > 0)
		{
			// deactivate strict: old + 2
			loseOffIndex = currentLevel+2;

			// activate: current - 1
			loseOnIndex = currentLevel-1;
		}
		else // INDEX_DECREASE (when increment)
		{
			// deactivate strict: old - 2
			loseOffIndex = currentLevel-2;

			// activate: current + 1
			loseOnIndex = currentLevel+1;
		}

		if(levelScriptDict.ContainsKey(loseOffIndex))
		{
			levelScriptDict [loseOffIndex].Deactivate ();
		}

		if(levelScriptDict.ContainsKey(loseOnIndex))
		{
			if(!levelScriptDict [loseOnIndex].onMode)
			{
				levelScriptDict [loseOnIndex].Activate ();
				//Debug.Log ("swap level: " + loseOnIndex);
			}
		}

		// update
		oldLevel = currentLevel;
	}

	private int time_state(float this_time)
	{
		for (int i=0; i<times.Length-1; i++)
		{
			if (times[i] <= this_time && this_time < times[i + 1])
			{
				return i;
			}
		}
		return times.Length - 1;
	}

	private void HandlePadDown(object sender, ClickedEventArgs e)
	{
		padDownCount++;
	}

	private void HandlePadUp(object sender, ClickedEventArgs e)
	{
		padDownCount--;
	}

	private void DoForwardToLevel()
	{
		float currPassTime = Time.time - startTime;
		float idealPasTime = 34f;
		startTime -= idealPasTime - currPassTime;
	}
}
