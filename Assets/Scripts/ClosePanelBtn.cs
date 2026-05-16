using System;
using UnityEngine;
using UnityEngine.UI;

public class ClosePanelBtn : MonoBehaviour
{
	private void Start()
	{
		this._panel = base.transform.parent.gameObject;
		base.GetComponent<Button>().onClick.AddListener(delegate()
		{
			this._panel.SetActive(false);
		});
	}

	private GameObject _panel;
}
