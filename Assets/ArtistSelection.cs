using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtistSelection : MonoBehaviour {

	public VMButton[] buttons;

	public List<int> selectedAritsts = new List<int>();

	void OnEnable()
	{
		for(int i=0; i<buttons.Length; i++)
		{
			buttons[i].OnSelectArtist += RegisterArtist;
		}
	}

	void OnDisable()
	{
		for(int i=0; i<buttons.Length; i++)
		{
			buttons[i].OnSelectArtist -= RegisterArtist;
		}
	}

	void RegisterArtist(int artistIndex, bool selected)
	{
		// to add
		if(selected)
		{
			if(selectedAritsts.Count==2)
			{
				// if already selected 2, pop out the old one
				buttons[selectedAritsts[0]].ToggleButton();
				selectedAritsts.RemoveAt (0);
				selectedAritsts.Add (artistIndex);
			}
			else
			{
				selectedAritsts.Add (artistIndex);
			}

			if (selectedAritsts.Count == 2)
			{
				// can start!
				buttons [6].ChangeMaterial(true);
				buttons [6].Down = true;
			}
		}
		else
		{
			// to remove
			if(selectedAritsts.Contains(artistIndex))
			{
				selectedAritsts.Remove (artistIndex);

				// cancel start
				if(buttons[6].Down)
				{
					buttons [6].ChangeMaterial(false);
					buttons [6].Down = false;
				}
			}
		}
	}
}
