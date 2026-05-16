using System;
using UnityEngine;
using UnityEngine.UI;

public class OpenPanelBtn : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Button>().onClick.AddListener(delegate()
		{
			this._panel.SetActive(true);
		});
	}

	[SerializeField]
	private GameObject _panel;
}
