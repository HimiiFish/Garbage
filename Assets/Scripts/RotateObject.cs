using UnityEngine;

public class RotateObject : MonoBehaviour
{
	[SerializeField]
	[Tooltip("垃圾启用后先纯公转、再开启径向沉降，避免开局螺旋像倒转")]
	private float garbageOrbitSettleSeconds = 0.4f;

	private float _garbageOrbitActiveTime;

	private void Awake()
	{
		this.EnsurePlantReference();
	}

	private void OnEnable()
	{
		if (!base.gameObject.CompareTag("Garbage"))
		{
			return;
		}
		this._garbageOrbitActiveTime = 0f;
		this._garbageDecaySpeed = 0f;
		this.ResetOrbitStateFromWorld();
	}

	private void Update()
	{
		if (!StaticData.IsOrbitalMotionActive && (base.gameObject.CompareTag("SpaceStation") || base.gameObject.CompareTag("Garbage")))
		{
			return;
		}
		if (base.gameObject.CompareTag("SpaceStation"))
		{
			this.HoleRotate();
			this.ShipFace();
			return;
		}
		if (base.gameObject.CompareTag("Garbage"))
		{
			this._garbageOrbitActiveTime += Time.deltaTime;
		}
		this.ComputeOrbitSpeed();
		if (base.gameObject.CompareTag("Garbage") && this._garbageDecaySpeed > 0f && this._garbageOrbitActiveTime >= this.garbageOrbitSettleSeconds)
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
		float r = Mathf.Max(0.5f, this._fixedSpaceStationOrbitRadius);
		this.orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient / r);
		float angleStep = StaticData.OrbitAngularDirectionSign * Time.deltaTime * this.orbitSpeed * this.spaceStationOrbitVisualScale;
		this.orbitRadius = (Quaternion.AngleAxis(angleStep, Vector3.forward) * this.orbitRadius).normalized * r;
		base.transform.position = this._centerPoint + this.orbitRadius;
	}

	private void ShipFace()
	{
		if (base.gameObject.CompareTag("Garbage"))
		{
			this.FaceGarbageAlongMotion();
			return;
		}
		base.transform.rotation = Quaternion.LookRotation(Vector3.forward, this._centerPoint - base.transform.position);
	}

	private void FaceGarbageAlongMotion()
	{
		if (this.orbitRadius.sqrMagnitude < 1e-8f)
		{
			return;
		}
		Vector3 radial = this.orbitRadius.normalized;
		Vector3 tangential = Vector3.Cross(Vector3.forward * StaticData.OrbitAngularDirectionSign, radial);
		base.transform.rotation = Quaternion.LookRotation(Vector3.forward, tangential);
	}

	private void Start()
	{
		this.EnsurePlantReference();
		this.ResetOrbitStateFromWorld();
		if (base.gameObject.CompareTag("SpaceStation"))
		{
			this._fixedSpaceStationOrbitRadius = this.orbitRadius.magnitude;
			if (this._fixedSpaceStationOrbitRadius < 0.01f)
			{
				this._fixedSpaceStationOrbitRadius = Mathf.Clamp((this.minOrbitRadius + this.maxOrbitRadius) * 0.5f, this.minOrbitRadius, this.maxOrbitRadius);
			}
		}
	}

	public void PrepareForPool()
	{
		this.ClearGarbageOrbitalDecay();
		this._garbageOrbitActiveTime = 0f;
		this._radiusSmoothVel = 0f;
		this.orbitRadius = Vector3.zero;
		this.orbitSpeed = 0f;
	}

	public void ResetOrbitStateFromWorld()
	{
		this.EnsurePlantReference();
		this._centerPoint = this.plant != null ? this.plant.transform.position : Vector3.zero;
		Vector3 offset = base.transform.position - this._centerPoint;
		if (offset.sqrMagnitude < 0.0001f)
		{
			offset = Vector3.right * Mathf.Max(0.5f, this.minOrbitRadius);
		}
		this.orbitRadius = offset;
		this._radiusSmoothVel = 0f;
		this._targetOrbitRadius = this.orbitRadius.magnitude;
		base.transform.position = this._centerPoint + this.orbitRadius;
		if (base.gameObject.CompareTag("Garbage"))
		{
			this.FaceGarbageAlongMotion();
		}
		if (base.gameObject.CompareTag("SpaceStation"))
		{
			this._fixedSpaceStationOrbitRadius = Mathf.Max(0.5f, this.orbitRadius.magnitude);
		}
	}

	private void EnsurePlantReference()
	{
		if (this.plant != null)
		{
			return;
		}
		GameObject p = GameObject.Find("Planet");
		if (p != null)
		{
			this.plant = p.GetComponent<Plant>();
		}
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
		if (base.gameObject.CompareTag("Garbage") && this.orbitRadius.sqrMagnitude < 0.0001f)
		{
			this.ResetOrbitStateFromWorld();
		}
		float angularMul = 2f;
		if (base.gameObject.CompareTag("Garbage"))
		{
			Garabage garabage = base.GetComponent<Garabage>();
			if (garabage != null)
			{
				angularMul = garabage.OrbitAngularFactor;
			}
			else
			{
				angularMul = 1.1f;
			}
		}
		float angle = StaticData.OrbitAngularDirectionSign * Time.deltaTime * this.orbitSpeed * angularMul;
		this.orbitRadius = Quaternion.AngleAxis(angle, Vector3.forward) * this.orbitRadius;
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

	[Tooltip("空间站已改为固定轨道半径（取场景初始位置）；下列两项仅作备用初值，不再参与随机变轨。")]
	public float minOrbitRadius = 4f;

	[Tooltip("备用：若场景空间站恰在原点时使用")]
	public float maxOrbitRadius = 18f;

	public float lerpSpeed = 0.1f;

	private float _targetOrbitRadius;

	private float _fixedSpaceStationOrbitRadius;

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
