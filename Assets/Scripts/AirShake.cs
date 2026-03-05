using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

// Token: 0x0200000D RID: 13
public class AirShake : MonoBehaviour
{
	// Token: 0x06000030 RID: 48 RVA: 0x00002ADF File Offset: 0x00000CDF
	private void Start()
	{
		base.transform.DORotate(new Vector3(0f, 0f, 5f), 1f, RotateMode.Fast).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
	}
}
