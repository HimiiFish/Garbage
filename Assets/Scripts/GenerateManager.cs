using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateManager : MonoBehaviour
{
	public static GenerateManager Instance { get; private set; }

	private int _lifetimeSpawnCount;

	private float _spawnCheckTimer;

	private readonly Dictionary<GameObject, Queue<GameObject>> _poolByPrefab = new Dictionary<GameObject, Queue<GameObject>>();

	private Transform _poolRoot;

	private int GetLifetimeSpawnCap()
	{
		if (this.generateData == null)
		{
			return 0;
		}
		if (this.generateData.allowInfiniteRefill)
		{
			return int.MaxValue;
		}
		if (this.generateData.maxLifetimeSpawns > 0)
		{
			return this.generateData.maxLifetimeSpawns;
		}
		int sum = 0;
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			sum += generateList.maxGarbageCount;
		}
		return Mathf.Max(sum, 1);
	}

	private bool CanSpawnLifetime()
	{
		return this._lifetimeSpawnCount < this.GetLifetimeSpawnCap();
	}

	private void Awake()
	{
		Instance = this;
		GameObject root = new GameObject("PooledGarbageRoot");
		root.transform.SetParent(base.transform, false);
		this._poolRoot = root.transform;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void ApplyCampaignPhase(GameCampaignPhase phase, bool resetCounters)
	{
		if (resetCounters)
		{
			this.ResetSpawnCounters();
		}
	}

	public void ResetSpawnCounters()
	{
		this._lifetimeSpawnCount = 0;
		this._spawnCheckTimer = 0f;
		if (this.generateData == null)
		{
			return;
		}
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			generateList.currentGarbageCount = 0;
		}
	}

	public void BeginNewOutingSpawn()
	{
		this.ResetSpawnCounters();
		this.FillFieldToCap();
	}

	public void ClearAllActiveGarbage()
	{
		GameObject[] active = GameObject.FindGameObjectsWithTag("Garbage");
		for (int i = 0; i < active.Length; i++)
		{
			if (active[i] != null)
			{
				this.ReleaseGarbage(active[i]);
			}
		}
		this.ResetSpawnCounters();
	}

	private void FillFieldToCap()
	{
		if (this.generateData == null)
		{
			return;
		}
		int guard = 0;
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			while (generateList.currentGarbageCount < generateList.maxGarbageCount && this.CanSpawnLifetime() && guard < 64)
			{
				if (!this.TrySpawnOne(generateList))
				{
					break;
				}
				guard++;
			}
		}
	}

	private void Start()
	{
		this.ResetSpawnCounters();
		MessageBroker.Default.Receive<GenerateGarbageMessage>().Subscribe(delegate(GenerateGarbageMessage _)
		{
			this.GenerateTenGarbage();
		}).AddTo(this);
		MessageBroker.Default.Receive<GarbageCollectedMessage>().Subscribe(delegate(GarbageCollectedMessage _)
		{
			foreach (GenerateList generateList in this.generateData.generateLists)
			{
				if (generateList.currentGarbageCount > 0)
				{
					generateList.currentGarbageCount--;
					break;
				}
			}
		}).AddTo(this);
	}

	private void Update()
	{
		if (!StaticData.IsOrbitalMotionActive)
		{
			return;
		}
		if (CampaignManager.Instance != null && !CampaignManager.IsEndlessMode() && this._lifetimeSpawnCount >= this.GetLifetimeSpawnCap())
		{
			return;
		}
		if (!this.CanSpawnLifetime())
		{
			return;
		}
		float interval = this.generateData != null ? Mathf.Max(0.05f, this.generateData.spawnCheckInterval) : 0.45f;
		this._spawnCheckTimer += Time.deltaTime;
		if (this._spawnCheckTimer < interval)
		{
			return;
		}
		this._spawnCheckTimer = 0f;
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			if (generateList.currentGarbageCount < generateList.maxGarbageCount && this.CanSpawnLifetime())
			{
				this.TrySpawnOne(generateList);
			}
		}
	}

	public void GenerateTenGarbage()
	{
		if (!this.CanSpawnLifetime())
		{
			return;
		}
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			for (int i = 0; i < 1; i++)
			{
				if (!this.CanSpawnLifetime())
				{
					return;
				}
				if ((float)Random.Range(0, 100) < 100f * (generateList.RangeMax - Vector3.Distance(Vector3.zero, base.transform.position)) / Mathf.Max(generateList.RangeMax, 0.01f))
				{
					this.TrySpawnOne(generateList);
				}
			}
		}
	}

	public void GenerateGarbage()
	{
		if (!this.CanSpawnLifetime())
		{
			return;
		}
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			if (generateList.currentGarbageCount < generateList.maxGarbageCount && this.CanSpawnLifetime())
			{
				this.TrySpawnOne(generateList);
			}
		}
	}

	private GameObject RentFromPool(GameObject prefab)
	{
		if (!this._poolByPrefab.TryGetValue(prefab, out Queue<GameObject> queue))
		{
			queue = new Queue<GameObject>();
			this._poolByPrefab[prefab] = queue;
		}
		if (queue.Count > 0)
		{
			return queue.Dequeue();
		}
		GameObject created = UnityEngine.Object.Instantiate(prefab, this._poolRoot);
		created.SetActive(false);
		Garabage g = created.GetComponent<Garabage>();
		if (g != null)
		{
			g.AssignPoolPrefabKey(prefab);
		}
		return created;
	}

	public void ReleaseGarbage(GameObject instance)
	{
		if (instance == null)
		{
			return;
		}
		instance.transform.DOKill(true);
		instance.transform.SetParent(this._poolRoot, false);
		foreach (Collider2D c in instance.GetComponentsInChildren<Collider2D>(true))
		{
			c.enabled = true;
		}
		RotateObject ro = instance.GetComponent<RotateObject>();
		if (ro != null)
		{
			ro.PrepareForPool();
			ro.enabled = true;
		}
		Garabage gb = instance.GetComponent<Garabage>();
		GameObject prefabKey = gb != null ? gb.PoolPrefabKey : null;
		instance.SetActive(false);
		if (prefabKey == null)
		{
			UnityEngine.Object.Destroy(instance);
			return;
		}
		if (!this._poolByPrefab.TryGetValue(prefabKey, out Queue<GameObject> queue))
		{
			queue = new Queue<GameObject>();
			this._poolByPrefab[prefabKey] = queue;
		}
		queue.Enqueue(instance);
	}

	private bool TrySpawnOne(GenerateList generateList)
	{
		if (generateList.currentGarbageCount >= generateList.maxGarbageCount || !this.CanSpawnLifetime())
		{
			return false;
		}
		float prob = 100f * (generateList.RangeMax - Vector3.Distance(Vector3.zero, base.transform.position)) / Mathf.Max(generateList.RangeMax, 0.01f);
		if ((float)Random.Range(0, 100) >= prob)
		{
			return false;
		}
		float rMin = this.generateData.garbageSpawnRadiusMin;
		float rMax = this.generateData.garbageSpawnRadiusMax;
		if (rMax < rMin)
		{
			float t = rMin;
			rMin = rMax;
			rMax = t;
		}
		float shipCap = Mathf.Max(0.5f, this.generateData.shipMaxOrbitRadiusForGarbageLayout);
		float margin = Mathf.Max(0f, this.generateData.garbageSpawnMinMarginAboveShipOrbit);
		float rMinFromShip = shipCap + margin;
		rMin = Mathf.Max(rMin, rMinFromShip);
		rMax = Mathf.Max(rMax, rMin + 0.5f);
		Vector2 dir2 = Random.insideUnitCircle;
		if (dir2.sqrMagnitude < 0.0001f)
		{
			dir2 = Vector2.right;
		}
		dir2.Normalize();
		float radius = Random.Range(rMin, rMax);
		Vector3 spawnPos = new Vector3(dir2.x * radius, dir2.y * radius, 0f);
		GameObject gameObject = this.RentFromPool(generateList.Garbage);
		gameObject.transform.SetParent(null, true);
		gameObject.transform.position = spawnPos;
		Garabage garabage = gameObject.GetComponent<Garabage>();
		if (garabage != null)
		{
			if (garabage.PoolPrefabKey == null)
			{
				garabage.AssignPoolPrefabKey(generateList.Garbage);
			}
			garabage.ApplySpawnRandomization(this.generateData);
		}
		RotateObject rotateObject = gameObject.GetComponent<RotateObject>();
		if (rotateObject != null)
		{
			rotateObject.ResetOrbitStateFromWorld();
		}
		gameObject.SetActive(true);
		if (rotateObject != null)
		{
			float decaySpeed = Random.Range(this.generateData.garbageDecaySpeedMin, this.generateData.garbageDecaySpeedMax);
			rotateObject.SetGarbageOrbitalDecay(decaySpeed, this.generateData.garbageDecayRadiusFloor);
		}
		generateList.currentGarbageCount++;
		this._lifetimeSpawnCount++;
		return true;
	}

	[SerializeField]
	private GenerateData generateData;
}
