using System;
using UnityEngine;
using UnityEngine.UI;

public class CoursePanel : MonoBehaviour
{
	private void OnEnable()
	{
		this.btn1.gameObject.SetActive(true);
		this.btn2.gameObject.SetActive(true);
		this.btn3.gameObject.SetActive(true);
		this.btn1.onClick.AddListener(delegate()
		{
			this.btn1.gameObject.SetActive(false);
		});
		this.btn2.onClick.AddListener(delegate()
		{
			this.btn2.gameObject.SetActive(false);
		});
		this.btn3.onClick.AddListener(delegate()
		{
			this.btn3.gameObject.SetActive(false);
			base.gameObject.SetActive(false);
		});
	}

	public Button btn1;

	public Button btn2;

	public Button btn3;
}
