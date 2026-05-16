using System;
using DG.Tweening;
using UnityEngine;

public static class StaticData
{
	public static float cameraSize = 5f;

	public static string[] strings = null;

	public static bool isInHole;

	public static float coefficient;

	public static Vector3 blackHoleWorldPosition;

	public static float blackHoleGravityRadius = 5f;

	public static float blackHolePullSpeed = 0.55f;

	public const float OrbitAngularDirectionSign = 1f;

	public static bool IsOrbitalMotionActive { get; private set; }

	public static void BeginOrbitalMotion(Vector3? focusWorld = null)
	{
		if (IsOrbitalMotionActive)
		{
			return;
		}
		FinalizeIntroCamera(focusWorld);
		IsOrbitalMotionActive = true;
		if (GenerateManager.Instance != null)
		{
			GenerateManager.Instance.ClearAllActiveGarbage();
		}
		RotateObject[] orbitals = UnityEngine.Object.FindObjectsOfType<RotateObject>();
		for (int i = 0; i < orbitals.Length; i++)
		{
			if (orbitals[i] != null)
			{
				orbitals[i].ResetOrbitStateFromWorld();
			}
		}
	}

	/// <summary>结束开场相机动画，避免与飞船跟拍 tween 冲突导致公转看起来正反乱跳。</summary>
	public static void FinalizeIntroCamera(Vector3? focusWorld = null)
	{
		Camera cam = Camera.main;
		if (cam == null)
		{
			return;
		}
		cam.DOKill();
		Transform camTf = cam.transform;
		camTf.DOKill();
		if (camTf.parent != null)
		{
			camTf.SetParent(null, true);
		}
		Vector3 focus = focusWorld ?? Vector3.zero;
		camTf.position = new Vector3(focus.x, focus.y, -10f);
		cam.orthographicSize = 20f;
		SpaceStation station = UnityEngine.Object.FindObjectOfType<SpaceStation>();
		if (station != null)
		{
			station.NotifyIntroCameraFinished();
		}
	}
}
