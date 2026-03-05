using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000006 RID: 6
public class GameOverPanel : MonoBehaviour
{
	// Token: 0x0600000E RID: 14 RVA: 0x000021EC File Offset: 0x000003EC
	private void Start()
	{
		this.moneyText.text = this.shipController.money.ToString();
		MessageBroker.Default.Receive<GameOverMessage>().Subscribe(delegate(GameOverMessage _)
		{
			base.GetComponent<CanvasGroup>().alpha = 1f;
			Time.timeScale = 0f;
		}).AddTo(this);
		this.close.onClick.AddListener(delegate()
		{
			Time.timeScale = 1f;
			SceneLoader.Instance.LoadScene("Main");
		});
	}

	// Token: 0x04000007 RID: 7
	public ShipController shipController;

	// Token: 0x04000008 RID: 8
	public TextMeshProUGUI moneyText;

	// Token: 0x04000009 RID: 9
	public Button close;
}
