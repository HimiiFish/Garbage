using System;
using UnityEngine;

// Token: 0x02000015 RID: 21
public class LoadText : MonoBehaviour
{
	// Token: 0x06000048 RID: 72 RVA: 0x000031A8 File Offset: 0x000013A8
	private void Awake()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("Text/TestText");
		Debug.Log(textAsset);
		StaticData.strings = textAsset.text.Split('\n', StringSplitOptions.None);
	}
}
