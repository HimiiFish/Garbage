using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UniRx;
using UnityEngine;

// Token: 0x0200001D RID: 29
public class SpaceStation : MonoBehaviour
{
	// Token: 0x06000072 RID: 114 RVA: 0x000040D1 File Offset: 0x000022D1
	private void Start()
	{
		this.InitCamera();
		this.a = MessageBroker.Default.Receive<StartGameMessage>().Subscribe(delegate(StartGameMessage _)
		{
			this.Launch();
			AudioManager.Instance.Play("启动");
			this.a.Dispose();
		});
	}

	// Token: 0x06000073 RID: 115 RVA: 0x000040FA File Offset: 0x000022FA
	private void SpaceStationLogic()
	{
	}

	// Token: 0x06000074 RID: 116 RVA: 0x000040FC File Offset: 0x000022FC
	private void Update()
	{
		if (Input.GetKey(KeyCode.U))
		{
			Time.timeScale = 10f;
			return;
		}
		if (Input.GetKeyUp(KeyCode.U))
		{
			Time.timeScale = 1f;
		}
	}

	// Token: 0x06000075 RID: 117 RVA: 0x00004128 File Offset: 0x00002328
	public void Launch()
	{
		this.ship.SetActive(true);
		this.ship.GetComponent<ShipController>().enabled = false;
		this.ship.transform.position = base.transform.position;
		base.StartCoroutine(this.LaunchCoroutine());
	}

	// Token: 0x06000076 RID: 118 RVA: 0x0000417A File Offset: 0x0000237A
	private IEnumerator LaunchCoroutine()
	{
		this.orbitRadius = base.transform.position - this.plant.transform.position;
		float time = 0f;
		while (time < 1.5f)
		{
			this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient / this.orbitRadius.magnitude);
			time += Time.deltaTime;
			this.acceleration += this.leap * Time.deltaTime;
			this.orbitRadius = Vector3.MoveTowards(this.orbitRadius, this.orbitRadius.normalized * (this.orbitRadius.magnitude + this.changeCoefficient), Time.deltaTime * this.acceleration);
			this.orbitRadius = Quaternion.AngleAxis(Time.deltaTime * this.orbitSpeed, Vector3.forward) * this.orbitRadius;
			this.ship.transform.position = this.plant.transform.position + this.orbitRadius;
			float endValue = this.orbitRadius.magnitude * 1.2f;
			Camera.main.DOOrthoSize(endValue, 0.05f).SetEase(Ease.InOutSine);
			yield return null;
		}
		this.ship.GetComponent<ShipController>().enabled = true;
		yield break;
	}

	// Token: 0x06000077 RID: 119 RVA: 0x00004189 File Offset: 0x00002389
	private void InitCamera()
	{
		Camera.main.DOOrthoSize(13f, 30f).SetEase(Ease.InOutSine).OnComplete(delegate
		{
			Camera.main.transform.parent = null;
			Camera.main.transform.DOMove(new Vector3(0f, 0f, -10f), 3f, false).OnComplete(delegate
			{
				this.isBackGroundOver = true;
				float endValue = 20f;
				Camera.main.DOOrthoSize(endValue, 1f).SetEase(Ease.InOutSine);
			});
		});
	}

	// Token: 0x0400005B RID: 91
	public GameObject ship;

	// Token: 0x0400005C RID: 92
	private CinemachineVirtualCamera vcam;

	// Token: 0x0400005D RID: 93
	private IDisposable a;

	// Token: 0x0400005E RID: 94
	private Vector3 orbitRadius;

	// Token: 0x0400005F RID: 95
	private float orbitSpeed;

	// Token: 0x04000060 RID: 96
	private float acceleration;

	// Token: 0x04000061 RID: 97
	private float maxAcceleration = 1.5f;

	// Token: 0x04000062 RID: 98
	private float leap = 2f;

	// Token: 0x04000063 RID: 99
	private float changeCoefficient = 0.3f;

	// Token: 0x04000064 RID: 100
	private float gravityCoefficient = 50000f;

	// Token: 0x04000065 RID: 101
	public Plant plant;

	// Token: 0x04000066 RID: 102
	public bool isBackGroundOver;
}
