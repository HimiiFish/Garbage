using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
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
			SceneLoader.Instance.LoadScene("Menu");
		});
	}

	public ShipController shipController;

	public TextMeshProUGUI moneyText;

	public Button close;
}
