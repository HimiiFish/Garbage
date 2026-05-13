using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlackHole : MonoBehaviour
{
	[Header("黑洞本体运动")]
	[SerializeField]
	private float minOrbitRadius;

	[SerializeField]
	private float maxOrbitRadius;

	[SerializeField]
	[Range(0.05f, 1f)]
	[Tooltip("黑洞绕行星公转的角速度倍率；越小越慢。")]
	private float holeOrbitAngularScale = 0.28f;

	[Header("引力范围 = 触发器圆半径")]
	[Tooltip("与同物体上 CircleCollider2D.radius 一致（Scale 为 1 时即世界单位）。飞船碰撞体进入该圆：触发 OnTrigger，开始吸引与 isInHole。")]
	[SerializeField]
	private float gravityRadius;

	[Header("视界（代码距离判定，非第二个碰撞体）")]
	[Tooltip("飞船 transform.position 与黑洞中心距离 ≤ 此值时 GameOver。应小于引力半径，通常与子物体 DeadLine 的碰撞范围接近。")]
	[SerializeField]
	private float eventHorizonRadius = 0.45f;

	[Header("吸引：靠近中心时略强，边缘较弱（世界单位/秒量级）")]
	[SerializeField]
	private float attractionPullSpeed = 0.55f;

	[Header("运行时视觉（代码生成子物体）")]
	[SerializeField]
	private bool enableRuntimeVisualFx = true;

	[SerializeField]
	[Range(16, 128)]
	private int accretionRingSegments = 72;

	public Plant plant;

	private Transform _fxRoot;

	private LineRenderer _accretionRingLine;

	private ParticleSystem _diskDustParticles;

	private Material _lineMaterial;

	private Material _particleMaterial;

	private Vector3 _centerPoint;

	private Vector3 _orbitRadius;

	private float _targetOrbitRadius;

	private float _orbitSpeed;

	private readonly float _lerpSpeed = 0.1f;

	private CircleCollider2D _zoneCollider;

	private void Awake()
	{
		this._zoneCollider = base.GetComponent<CircleCollider2D>();
		if (this._zoneCollider != null)
		{
			this._zoneCollider.isTrigger = true;
			this._zoneCollider.radius = this.gravityRadius;
		}
		if (this.enableRuntimeVisualFx)
		{
			this.SetupRuntimeVisualFx();
		}
	}

	private void OnDestroy()
	{
		if (this._lineMaterial != null)
		{
			Object.Destroy(this._lineMaterial);
			this._lineMaterial = null;
		}
		if (this._particleMaterial != null)
		{
			Object.Destroy(this._particleMaterial);
			this._particleMaterial = null;
		}
	}

	private void Start()
	{
		this._centerPoint = new Vector3(0f, 0f, 0f);
		this._targetOrbitRadius = this.minOrbitRadius;
		this._orbitRadius = new Vector3(Random.Range(this.minOrbitRadius, this.maxOrbitRadius), 0f, 0f);
	}

	private void Update()
	{
		this.ComputeOrbitSpeed();
		this.HoleRotate();
		this.TickVisualFx();
	}

	private void HoleRotate()
	{
		if (Random.Range(0, 100) < 1)
		{
			this._targetOrbitRadius = Random.Range(this.minOrbitRadius, this.maxOrbitRadius);
		}
		float d = Mathf.Lerp(this._orbitRadius.magnitude, this._targetOrbitRadius, Time.deltaTime * this._lerpSpeed);
		this._orbitRadius = (Quaternion.AngleAxis(Time.deltaTime * this._orbitSpeed * this.holeOrbitAngularScale, Vector3.forward) * this._orbitRadius).normalized * d;
		base.transform.position = this._centerPoint + this._orbitRadius;
	}

	private void ComputeOrbitSpeed()
	{
		this._orbitSpeed = Mathf.Sqrt(this.plant.gravityCoefficient * 3f / this._orbitRadius.magnitude);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Ship"))
		{
			return;
		}
		StaticData.isInHole = true;
		StaticData.blackHoleWorldPosition = base.transform.position;
		StaticData.blackHoleGravityRadius = this.gravityRadius;
		StaticData.blackHolePullSpeed = this.attractionPullSpeed;
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (!other.CompareTag("Ship"))
		{
			return;
		}
		ShipController ship = other.GetComponent<ShipController>();
		if (ship == null || ship.IsDockedInStation || ship.isOver)
		{
			return;
		}
		Vector3 holePos = base.transform.position;
		Vector3 shipPos = other.transform.position;
		Vector3 offset = shipPos - holePos;
		float dist = offset.magnitude;
		StaticData.coefficient = dist / Mathf.Max(this.gravityRadius, 0.01f);
		StaticData.blackHoleWorldPosition = holePos;
		StaticData.blackHoleGravityRadius = this.gravityRadius;
		StaticData.blackHolePullSpeed = this.attractionPullSpeed;
		if (dist <= this.eventHorizonRadius)
		{
			ship.isOver = true;
			MessageBroker.Default.Publish<GameOverMessage>(new GameOverMessage());
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (!other.CompareTag("Ship"))
		{
			return;
		}
		StaticData.isInHole = false;
	}

	private void SetupRuntimeVisualFx()
	{
		Transform existing = base.transform.Find("BlackHoleFX");
		if (existing != null)
		{
			this._fxRoot = existing;
			this._accretionRingLine = existing.GetComponentInChildren<LineRenderer>();
			this._diskDustParticles = existing.GetComponentInChildren<ParticleSystem>();
			this.RefreshFxLayout();
			return;
		}
		GameObject fx = new GameObject("BlackHoleFX");
		fx.transform.SetParent(base.transform, false);
		fx.transform.localPosition = Vector3.zero;
		fx.transform.localRotation = Quaternion.identity;
		this._fxRoot = fx.transform;
		GameObject ringGo = new GameObject("AccretionRing");
		ringGo.transform.SetParent(this._fxRoot, false);
		this._accretionRingLine = ringGo.AddComponent<LineRenderer>();
		this._accretionRingLine.loop = true;
		this._accretionRingLine.useWorldSpace = false;
		this._accretionRingLine.startWidth = 0.12f;
		this._accretionRingLine.endWidth = 0.05f;
		this._accretionRingLine.numCornerVertices = 4;
		this._accretionRingLine.numCapVertices = 2;
		Shader lineShader = Shader.Find("Sprites/Default");
		if (lineShader == null || !lineShader.isSupported)
		{
			lineShader = Shader.Find("Unlit/Color");
		}
		this._lineMaterial = new Material(lineShader);
		this._lineMaterial.color = new Color(0.5f, 0.2f, 0.95f, 0.5f);
		this._accretionRingLine.material = this._lineMaterial;
		this._accretionRingLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		this._accretionRingLine.receiveShadows = false;
		SpriteRenderer bodySr = base.GetComponent<SpriteRenderer>();
		if (bodySr != null)
		{
			this._accretionRingLine.sortingLayerID = bodySr.sortingLayerID;
			this._accretionRingLine.sortingOrder = bodySr.sortingOrder + 1;
		}
		GameObject dustGo = new GameObject("DiskDust");
		dustGo.transform.SetParent(this._fxRoot, false);
		this._diskDustParticles = dustGo.AddComponent<ParticleSystem>();
		this._diskDustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		ParticleSystem.MainModule main = this._diskDustParticles.main;
		main.playOnAwake = false;
		main.loop = true;
		main.duration = 5f;
		main.prewarm = true;
		main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.4f);
		main.startSpeed = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
		main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
		main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.2831855f);
		main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.45f, 0.75f, 1f, 0.45f), new Color(0.95f, 0.4f, 1f, 0.35f));
		main.gravityModifier = 0f;
		main.simulationSpace = ParticleSystemSimulationSpace.Local;
		main.maxParticles = 220;
		ParticleSystem.EmissionModule emission = this._diskDustParticles.emission;
		emission.rateOverTime = 40f;
		ParticleSystem.ShapeModule shape = this._diskDustParticles.shape;
		shape.enabled = true;
		shape.shapeType = ParticleSystemShapeType.Circle;
		shape.radius = this.gravityRadius * 0.55f;
		shape.randomDirectionAmount = 0.12f;
		ParticleSystem.VelocityOverLifetimeModule vol = this._diskDustParticles.velocityOverLifetime;
		vol.enabled = true;
		vol.space = ParticleSystemSimulationSpace.Local;
		vol.radial = new ParticleSystem.MinMaxCurve(-0.38f, -0.1f);
		ParticleSystem.RotationOverLifetimeModule rol = this._diskDustParticles.rotationOverLifetime;
		rol.enabled = true;
		rol.z = new ParticleSystem.MinMaxCurve(1f, 2.5f);
		ParticleSystem.SizeOverLifetimeModule sol = this._diskDustParticles.sizeOverLifetime;
		sol.enabled = true;
		AnimationCurve sz = new AnimationCurve();
		sz.AddKey(0f, 0.35f);
		sz.AddKey(0.45f, 1f);
		sz.AddKey(1f, 0.08f);
		sol.size = new ParticleSystem.MinMaxCurve(1f, sz);
		ParticleSystemRenderer psr = this._diskDustParticles.GetComponent<ParticleSystemRenderer>();
		psr.renderMode = ParticleSystemRenderMode.Billboard;
		Shader pShader = Shader.Find("Particles/Standard Unlit");
		if (pShader == null || !pShader.isSupported)
		{
			pShader = Shader.Find("Particles/Alpha Blended");
		}
		if (pShader != null && pShader.isSupported)
		{
			this._particleMaterial = new Material(pShader);
			psr.material = this._particleMaterial;
		}
		if (bodySr != null)
		{
			psr.sortingLayerID = bodySr.sortingLayerID;
			psr.sortingOrder = bodySr.sortingOrder + 2;
		}
		this._diskDustParticles.Play();
		this.RefreshFxLayout();
	}

	private void RefreshFxLayout()
	{
		this.RefreshRingGeometry();
		if (this._diskDustParticles != null)
		{
			ParticleSystem.ShapeModule shape = this._diskDustParticles.shape;
			shape.radius = Mathf.Max(0.05f, this.gravityRadius * 0.58f);
		}
	}

	private void RefreshRingGeometry()
	{
		if (this._accretionRingLine == null)
		{
			return;
		}
		int n = Mathf.Clamp(this.accretionRingSegments, 16, 128);
		if (this._accretionRingLine.positionCount != n)
		{
			this._accretionRingLine.positionCount = n;
		}
		float r = Mathf.Max(0.05f, this.gravityRadius * 0.88f);
		for (int i = 0; i < n; i++)
		{
			float ang = (float)i / (float)n * 6.2831855f;
			this._accretionRingLine.SetPosition(i, new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, -0.02f));
		}
	}

	private void TickVisualFx()
	{
		if (!this.enableRuntimeVisualFx || this._fxRoot == null)
		{
			return;
		}
		this._fxRoot.Rotate(0f, 0f, 24f * Time.deltaTime);
		this.RefreshRingGeometry();
		if (this._diskDustParticles != null)
		{
			ParticleSystem.EmissionModule emission = this._diskDustParticles.emission;
			float boost = StaticData.isInHole ? 1.8f : 1f;
			emission.rateOverTime = 40f * boost;
		}
	}

	private void OnValidate()
	{
		this._zoneCollider = this._zoneCollider != null ? this._zoneCollider : base.GetComponent<CircleCollider2D>();
		if (this._zoneCollider != null)
		{
			this._zoneCollider.isTrigger = true;
			this.gravityRadius = Mathf.Max(0.05f, this.gravityRadius);
			this._zoneCollider.radius = this.gravityRadius;
		}
		this.eventHorizonRadius = Mathf.Clamp(this.eventHorizonRadius, 0.05f, Mathf.Max(0.06f, this.gravityRadius - 0.02f));
		if (Application.isPlaying && this.enableRuntimeVisualFx && this._fxRoot != null)
		{
			this.RefreshFxLayout();
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Vector3 c = base.transform.position;
		float g = Mathf.Max(0.01f, this.gravityRadius);
		float h = Mathf.Max(0.01f, this.eventHorizonRadius);
		UnityEditor.Handles.color = new Color(0f, 0.85f, 1f, 0.75f);
		UnityEditor.Handles.DrawWireDisc(c, Vector3.forward, g);
		UnityEditor.Handles.color = new Color(1f, 0.35f, 0.15f, 0.9f);
		UnityEditor.Handles.DrawWireDisc(c, Vector3.forward, h);
		UnityEditor.Handles.color = new Color(0.7f, 0.95f, 1f, 0.85f);
		UnityEditor.Handles.Label(c + Vector3.up * (g + 0.35f), "引力触发半径 = " + g.ToString("F2"));
		UnityEditor.Handles.Label(c + Vector3.right * (g + 0.35f), "视界距离 ≤ " + h.ToString("F2"));
	}
#endif
}
