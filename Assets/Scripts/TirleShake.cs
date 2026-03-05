using System;
using DG.Tweening;
using UnityEngine;

// Token: 0x0200000E RID: 14
public class TirleShake : MonoBehaviour
{
	// Token: 0x06000032 RID: 50 RVA: 0x00002B1C File Offset: 0x00000D1C
	private void Start()
	{
		base.transform.DOShakePosition(2f, new Vector3(10f, 10f, 0f), 0, 90f, false, true, ShakeRandomnessMode.Full).SetLoops(-1, LoopType.Incremental);
	}
}
