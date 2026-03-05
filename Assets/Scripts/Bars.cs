using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200000A RID: 10
public class Bars : MonoBehaviour
{
	// Token: 0x06000024 RID: 36 RVA: 0x000028A0 File Offset: 0x00000AA0
	private void Update()
	{
		this.fuelBar.fillAmount = this.shipController.fuelValue / this.shipController.fuelValueMax;
		this.garbageBar.fillAmount = (float)this.shipController.garbageCount / (float)this.shipController.garbageCountMax;
	}

	// Token: 0x04000018 RID: 24
	public Image fuelBar;

	// Token: 0x04000019 RID: 25
	public Image garbageBar;

	// Token: 0x0400001A RID: 26
	public ShipController shipController;
}
