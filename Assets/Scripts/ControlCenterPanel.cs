using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControlCenterPanel : MonoBehaviour
{
	private void Start()
	{
		this.startButton.onClick.AddListener(new UnityAction(this.StartGame));
		MessageBroker.Default.Receive<StartGameMessage>().Subscribe(delegate(StartGameMessage _)
		{
			this.startButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/主界面+控制台+结局（缺少标题）/控制台/开关on");
		}).AddTo(this);
	}

	private void StartGame()
	{
		MessageBroker.Default.Publish<StartGameMessage>(new StartGameMessage());
	}

	public Button startButton;
}
