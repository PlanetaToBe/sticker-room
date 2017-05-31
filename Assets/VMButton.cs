using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VMButton : MonoBehaviour {

	public event Action<int, bool> OnSelectArtist;
	public int artistIndex;
	public Material loopDreamMat;

	private bool m_down = false;
	public bool Down
	{
		get { return m_down; }
		set { m_down = value; }
	}
	private Renderer m_renderer;
	private Material ori_mat;

	void Start()
	{
		m_renderer = GetComponentInChildren<Renderer> ();
		ori_mat = m_renderer.material;
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Hand"))
		{
			if(artistIndex==-1)
			{
				if (m_down) {
					MoveButtonDown (m_down);
					// start level 1!!
				}
			}
			else
			{
				ToggleButton ();

				if (OnSelectArtist != null)
					OnSelectArtist (artistIndex, m_down);
			}
		}
	}

	public void ToggleButton()
	{
		m_down = !m_down;
		MoveButtonDown (m_down);

		if (m_down) {
			m_renderer.material = loopDreamMat;
		}
		else {
			m_renderer.material = ori_mat;
		}
	}

	public void ChangeMaterial(bool beFancy)
	{
		if(beFancy)
			m_renderer.material = loopDreamMat;
		else
			m_renderer.material = ori_mat;
	}

	public void MoveButtonDown(bool down)
	{
		if (down) {
			transform.GetChild(0).Translate (0,0,-0.001f);
		}
		else {
			transform.GetChild(0).Translate (0,0,+0.001f);
		}
	}
}
