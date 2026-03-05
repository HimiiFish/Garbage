using System;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// Token: 0x02000012 RID: 18
public class GenerateManager : MonoBehaviour
{
	// Token: 0x06000038 RID: 56 RVA: 0x00002BB4 File Offset: 0x00000DB4
	private void Start()
	{
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			generateList.currentGarbageCount = 0;
		}
		MessageBroker.Default.Receive<GenerateGarbageMessage>().Subscribe(delegate(GenerateGarbageMessage _)
		{
			this.GenerateTenGarbage();
		}).AddTo(this);
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00002C2C File Offset: 0x00000E2C
	private void Update()
	{
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			if (generateList.currentGarbageCount < generateList.maxGarbageCount)
			{
				this.GenerateGarbage();
			}
		}
	}

	// Token: 0x0600003A RID: 58 RVA: 0x00002C94 File Offset: 0x00000E94
	public void GenerateTenGarbage()
	{
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			for (int i = 0; i < 1; i++)
			{
				if ((float)Random.Range(0, 100) < 100f * (generateList.RangeMax - Vector3.Distance(Vector3.zero, base.transform.position)) / generateList.RangeMax)
				{
					GameObject gameObject = Object.Instantiate<GameObject>(generateList.Garbage, new Vector3(Random.Range(-generateList.RangeMax, generateList.RangeMax), Random.Range(-generateList.RangeMax, generateList.RangeMax), 0f), Quaternion.identity);
					generateList.currentGarbageCount++;
					if (Vector3.Distance(Vector3.zero, gameObject.transform.position) < 20f)
					{
						gameObject.transform.position = gameObject.transform.position.normalized * 20f;
					}
				}
			}
		}
	}

	// Token: 0x0600003B RID: 59 RVA: 0x00002DC8 File Offset: 0x00000FC8
	public void GenerateGarbage()
	{
		foreach (GenerateList generateList in this.generateData.generateLists)
		{
			while (generateList.currentGarbageCount < generateList.maxGarbageCount)
			{
				if ((float)Random.Range(0, 100) < 100f * (generateList.RangeMax - Vector3.Distance(Vector3.zero, base.transform.position)) / generateList.RangeMax)
				{
					GameObject gameObject = Object.Instantiate<GameObject>(generateList.Garbage, new Vector3(Random.Range(-generateList.RangeMax, generateList.RangeMax), Random.Range(-generateList.RangeMax, generateList.RangeMax), 0f), Quaternion.identity);
					generateList.currentGarbageCount++;
					if (Vector3.Distance(Vector3.zero, gameObject.transform.position) < 20f)
					{
						gameObject.transform.position = gameObject.transform.position.normalized * 20f;
					}
				}
			}
		}
	}

	// Token: 0x0400002B RID: 43
	[SerializeField]
	private GenerateData generateData;
}
