using System;
using UnityEngine;
using Random = UnityEngine.Random;

// Token: 0x02000007 RID: 7
public class Garabage : MonoBehaviour
{
	// Token: 0x06000011 RID: 17 RVA: 0x00002289 File Offset: 0x00000489
	private void Awake()
	{
		this.spriteRenderer = base.GetComponent<SpriteRenderer>();
	}

	// Token: 0x06000012 RID: 18 RVA: 0x00002298 File Offset: 0x00000498
	private void Start()
	{
		int num = Random.Range(2, 22);
		this.spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/废物/图层 " + num.ToString());
	}

	// Token: 0x0400000A RID: 10
	public string GarbageName;

	// Token: 0x0400000B RID: 11
	public int GarbageValue;

	// Token: 0x0400000C RID: 12
	private SpriteRenderer spriteRenderer;
}
