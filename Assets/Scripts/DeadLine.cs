using System;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class DeadLine : MonoBehaviour
{
	// Token: 0x0600002E RID: 46 RVA: 0x00002AC9 File Offset: 0x00000CC9
	private void OnTriggerEnter2D(Collider2D other)
	{
		other.CompareTag("Ship");
	}
}
