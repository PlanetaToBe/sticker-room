using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovment : MonoBehaviour {

	[Header("Vive Reference")]
	public Transform CameraEye;
	public Transform CameraRig;
	public Transform Controller_R;
	public Transform Controller_L;

	[Header("Creature Reference")]
	public Transform pivot;
	public Transform spine;
	public Transform arm_R;
	public Transform arm_L;
	public float RotationSpeed=1f;

	void Update()
	{
		if(CameraEye.gameObject.activeSelf)
		{
			Vector3 pivotPos = CameraEye.localPosition;
			pivotPos.y = 0f;
			pivot.localPosition = pivotPos;

			Vector3 spinePos = CameraEye.localPosition;
			spinePos.x = spinePos.z = 0f;
			spine.localPosition = spinePos;

			pivot.localEulerAngles = new Vector3 (0f, CameraEye.eulerAngles.y, 0f);

			if(Controller_R.gameObject.activeSelf)
			{
				Vector3 relativePos = (Controller_R.localPosition - CameraEye.localPosition).normalized;
				Quaternion lookRotation = Quaternion.LookRotation(relativePos);
				arm_R.rotation = Quaternion.Slerp(arm_R.rotation, lookRotation, Time.deltaTime * RotationSpeed);
			}

			if(Controller_L.gameObject.activeSelf)
			{
				Vector3 relativePos = (Controller_L.localPosition - CameraEye.localPosition).normalized;
				Quaternion lookRotation = Quaternion.LookRotation(relativePos); //, Vector3.up
				arm_L.rotation = Quaternion.Slerp(arm_L.rotation, lookRotation, Time.deltaTime * RotationSpeed);
			}
		}
	}

	Quaternion GetRotation(Vector3 u, Vector3 v)
	{
		Quaternion rot = Quaternion.identity;

		float k_cos_theta = Vector3.Dot(u, v);
		float k = Mathf.Sqrt (Mathf.Pow (u.magnitude, 2F) * Mathf.Pow (v.magnitude, 2F));

		if(k_cos_theta/k==-1f)
		{
			Vector3 ortho = orthogonal (u).normalized;
			rot.Set (ortho.x, ortho.y, ortho.z, 0);
		}
		Vector3 vec = Vector3.Cross (u, v);
		rot.Set (vec.x, vec.y, vec.z, k_cos_theta+k);

		return rot;
	}

	Vector3 orthogonal(Vector3 v)
	{
		float x = Mathf.Abs(v.x);
		float y = Mathf.Abs(v.y);
		float z = Mathf.Abs(v.z);

		Vector3 other = x < y ? (x < z ? Vector3.right : Vector3.forward) : (y < z ? Vector3.up : Vector3.forward);
		return Vector3.Cross(v, other);
	}
}
