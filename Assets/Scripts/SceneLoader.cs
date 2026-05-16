using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class SceneLoader : MonoBehaviour
{
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

	public void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	private static SceneLoader _instance;
}
