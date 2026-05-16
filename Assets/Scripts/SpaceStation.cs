using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UniRx;
using UnityEngine;

public class SpaceStation : MonoBehaviour
{
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

	private void SpaceStationLogic()
	{
	}

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

	public void Launch()
	{
		this.ship.SetActive(true);
		ShipController shipController = this.ship.GetComponent<ShipController>();
		shipController.enabled = false;
		shipController.ApplyFlyingVisualState();
		this.ship.transform.position = base.transform.position;
		base.StartCoroutine(this.LaunchCoroutine());
	}

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
			this.orbitRadius = Quaternion.AngleAxis(StaticData.OrbitAngularDirectionSign * Time.deltaTime * this.orbitSpeed, Vector3.forward) * this.orbitRadius;
			this.ship.transform.position = this.plant.transform.position + this.orbitRadius;
			float endValue = this.orbitRadius.magnitude * 1.2f;
			Camera.main.DOOrthoSize(endValue, 0.05f).SetEase(Ease.InOutSine);
			yield return null;
		}
		ShipController shipController = this.ship.GetComponent<ShipController>();
		shipController.ApplyFlyingVisualState();
		shipController.enabled = true;
		Vector3 focus = this.plant != null ? this.plant.transform.position : Vector3.zero;
		StaticData.BeginOrbitalMotion(focus);
		if (CampaignManager.Instance != null)
		{
			CampaignManager.Instance.OnLaunchedFromStation();
		}
		else if (GenerateManager.Instance != null)
		{
			GenerateManager.Instance.BeginNewOutingSpawn();
		}
		yield break;
	}

	private void InitCamera()
	{
		if (StaticData.IsOrbitalMotionActive)
		{
			StaticData.FinalizeIntroCamera();
			return;
		}
		Camera.main.DOOrthoSize(13f, 30f).SetEase(Ease.InOutSine).OnComplete(delegate
		{
			if (StaticData.IsOrbitalMotionActive)
			{
				return;
			}
			Camera.main.transform.parent = null;
			Camera.main.transform.DOMove(new Vector3(0f, 0f, -10f), 3f, false).OnComplete(delegate
			{
				if (StaticData.IsOrbitalMotionActive)
				{
					return;
				}
				this.NotifyIntroCameraFinished();
				float endValue = 20f;
				Camera.main.DOOrthoSize(endValue, 1f).SetEase(Ease.InOutSine);
			});
		});
	}

	public void NotifyIntroCameraFinished()
	{
		this.isBackGroundOver = true;
	}
	
	public GameObject ship;

	private CinemachineVirtualCamera vcam;

	private IDisposable a;

	private Vector3 orbitRadius;

	private float orbitSpeed;

	private float acceleration;

	private float maxAcceleration = 1.5f;

	private float leap = 2f;

	private float changeCoefficient = 0.3f;

	private float gravityCoefficient = 50000f;

	public Plant plant;

	public bool isBackGroundOver;
}
