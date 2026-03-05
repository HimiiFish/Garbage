using System;
using UnityEngine;

// Token: 0x02000016 RID: 22
public class Plant : MonoBehaviour
{
	// Token: 0x0600004A RID: 74 RVA: 0x000031D4 File Offset: 0x000013D4
	private void Start()
	{
		AudioManager.Instance.Play("BGM1");
	}

	// Token: 0x0600004B RID: 75 RVA: 0x000031E5 File Offset: 0x000013E5
	private void Update()
	{
	}

	// Token: 0x04000034 RID: 52
	[Header("引力系数")]
	[SerializeField]
	public float gravityCoefficient;
}
