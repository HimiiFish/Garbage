using System;
using UnityEngine;
using UnityEngine.UI;

public class Bars : MonoBehaviour
{
	private void Update()
	{
		this.fuelBar.fillAmount = this.shipController.fuelValue / this.shipController.fuelValueMax;
		this.garbageBar.fillAmount = (float)this.shipController.garbageCount / (float)this.shipController.garbageCountMax;
	}

	public Image fuelBar;

	public Image garbageBar;

	public ShipController shipController;
}
