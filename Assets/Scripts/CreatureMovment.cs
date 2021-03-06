﻿using System.Collections;
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
	public float RotationSpeed=10f;
	public float neckOffset = 0.25f;
	public float shoulderOffset = 0.156f;
	public float neckLength = 0.1f;

	public Vector3 HeadPos
	{
		get {
			var camEye = CameraEye.localPosition;
			camEye.z -= neckOffset;// * PlayerSize;
			return camEye;
		}
	}
	public Vector3 ShoulderRightPos
	{
		get {
//			var _pos = HeadPos;
////			_pos.y -= neckLength;
//			_pos.x -= shoulderOffset;// * PlayerSize;
			var _pos = arm_R.position;
			_pos.y += shoulderOffset * PlayerSize;
			return _pos;
		}
	}
	public Vector3 ShoulderLeftPos
	{
		get {
//			var _pos = HeadPos;
////			_pos.y -= neckLength;
//			_pos.x += shoulderOffset;// * PlayerSize;
			var _pos = arm_L.position;
			_pos.y += shoulderOffset * PlayerSize;
			return _pos;
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
			// === Position ===
			// bone: sync x & z, y on the ground
			tmpPivotPot = CameraEye.localPosition;
			tmpPivotPot.y = 0f;
			pivot.parent.localPosition = tmpPivotPot;

			// pivot: same as bone, except z - offset
			tmpPivotPot.z = -neckOffset;// * PlayerSize;
			tmpPivotPot.x = 0f;
			pivot.localPosition = tmpPivotPot;

			// spine (Eye): child of pivot, sync y with camera
			tmpSpinePos = CameraEye.localPosition;
			tmpSpinePos.x = tmpSpinePos.z = 0f;
			spine.localPosition = tmpSpinePos;

			// === Rotation ===
			// bone
			//tmpRot = CameraEye.rotation;
			tmpRot = CameraEye.localRotation;
			tmpRot.x = tmpRot.z = 0f;
			pivot.parent.localRotation = tmpRot;// = new Vector3 (0f, CameraEye.eulerAngles.y, 0f);

			// right arm
			if(Controller_R.gameObject.activeSelf)
			{
				Vector3 relativePos = (Controller_R.position - ShoulderRightPos).normalized;
				//Vector3 relativePos = (Controller_R.position - arm_R.position).normalized; //=>match perfectly but cover controller
				Quaternion lookRotation = Quaternion.LookRotation(relativePos);
				arm_R.rotation = Quaternion.Slerp(arm_R.rotation, lookRotation, Time.deltaTime * RotationSpeed);
			}

			// left arm
			if(Controller_L.gameObject.activeSelf)
			{
				Vector3 relativePos = (Controller_L.position - ShoulderLeftPos).normalized;
				//Vector3 relativePos = (Controller_L.position - arm_L.position).normalized; //=>match perfectly but cover controller
				Quaternion lookRotation = Quaternion.LookRotation(relativePos); //, Vector3.up
				arm_L.rotation = Quaternion.Slerp(arm_L.rotation, lookRotation, Time.deltaTime * RotationSpeed);
			}
		}
	}
}
