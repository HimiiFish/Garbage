using System;
using DG.Tweening;
using UnityEngine;

// Token: 0x0200000F RID: 15
public class UpDownShake : MonoBehaviour
{
	// Token: 0x06000034 RID: 52 RVA: 0x00002B5B File Offset: 0x00000D5B
	private void Start()
	{
		base.transform.DOShakePosition(2f, new Vector3(10f, 0f, 0f), 0, 0f, false, true, ShakeRandomnessMode.Full).SetLoops(-1, LoopType.Incremental).SetEase(Ease.InExpo);
	}
}
