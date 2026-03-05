using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000004 RID: 4
public class CourseBtn : MonoBehaviour
{
	// Token: 0x06000006 RID: 6 RVA: 0x000020CD File Offset: 0x000002CD
	private void Start()
	{
		this.btn.onClick.AddListener(delegate()
		{
			this.course.gameObject.SetActive(true);
		});
	}

	// Token: 0x04000002 RID: 2
	public Button btn;

	// Token: 0x04000003 RID: 3
	public GameObject course;
}
