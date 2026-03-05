using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000003 RID: 3
public class ControlCenterPanel : MonoBehaviour
{
	// Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
	private void Start()
	{
		this.startButton.onClick.AddListener(new UnityAction(this.StartGame));
		MessageBroker.Default.Receive<StartGameMessage>().Subscribe(delegate(StartGameMessage _)
		{
			this.startButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/主界面+控制台+结局（缺少标题）/控制台/开关on");
		}).AddTo(this);
	}

	// Token: 0x06000003 RID: 3 RVA: 0x00002098 File Offset: 0x00000298
	private void StartGame()
	{
		MessageBroker.Default.Publish<StartGameMessage>(new StartGameMessage());
	}

	// Token: 0x04000001 RID: 1
	public Button startButton;
}
