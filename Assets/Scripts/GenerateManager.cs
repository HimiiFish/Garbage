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

	private void Start()
	{
		this._lifetimeSpawnCount = 0;
		this._spawnCheckTimer = 0f;
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			generateList.currentGarbageCount = 0;
		}
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
			ro.enabled = true;
			ro.ClearGarbageOrbitalDecay();
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
		gameObject.SetActive(true);
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
