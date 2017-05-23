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
	public float neckOffset = 0.1f;

	[Header("Toolbelt Reference")]
	public Transform toolBelt;

	public Vector3 HeadPos
	{
		get {
			var camEye = CameraEye.localPosition;
			camEye.z -= neckOffset * PlayerSize;
			return camEye;
		}
	}
	public float PlayerSize
	{
		get { return transform.parent.localScale.x; }
	}

	private Vector3 tmpPivotPot, tmpSpinePos;
	private Quaternion tmpRot;

	void Update()
	{
		if(CameraEye.gameObject.activeSelf)
		{
			tmpPivotPot = CameraEye.localPosition;
			tmpPivotPot.y = 0f;
			pivot.parent.localPosition = tmpPivotPot;

			tmpPivotPot.z = -neckOffset * PlayerSize;
			tmpPivotPot.x = 0f;
			pivot.localPosition = tmpPivotPot;

			tmpSpinePos = CameraEye.localPosition;
			tmpSpinePos.x = tmpSpinePos.z = 0f;
			spine.localPosition = tmpSpinePos;

			tmpSpinePos = spine.position;
			tmpSpinePos.y -= 0.5f;
			toolBelt.position = tmpSpinePos;

			tmpRot = CameraEye.rotation;
			tmpRot.x = tmpRot.z = 0f;
			pivot.parent.localRotation = tmpRot;// = new Vector3 (0f, CameraEye.eulerAngles.y, 0f);

			if(Controller_R.gameObject.activeSelf)
			{
				Vector3 relativePos = (Controller_R.localPosition - HeadPos).normalized;
				Quaternion lookRotation = Quaternion.LookRotation(relativePos);
				arm_R.rotation = Quaternion.Slerp(arm_R.rotation, lookRotation, Time.deltaTime * RotationSpeed);
			}

			if(Controller_L.gameObject.activeSelf)
			{
				Vector3 relativePos = (Controller_L.localPosition - HeadPos).normalized;
				Quaternion lookRotation = Quaternion.LookRotation(relativePos); //, Vector3.up
				arm_L.rotation = Quaternion.Slerp(arm_L.rotation, lookRotation, Time.deltaTime * RotationSpeed);
			}
		}
	}
}
