using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TransfromSceneBtn : MonoBehaviour
{
	private void Start()
	{
		this._backBtn = base.GetComponent<Button>();
		this._backBtn.onClick.AsObservable().Subscribe(delegate(Unit _)
		{
			SceneLoader.Instance.LoadScene(this.targetSceneName);
		});
	}

	private Button _backBtn;

	[SerializeField]
	private string targetSceneName;
}
