using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Token: 0x02000018 RID: 24
public class SceneLoader : MonoBehaviour
{
	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000055 RID: 85 RVA: 0x00003458 File Offset: 0x00001658
	public static SceneLoader Instance
	{
		get
		{
			if (SceneLoader._instance == null)
			{
				SceneLoader._instance = Object.FindObjectOfType<SceneLoader>();
				if (SceneLoader._instance == null)
				{
					SceneLoader._instance = new GameObject(typeof(SceneLoader).Name).AddComponent<SceneLoader>();
				}
			}
			return SceneLoader._instance;
		}
	}

	// Token: 0x06000056 RID: 86 RVA: 0x000034AC File Offset: 0x000016AC
	public void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	// Token: 0x0400003D RID: 61
	private static SceneLoader _instance;
}
