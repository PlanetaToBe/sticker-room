using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovment : MonoBehaviour {

	public Transform CameraEye;
	public Transform CameraRig;
	public Transform Controller_R;
	public Transform Controller_R1;
	public Transform Controller_R2;
	public Transform Controller_L;

	public Transform pivot;
	public Transform spine;
	public Transform arm_R;
	public Transform arm_L;

	void Update()
	{
		if(CameraEye.gameObject.activeSelf)
		{
			pivot.localEulerAngles = new Vector3 (0f, CameraEye.eulerAngles.y, 0f);

			if(Controller_R.gameObject.activeSelf)
			{
				Vector3 relativePos = Controller_R.localPosition + Controller_R1.position - CameraEye.localPosition;
				Vector3 relativePos2 = Controller_R.localPosition + Controller_R2.position - CameraEye.localPosition;
				Vector3 normal = Vector3.Cross (relativePos, relativePos2);
				Quaternion rotation = Quaternion.LookRotation(relativePos, normal);
				arm_R.localRotation = rotation;
			}

			if(Controller_L.gameObject.activeSelf)
			{
				Vector3 relativePos = Controller_L.position - CameraEye.position;
				Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
				arm_L.rotation = rotation;
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
