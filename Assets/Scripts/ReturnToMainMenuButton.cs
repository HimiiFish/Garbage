using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReturnToMainMenuButton : MonoBehaviour
{
	[SerializeField]
	private string mainMenuSceneName = "Menu";

	private void Awake()
	{
		gameObject.GetComponent<Button>().onClick.AddListener(this.ReturnToMainMenu);
	}

	public void ReturnToMainMenu()
	{
		Time.timeScale = 1f;
		SceneLoader.Instance.LoadScene(this.mainMenuSceneName);
	}
}
