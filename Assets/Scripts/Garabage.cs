using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Garabage : MonoBehaviour
{
	private static readonly int[] WasteLayerIndicesFallback =
	{
		2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 21
	};

	[SerializeField]
	[Tooltip("若已指定，则只从其中的 Sprite 随机；请在资源里配置需要的少数贴图")]
	private GarbageVisualLibrary visualLibrary;

	private SpriteRenderer spriteRenderer;

	private float _runtimeMass = 1f;

	public float RuntimeMass
	{
		get
		{
			return this._runtimeMass;
		}
	}

	public GameObject PoolPrefabKey { get; private set; }

	public void AssignPoolPrefabKey(GameObject prefab)
	{
		this.PoolPrefabKey = prefab;
	}

	public void ApplySpawnRandomization(GenerateData data)
	{
		if (data == null)
		{
			return;
		}
		this._runtimeMass = Random.Range(data.garbageMassMin, data.garbageMassMax);
		float uniformScale = Random.Range(data.garbageUniformScaleMin, data.garbageUniformScaleMax);
		base.transform.localScale = Vector3.one * uniformScale;
		Rigidbody2D rb = base.GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			rb.mass = this._runtimeMass;
		}
		this.ApplyRandomSpriteOnce();
	}

	private void Awake()
	{
		this.spriteRenderer = base.GetComponent<SpriteRenderer>();
	}

	private static Sprite TryLoadWasteSprite(int layerIndex)
	{
		string pathSuffix = "废物/图层 " + layerIndex;
		Sprite s = Resources.Load<Sprite>("sprites/" + pathSuffix);
		if (s != null)
		{
			return s;
		}
		return Resources.Load<Sprite>("Sprites/" + pathSuffix);
	}

	private static Sprite PickSpriteFromLibrary(GarbageVisualLibrary library)
	{
		if (library == null || library.wasteSprites == null || library.wasteSprites.Length == 0)
		{
			return null;
		}
		List<Sprite> valid = new List<Sprite>();
		for (int i = 0; i < library.wasteSprites.Length; i++)
		{
			Sprite s = library.wasteSprites[i];
			if (s != null)
			{
				valid.Add(s);
			}
		}
		if (valid.Count == 0)
		{
			return null;
		}
		return valid[Random.Range(0, valid.Count)];
	}

	private static Sprite PickSpriteFromResourcesFallback()
	{
		int firstPick = WasteLayerIndicesFallback[Random.Range(0, WasteLayerIndicesFallback.Length)];
		Sprite sprite = TryLoadWasteSprite(firstPick);
		if (sprite == null)
		{
			for (int k = 0; k < WasteLayerIndicesFallback.Length; k++)
			{
				int i = WasteLayerIndicesFallback[k];
				sprite = TryLoadWasteSprite(i);
				if (sprite != null)
				{
					break;
				}
			}
		}
		return sprite;
	}

	private void ApplyRandomSpriteOnce()
	{
		if (this.spriteRenderer == null)
		{
			this.spriteRenderer = base.GetComponent<SpriteRenderer>();
		}
		Sprite sprite = PickSpriteFromLibrary(this.visualLibrary);
		if (sprite == null)
		{
			sprite = PickSpriteFromResourcesFallback();
		}
		if (sprite != null)
		{
			this.spriteRenderer.sprite = sprite;
		}
		else
		{
			Debug.LogWarning("Garabage: 未配置 GarbageVisualLibrary 有效贴图，且 Resources 回退失败。物体名: " + base.gameObject.name);
		}
	}

	public string GarbageName;

	public int GarbageValue;
}
