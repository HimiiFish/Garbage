using System;
using UnityEngine;
using UnityEngine.UI;

public class CourseBtn : MonoBehaviour
{
	private void Start()
	{
		this.btn.onClick.AddListener(delegate()
		{
			this.course.gameObject.SetActive(true);
		});
	}

	public Button btn;

	public GameObject course;
}
