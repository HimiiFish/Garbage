using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200001F RID: 31
public class ClosePanelBtn : MonoBehaviour
{
	// Token: 0x0600007D RID: 125 RVA: 0x000042AB File Offset: 0x000024AB
	private void Start()
	{
		this._panel = base.transform.parent.gameObject;
		base.GetComponent<Button>().onClick.AddListener(delegate()
		{
			this._panel.SetActive(false);
		});
	}

	// Token: 0x0400006B RID: 107
	private GameObject _panel;
}
