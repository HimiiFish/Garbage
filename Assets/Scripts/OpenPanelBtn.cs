using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000021 RID: 33
public class OpenPanelBtn : MonoBehaviour
{
	// Token: 0x06000082 RID: 130 RVA: 0x00004304 File Offset: 0x00002504
	private void Start()
	{
		base.GetComponent<Button>().onClick.AddListener(delegate()
		{
			this._panel.SetActive(true);
		});
	}

	// Token: 0x0400006C RID: 108
	[SerializeField]
	private GameObject _panel;
}
