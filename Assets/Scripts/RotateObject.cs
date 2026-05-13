using System;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class RotateObject : MonoBehaviour
{
	private void Awake()
	{
		if (this.plant == null)
		{
			GameObject p = GameObject.Find("Planet");
			if (p != null)
			{
				this.plant = p.GetComponent<Plant>();
			}
		}
	}

	private void Update()
	{
		if (base.gameObject.CompareTag("SpaceStation"))
		{
			this.HoleRotate();
			this.ShipFace();
			return;
		}
		this.ComputeOrbitSpeed();
		if (base.gameObject.CompareTag("Garbage") && this._garbageDecaySpeed > 0f)
		{
			float mag = this.orbitRadius.magnitude;
			if (mag > 0.001f)
			{
				float newMag = Mathf.MoveTowards(mag, this._garbageDecayRadiusFloor, this._garbageDecaySpeed * Time.deltaTime);
				this.orbitRadius = this.orbitRadius.normalized * newMag;
			}
		}
		this.ShipRotate();
		this.ShipFace();
	}

	private void HoleRotate()
	{
		if (this.plant == null)
		{
			return;
		}
		float currentMag = this.orbitRadius.magnitude;
		if (currentMag < 0.001f)
		{
			currentMag = this.minOrbitRadius;
		}
		this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient / currentMag);
		float smoothedMag = Mathf.SmoothDamp(currentMag, this._targetOrbitRadius, ref this._radiusSmoothVel, this.radiusSmoothTime, Mathf.Infinity, Time.deltaTime);
		smoothedMag = Mathf.Clamp(smoothedMag, this.minOrbitRadius, this.maxOrbitRadius);
		float angleStep = Time.deltaTime * this.orbitSpeed * this.spaceStationOrbitVisualScale;
		this.orbitRadius = (Quaternion.AngleAxis(angleStep, Vector3.forward) * this.orbitRadius).normalized * smoothedMag;
		base.transform.position = this._centerPoint + this.orbitRadius;
	}

	private void ShipFace()
	{
		base.transform.rotation = Quaternion.LookRotation(Vector3.forward, this._centerPoint - base.transform.position);
	}

	private void Start()
	{
		this.plant = GameObject.Find("Planet").GetComponent<Plant>();
		this._centerPoint = this.plant != null ? this.plant.transform.position : Vector3.zero;
		this.orbitRadius = base.transform.position - this._centerPoint;
		this._targetOrbitRadius = this.orbitRadius.magnitude;
		if (base.gameObject.CompareTag("SpaceStation"))
		{
			Observable.Timer(TimeSpan.FromSeconds(10.0)).Repeat<long>().Subscribe(delegate(long _)
			{
				this._targetOrbitRadius = Random.Range(this.minOrbitRadius, this.maxOrbitRadius);
			}).AddTo(this);
		}
	}

	public void ResetOrbitStateFromWorld()
	{
		if (this.plant == null)
		{
			GameObject p = GameObject.Find("Planet");
			if (p != null)
			{
				this.plant = p.GetComponent<Plant>();
			}
		}
		this._centerPoint = this.plant != null ? this.plant.transform.position : Vector3.zero;
		this.orbitRadius = base.transform.position - this._centerPoint;
		this._radiusSmoothVel = 0f;
		this._targetOrbitRadius = this.orbitRadius.magnitude;
	}

	public void SetGarbageOrbitalDecay(float speedWorldUnitsPerSecond, float innerRadiusFloor)
	{
		if (!base.gameObject.CompareTag("Garbage"))
		{
			return;
		}
		this._garbageDecaySpeed = Mathf.Max(0f, speedWorldUnitsPerSecond);
		this._garbageDecayRadiusFloor = Mathf.Max(0.5f, innerRadiusFloor);
	}

	public void ClearGarbageOrbitalDecay()
	{
		this._garbageDecaySpeed = 0f;
	}

	private void ShipRotate()
	{
		if (this.plant == null)
		{
			return;
		}
		this.orbitRadius = Quaternion.AngleAxis(Time.deltaTime * this.orbitSpeed * 2f, Vector3.forward) * this.orbitRadius;
		base.transform.position = this._centerPoint + this.orbitRadius;
	}

	private void ComputeOrbitSpeed()
	{
		if (this.plant == null)
		{
			return;
		}
		this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient / Mathf.Max(this.orbitRadius.magnitude, 0.01f));
	}

	private Plant plant;

	private Vector3 _centerPoint;

	private Vector3 orbitRadius;

	private float orbitSpeed;

	[Tooltip("空间站随机目标半径下限；应明显小于场景中黑洞 minOrbitRadius，否则会与黑洞同高度带重叠。")]
	public float minOrbitRadius = 4f;

	[Tooltip("空间站随机目标半径上限；应小于黑洞轨道带下限，否则定时 Random 会经常抽到与黑洞相近的高度。")]
	public float maxOrbitRadius = 18f;

	public float lerpSpeed = 0.1f;

	private float _targetOrbitRadius;

	private float _radiusSmoothVel;

	private float _garbageDecaySpeed;

	private float _garbageDecayRadiusFloor = 11f;

	[SerializeField]
	[Tooltip("目标轨道半径变化时的平滑时间（秒），数值越大轨迹越顺")]
	private float radiusSmoothTime = 2.8f;

	[SerializeField]
	[Range(0.05f, 1.5f)]
	[Tooltip("空间站绕行星角速度相对飞船公式的比例；原逻辑为 2×orbitSpeed 易比飞船快太多")]
	private float spaceStationOrbitVisualScale = 0.42f;
}
