using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000022 RID: 34
public class TransfromSceneBtn : MonoBehaviour
{
	// Token: 0x06000085 RID: 133 RVA: 0x00004338 File Offset: 0x00002538
	private void Start()
	{
		this._backBtn = base.GetComponent<Button>();
		this._backBtn.onClick.AsObservable().Subscribe(delegate(Unit _)
		{
			SceneLoader.Instance.LoadScene(this.targetSceneName);
		});
	}

	// Token: 0x0400006D RID: 109
	private Button _backBtn;

	// Token: 0x0400006E RID: 110
	[SerializeField]
	private string targetSceneName;
}
