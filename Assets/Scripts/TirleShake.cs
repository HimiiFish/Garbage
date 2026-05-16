using System;
using DG.Tweening;
using UnityEngine;

public class TirleShake : MonoBehaviour
{
	private void Start()
	{
		base.transform.DOShakePosition(2f, new Vector3(10f, 10f, 0f), 0, 90f, false, true, ShakeRandomnessMode.Full).SetLoops(-1, LoopType.Incremental);
	}
}
