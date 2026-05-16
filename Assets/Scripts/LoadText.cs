using System;
using UnityEngine;

public class LoadText : MonoBehaviour
{
	private void Awake()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("Text/TestText");
		Debug.Log(textAsset);
		StaticData.strings = textAsset.text.Split('\n', StringSplitOptions.None);
	}
}
